﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SSCMS.Enums;
using SSCMS.Gather.Abstractions;
using SSCMS.Gather.Models;
using SSCMS.Models;
using SSCMS.Repositories;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Gather.Core
{
    public class GatherManager : IGatherManager
    {
        public const string TaskType = "Gather";
        public const string PermissionsAdd = "gather_add";
        public const string PermissionsList = "gather_list";
        public const string PermissionsTasks = "gather_tasks";

        private const string StatusProgress = "progress";
        private const string StatusSuccess = "success";

        private readonly IPathManager _pathManager;
        private readonly ICacheManager _cacheManager;
        private readonly ITaskManager _taskManager;
        private readonly ICreateManager _createManager;
        private readonly ISiteRepository _siteRepository;
        private readonly IChannelRepository _channelRepository;
        private readonly IContentRepository _contentRepository;
        private readonly IRuleRepository _ruleRepository;

        public GatherManager(IPathManager pathManager, ICacheManager cacheManager, ITaskManager taskManager, ICreateManager createManager, ISiteRepository siteRepository, IChannelRepository channelRepository, IContentRepository contentRepository, IRuleRepository ruleRepository)
        {
            _pathManager = pathManager;
            _cacheManager = cacheManager;
            _taskManager = taskManager;
            _createManager = createManager;
            _siteRepository = siteRepository;
            _channelRepository = channelRepository;
            _contentRepository = contentRepository;
            _ruleRepository = ruleRepository;
        }

        private ProgressCache InitCache(string guid, string message)
        {
            if (string.IsNullOrEmpty(guid)) return null;

            var cache = new ProgressCache
            {
                Status = StatusProgress,
                IsSuccess = true,
                Message = message,
                FailureMessages = new List<string>()
            };
            _cacheManager.AddOrUpdateSliding(guid, TranslateUtils.JsonSerialize(cache), 60);
            return cache;
        }

        private void SetCache(string guid, ProgressCache cache)
        {
            _cacheManager.AddOrUpdateSliding(guid, TranslateUtils.JsonSerialize(cache), 60);
        }

        public ProgressCache GetCache(string guid)
        {
            var objStr = _cacheManager.Get<string>(guid);
            var cache = TranslateUtils.JsonDeserialize<ProgressCache>(objStr);
            return cache;
        }

        public void Start(int adminId, int siteId, int ruleId, string guid)
        {
            _taskManager.Queue(async _ => { await GatherChannelsAsync(adminId, siteId, ruleId, guid); });
        }

        public string Single(int adminId, int siteId, int ruleId, int channelId, List<Item> items)
        {
            var guid = StringUtils.Guid();
            _taskManager.Queue(async _ =>
            {
                await GatherContentsAsync(adminId, siteId, ruleId, channelId, guid, items);
            });
            return guid;
        }

        public async Task GatherChannelsAsync(int adminId, int siteId, int ruleId, string guid)
        {
            var cache = InitCache(guid, "开始获取链接...");
            //if (isCli) await CliUtils.PrintLine(cache.Message);

            var rule = await _ruleRepository.GetAsync(ruleId);
            var siteInfo = await _siteRepository.GetAsync(siteId);
            var channelInfo = await _channelRepository.GetAsync(rule.ChannelId);
            if (channelInfo == null)
            {
                channelInfo = await _channelRepository.GetAsync(siteId);
                rule.ChannelId = siteId;
            }

            var regexTitleInclude = GatherUtils.GetRegexString(rule.TitleInclude);
            var regexContentExclude = GatherUtils.GetRegexString(rule.ContentExclude);
            var regexChannel = GatherUtils.GetRegexChannel(rule.ContentChannelStart, rule.ContentChannelEnd);
            var regexContent = GatherUtils.GetRegexContent(rule.ContentContentStart, rule.ContentContentEnd);
            var regexContent2 = string.Empty;
            if (!string.IsNullOrEmpty(rule.ContentContentStart2) && !string.IsNullOrEmpty(rule.ContentContentEnd2))
            {
                regexContent2 = GatherUtils.GetRegexContent(rule.ContentContentStart2, rule.ContentContentEnd2);
            }
            var regexContent3 = string.Empty;
            if (!string.IsNullOrEmpty(rule.ContentContentStart3) && !string.IsNullOrEmpty(rule.ContentContentEnd3))
            {
                regexContent3 = GatherUtils.GetRegexContent(rule.ContentContentStart3, rule.ContentContentEnd3);
            }
            var regexNextPage = GatherUtils.GetRegexUrl(rule.ContentNextPageStart, rule.ContentNextPageEnd);
            var regexTitle = GatherUtils.GetRegexTitle(rule.ContentTitleStart, rule.ContentTitleEnd);
            var contentAttributes = ListUtils.GetStringList(rule.ContentAttributes);

            var items = await GatherUtils.GetAllItemsAsync(rule, cache);

            cache.TotalCount = rule.GatherNum > 0 ? rule.GatherNum : items.Count;
            cache.IsSuccess = true;
            cache.Message = "开始采集内容...";
            SetCache(guid, cache);
            //if (isCli) await CliUtils.PrintLine(cache.Message);

            var channelIdAndContentIdList = new List<KeyValuePair<int, int>>();

            foreach (var item in items)
            {
                var result = await GatherOneAsync(siteInfo, channelInfo,
                    regexTitleInclude, regexContentExclude, regexTitle, regexContent, regexContent2, regexContent3, regexNextPage, regexChannel,
                    contentAttributes,
                    rule, item, channelIdAndContentIdList, adminId);
                if (result.Success)
                {
                    cache.SuccessCount++;
                    cache.IsSuccess = true;
                    cache.Message = $"采集成功：{result.Title}";
                    SetCache(guid, cache);
                    //if (isCli) await CliUtils.PrintLine(cache.Message);
                }
                else
                {
                    cache.FailureCount++;
                    cache.IsSuccess = false;
                    cache.Message = result.ErrorMessage;
                    //if (isCli) await CliUtils.PrintErrorAsync($"采集失败：{errorMessage}");
                    cache.FailureMessages.Add(result.ErrorMessage);
                    SetCache(guid, cache);
                }
                if (cache.SuccessCount == cache.TotalCount) break;
            }

            if (rule.IsChecked)
            {
                foreach (var channelIdAndContentId in channelIdAndContentIdList)
                {
                    var channelId = channelIdAndContentId.Key;
                    var contentId = channelIdAndContentId.Value;

                    await _createManager.ExecuteAsync(siteId, CreateType.Content, channelId, contentId);
                }
                var channelIds = channelIdAndContentIdList.Select(x => x.Key).Distinct().ToList();
                foreach (var channelId in channelIds)
                {
                    await _createManager.ExecuteAsync(siteId, CreateType.Channel, channelId);
                }
            }

            await _ruleRepository.UpdateLastGatherDateAsync(ruleId);

            cache.Status = StatusSuccess;
            cache.IsSuccess = true;
            cache.Message = $"任务完成，共采集内容 {cache.SuccessCount} 篇。";
            SetCache(guid, cache);
            //if (isCli) await CliUtils.PrintLine(cache.Message);
        }

        private async Task GatherContentsAsync(int adminId, int siteId, int ruleId, int channelId, string guid, List<Item> items)
        {
            var cache = InitCache(guid, "开始获取链接...");

            var rule = await _ruleRepository.GetAsync(ruleId);
            var siteInfo = await _siteRepository.GetAsync(siteId);
            var channelInfo = await _channelRepository.GetAsync(channelId) ?? await _channelRepository.GetAsync(siteId);

            var regexTitleInclude = GatherUtils.GetRegexString(rule.TitleInclude);
            var regexContentExclude = GatherUtils.GetRegexString(rule.ContentExclude);
            var regexChannel = GatherUtils.GetRegexChannel(rule.ContentChannelStart, rule.ContentChannelEnd);
            var regexContent = GatherUtils.GetRegexContent(rule.ContentContentStart, rule.ContentContentEnd);
            var regexContent2 = string.Empty;
            if (!string.IsNullOrEmpty(rule.ContentContentStart2) && !string.IsNullOrEmpty(rule.ContentContentEnd2))
            {
                regexContent2 = GatherUtils.GetRegexContent(rule.ContentContentStart2, rule.ContentContentEnd2);
            }
            var regexContent3 = string.Empty;
            if (!string.IsNullOrEmpty(rule.ContentContentStart3) && !string.IsNullOrEmpty(rule.ContentContentEnd3))
            {
                regexContent3 = GatherUtils.GetRegexContent(rule.ContentContentStart3, rule.ContentContentEnd3);
            }
            var regexNextPage = GatherUtils.GetRegexUrl(rule.ContentNextPageStart, rule.ContentNextPageEnd);
            var regexTitle = GatherUtils.GetRegexTitle(rule.ContentTitleStart, rule.ContentTitleEnd);
            var contentAttributes = ListUtils.GetStringList(rule.ContentAttributes);

            cache.TotalCount = rule.GatherNum > 0 ? rule.GatherNum : items.Count;
            cache.IsSuccess = true;
            cache.Message = "开始采集内容...";
            SetCache(guid, cache);

            var channelIdAndContentIdList = new List<KeyValuePair<int, int>>();

            foreach (var item in items)
            {
                var result = await GatherOneAsync(siteInfo, channelInfo,
                    regexTitleInclude, regexContentExclude, regexTitle,
                    regexContent, regexContent2, regexContent3, regexNextPage, regexChannel, contentAttributes,
                    rule, item, channelIdAndContentIdList, adminId);
                if (result.Success)
                {
                    cache.SuccessCount++;
                    cache.IsSuccess = true;
                    cache.Message = $"采集成功：{result.Title}";
                    SetCache(guid, cache);
                }
                else
                {
                    cache.FailureCount++;
                    cache.IsSuccess = false;
                    cache.Message = result.ErrorMessage;
                    cache.FailureMessages.Add(result.ErrorMessage);
                    SetCache(guid, cache);
                }
                if (cache.SuccessCount == cache.TotalCount) break;
            }

            //if (rule.IsChecked)
            //{
            //    foreach (var channelIdAndContentId in channelIdAndContentIdList)
            //    {
            //        var channelId = channelIdAndContentId.Key;
            //        var contentId = channelIdAndContentId.Value;

            //        CreateManager.CreateContent(siteId, channelId, contentId);
            //    }
            //}

            await _ruleRepository.UpdateLastGatherDateAsync(ruleId);

            cache.Status = StatusSuccess;
            cache.IsSuccess = true;
            cache.Message = $"任务完成，共采集内容 {cache.SuccessCount} 篇。";
            SetCache(guid, cache);
        }

        private async Task<(bool Success, string Title, string ErrorMessage)> GatherOneAsync(Site siteInfo, Channel channelInfo, string regexTitleInclude, string regexContentExclude, string regexTitle, string regexContent, string regexContent2, string regexContent3, string regexNextPage, string regexChannel, IEnumerable<string> contentAttributes, Rule rule, Item item, ICollection<KeyValuePair<int, int>> channelIdAndContentIdList, int adminId)
        {
            try
            {
                var result = await WebClientUtils.GetRemoteHtmlAsync(item.Url, rule.Charset, rule.CookieString);
                if (!result.IsSuccess)
                {
                    return (false, string.Empty, result.ErrorMessage);
                }
                var contentHtml = result.Content;
                var errorMessage = string.Empty;

                var title = rule.ContentTitleByList ? item.Content.Title : GatherUtils.GetValue("title", regexTitle, contentHtml);
                var content = GatherUtils.GetValue("content", regexContent, contentHtml);
                if (string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(regexContent2))
                {
                    content = GatherUtils.GetValue("content", regexContent2, contentHtml);
                }
                if (string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(regexContent3))
                {
                    content = GatherUtils.GetValue("content", regexContent3, contentHtml);
                }

                //如果标题或内容为空，返回false并退出
                if (string.IsNullOrEmpty(title))
                {
                    errorMessage = $"无法获取标题：{item.Url}";
                    return (false, title, errorMessage);
                }
                if (rule.IsEmptyContentAllowed == false && string.IsNullOrEmpty(content))
                {
                    errorMessage = $"无法获取内容正文：{item.Url}";
                    return (false, title, errorMessage);
                }

                title = StringUtils.StripTags(title);

                if (!string.IsNullOrEmpty(regexTitleInclude))
                {
                    if (GatherUtils.IsMatch(regexTitleInclude, title) == false)
                    {
                        errorMessage = $"标题不符合要求：{item.Url}";
                        return (false, title, errorMessage);
                    }
                }
                if (!string.IsNullOrEmpty(regexContentExclude))
                {
                    content = GatherUtils.Replace(regexContentExclude, content, string.Empty);
                }
                if (!string.IsNullOrEmpty(rule.ContentHtmlClearCollection))
                {
                    var htmlClearList = GatherUtils.StringCollectionToList(rule.ContentHtmlClearCollection);
                    foreach (var htmlClear in htmlClearList)
                    {
                        var clearRegex = $@"<{htmlClear}[^>]*>.*?<\/{htmlClear}>";
                        content = GatherUtils.Replace(clearRegex, content, string.Empty);
                    }
                }
                if (!string.IsNullOrEmpty(rule.ContentHtmlClearTagCollection))
                {
                    var htmlClearTagList = GatherUtils.StringCollectionToList(rule.ContentHtmlClearTagCollection);
                    foreach (var htmlClearTag in htmlClearTagList)
                    {
                        var clearRegex = $@"<{htmlClearTag}[^>]*>";
                        content = GatherUtils.Replace(clearRegex, content, string.Empty);
                        clearRegex = $@"<\/{htmlClearTag}>";
                        content = GatherUtils.Replace(clearRegex, content, string.Empty);
                    }
                }

                var contentNextPageUrl = GatherUtils.GetUrl(regexNextPage, contentHtml, item.Url);
                if (!string.IsNullOrEmpty(contentNextPageUrl))
                {
                    try
                    {
                        content = await GatherUtils.GetPageContentAsync(content, rule.Charset, contentNextPageUrl, rule.CookieString, regexContentExclude, rule.ContentHtmlClearCollection, rule.ContentHtmlClearTagCollection, regexContent, regexContent2, regexContent3, regexNextPage);
                    }
                    catch (Exception ex)
                    {
                        errorMessage = ex.Message;
                        return (false, title, errorMessage);
                    }
                }

                var channel = GatherUtils.GetValue("channel", regexChannel, contentHtml);
                var channelId = channelInfo.Id;
                if (!string.IsNullOrEmpty(channel))
                {
                    var channelIdByNodeName = 0;

                    var childChannelIdList = await _channelRepository.GetChannelIdsAsync(siteInfo.Id, channelInfo.Id, ScopeType.All);
                    foreach (var childChannelId in childChannelIdList)
                    {
                        if (channel == await _channelRepository.GetChannelNameAsync(siteInfo.Id, childChannelId))
                        {
                            channelIdByNodeName = childChannelId;
                        }
                    }

                    //var channelIdByNodeName = ChannelManager.GetChannelIdByParentIdAndChannelName(siteInfo.Id, channelInfo.Id, channel, recursive: false);
                    if (channelIdByNodeName == 0)
                    {
                        var newChannelInfo = new Channel
                        {
                            SiteId = siteInfo.Id,
                            ParentId = channelInfo.Id,
                            ChannelName = channel,
                            ContentModelPluginId = channelInfo.ContentModelPluginId
                        };

                        channelId = await _channelRepository.InsertAsync(newChannelInfo);
                    }
                    else
                    {
                        channelId = channelIdByNodeName;
                    }
                }

                if (!rule.IsSameTitleAllowed)
                {
                    var theChannel = await _channelRepository.GetAsync(channelId);
                    var contentIds = await _contentRepository.GetContentIdsBySameTitleAsync(siteInfo, theChannel, title);
                    if (contentIds.Count > 0)
                    {
                        errorMessage = $"已包含相同标题：{title}";
                        return (false, title, errorMessage);
                    }
                }

                var contentInfo = new Content
                {
                    AddDate = DateTime.Now
                };

                foreach (var attributeName in contentAttributes)
                {
                    if (!StringUtils.EqualsIgnoreCase(attributeName, nameof(Content.Title)) && !StringUtils.EqualsIgnoreCase(attributeName, nameof(Content.Body)))
                    {
                        var normalByList = GatherUtils.GetByListValue(rule, attributeName);
                        var normalStart = GatherUtils.GetStartValue(rule, attributeName);
                        var normalEnd = GatherUtils.GetEndValue(rule, attributeName);

                        //采集为空时的默认值
                        var normalDefault = GatherUtils.GetDefaultValue(rule, attributeName);

                        var regex = GatherUtils.GetRegexAttributeName(attributeName, normalStart, normalEnd);
                        var value = normalByList ? item.Content.Get<string>(attributeName) : GatherUtils.GetValue(attributeName, regex, contentHtml);

                        //采集为空时的默认值
                        if (string.IsNullOrEmpty(value))
                        {
                            value = normalDefault;
                        }

                        if (StringUtils.EqualsIgnoreCase(nameof(Content.AddDate), attributeName))
                        {
                            value = GatherUtils.ReplaceFirst(value, "：", ":");
                            contentInfo.AddDate = TranslateUtils.ToDateTime(value, DateTime.Now);
                        }
                        else if (StringUtils.EqualsIgnoreCase(nameof(Content.Color), attributeName))
                        {
                            contentInfo.Color = TranslateUtils.ToBool(value, defaultValue: false);
                        }
                        else if (StringUtils.EqualsIgnoreCase(nameof(Content.Hot), attributeName))
                        {
                            contentInfo.Hot = TranslateUtils.ToBool(value, defaultValue: false);
                        }
                        else if (StringUtils.EqualsIgnoreCase(nameof(Content.Recommend), attributeName))
                        {
                            contentInfo.Recommend = TranslateUtils.ToBool(value, defaultValue: false);
                        }
                        else if (StringUtils.EqualsIgnoreCase(nameof(Content.Top), attributeName))
                        {
                            contentInfo.Top = TranslateUtils.ToBool(value, defaultValue: false);
                        }
                        else if (StringUtils.EqualsIgnoreCase(nameof(Content.ImageUrl), attributeName))
                        {
                            if (!string.IsNullOrEmpty(value))
                            {
                                var attachmentUrl = GatherUtils.GetUrlByBaseUrl(value, item.Url);

                                var fileExtension = PageUtils.GetExtensionFromUrl(attachmentUrl);
                                var fileName =
                                    $"{StringUtils.GetShortGuid(false)}{fileExtension}";

                                var directoryPath = await _pathManager.GetUploadDirectoryPathAsync(siteInfo, UploadType.Image);
                                var filePath = PathUtils.Combine(directoryPath, fileName);
                                DirectoryUtils.CreateDirectoryIfNotExists(filePath);
                                try
                                {
                                    await WebClientUtils.DownloadAsync(attachmentUrl, filePath);
                                    contentInfo.ImageUrl =
                                        await _pathManager.GetVirtualUrlByPhysicalPathAsync(siteInfo, filePath);
                                }
                                catch
                                {
                                    // ignored
                                }
                            }
                        }
                        else if (StringUtils.EqualsIgnoreCase(nameof(Content.VideoUrl), attributeName))
                        {
                            if (!string.IsNullOrEmpty(value))
                            {
                                var attachmentUrl = GatherUtils.GetUrlByBaseUrl(value, item.Url);
                                var fileExtension = PageUtils.GetExtensionFromUrl(attachmentUrl);
                                var fileName = $"{StringUtils.GetShortGuid(false)}{fileExtension}";

                                var directoryPath = await _pathManager.GetUploadDirectoryPathAsync(siteInfo, UploadType.Video);
                                var filePath = PathUtils.Combine(directoryPath, fileName);
                                DirectoryUtils.CreateDirectoryIfNotExists(filePath);
                                try
                                {
                                    await WebClientUtils.DownloadAsync(attachmentUrl, filePath);
                                    contentInfo.VideoUrl = await _pathManager.GetVirtualUrlByPhysicalPathAsync(siteInfo, filePath);
                                }
                                catch
                                {
                                    // ignored
                                }
                            }
                        }
                        else if (StringUtils.EqualsIgnoreCase(nameof(Content.FileUrl), attributeName))
                        {
                            if (!string.IsNullOrEmpty(value))
                            {
                                var attachmentUrl = GatherUtils.GetUrlByBaseUrl(value, item.Url);
                                var fileExtension = PageUtils.GetExtensionFromUrl(attachmentUrl);
                                var fileName = $"{StringUtils.GetShortGuid(false)}{fileExtension}";

                                var directoryPath = await _pathManager.GetUploadDirectoryPathAsync(siteInfo, UploadType.File);
                                var filePath = PathUtils.Combine(directoryPath, fileName);
                                DirectoryUtils.CreateDirectoryIfNotExists(filePath);
                                try
                                {
                                    await WebClientUtils.DownloadAsync(attachmentUrl, filePath);
                                    contentInfo.FileUrl = await _pathManager.GetVirtualUrlByPhysicalPathAsync(siteInfo, filePath);
                                }
                                catch
                                {
                                    // ignored
                                }
                            }
                        }
                        else if (StringUtils.EqualsIgnoreCase(nameof(Content.Hits), attributeName))
                        {
                            contentInfo.Hits = TranslateUtils.ToInt(value);
                        }
                        else if (StringUtils.EqualsIgnoreCase("FileName", attributeName) && !string.IsNullOrEmpty(rule.FileNameAttributeName))
                        {
                            var fileName = PathUtils.GetFileNameWithoutExtension(item.Url);
                            contentInfo.Set(rule.FileNameAttributeName, fileName);
                        }
                        else
                        {
                            contentInfo.Set(attributeName, value);
                        }
                    }
                }

                var firstImageUrl = string.Empty;
                if (rule.IsSaveImage)
                {
                    var originalImageSrcList = GatherUtils.GetOriginalImageSrcList(content);
                    var imageSrcList = GatherUtils.GetImageSrcList(item.Url, content);
                    if (originalImageSrcList.Count == imageSrcList.Count)
                    {
                        for (var i = 0; i < originalImageSrcList.Count; i++)
                        {
                            var originalImageSrc = originalImageSrcList[i];
                            var imageSrc = imageSrcList[i];

                            var fileExtension = PathUtils.GetExtension(originalImageSrc);
                            var fileName = $"{StringUtils.GetShortGuid(false)}{fileExtension}";

                            var directoryPath = await _pathManager.GetUploadDirectoryPathAsync(siteInfo, UploadType.Image);
                            var filePath = PathUtils.Combine(directoryPath, fileName);
                            DirectoryUtils.CreateDirectoryIfNotExists(filePath);
                            try
                            {
                                await WebClientUtils.DownloadAsync(imageSrc, filePath);
                                var fileUrl = await _pathManager.GetVirtualUrlByPhysicalPathAsync(siteInfo, filePath);
                                content = content.Replace(originalImageSrc, fileUrl);
                                if (firstImageUrl == string.Empty)
                                {
                                    firstImageUrl = fileUrl;
                                }
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }
                }

                if (rule.ImageSource == ImageSource.Content)
                {
                    if (string.IsNullOrEmpty(firstImageUrl))
                    {
                        var imageSrcList = GatherUtils.GetImageSrcList(item.Url, content);
                        if (imageSrcList.Count > 0)
                        {
                            firstImageUrl = imageSrcList[index: 0];
                        }
                    }

                    if (!string.IsNullOrEmpty(firstImageUrl))
                    {
                        contentInfo.ImageUrl = firstImageUrl;
                    }
                }
                else if (rule.ImageSource == ImageSource.List)
                {
                    contentInfo.ImageUrl = item.Content.ImageUrl;

                    if (!string.IsNullOrEmpty(contentInfo.ImageUrl) && rule.IsSaveImage)
                    {
                        var originalLinkHref = contentInfo.ImageUrl;
                        var linkHref = GatherUtils.GetUrlByBaseUrl(originalLinkHref, item.GatherUrl);

                        var fileExtension = PathUtils.GetExtension(originalLinkHref);
                        var fileName = $"{StringUtils.GetShortGuid(false)}{fileExtension}";

                        var directoryPath = await _pathManager.GetUploadDirectoryPathAsync(siteInfo, UploadType.File);
                        var filePath = PathUtils.Combine(directoryPath, fileName);
                        DirectoryUtils.CreateDirectoryIfNotExists(filePath);
                        try
                        {
                            await WebClientUtils.DownloadAsync(linkHref, filePath);
                            var fileUrl = await _pathManager.GetVirtualUrlByPhysicalPathAsync(siteInfo, filePath);

                            contentInfo.ImageUrl = fileUrl;
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }

                if (rule.IsSaveFiles)
                {
                    var originalLinkHrefList = GatherUtils.GetOriginalLinkHrefList(content);
                    var linkHrefList = GatherUtils.GetLinkHrefList(item.Url, content);
                    if (originalLinkHrefList.Count == linkHrefList.Count)
                    {
                        for (var i = 0; i < originalLinkHrefList.Count; i++)
                        {
                            var originalLinkHref = originalLinkHrefList[i];
                            var linkHref = linkHrefList[i];

                            var fileExtension = PathUtils.GetExtension(originalLinkHref);
                            var fileName = $"{StringUtils.GetShortGuid(false)}{fileExtension}";

                            var directoryPath = await _pathManager.GetUploadDirectoryPathAsync(siteInfo, UploadType.File);
                            var filePath = PathUtils.Combine(directoryPath, fileName);
                            DirectoryUtils.CreateDirectoryIfNotExists(filePath);
                            try
                            {
                                await WebClientUtils.DownloadAsync(linkHref, filePath);
                                var fileUrl = await _pathManager.GetVirtualUrlByPhysicalPathAsync(siteInfo, filePath);
                                content = content.Replace(originalLinkHref, fileUrl);
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }
                }

                //contentInfo.Content = StringUtility.TextEditorContentEncode(content, siteInfo, false);
                contentInfo.SiteId = siteInfo.Id;
                contentInfo.ChannelId = channelId;
                contentInfo.AdminId = adminId;
                contentInfo.LastEditAdminId = adminId;
                contentInfo.Checked = rule.IsChecked;
                contentInfo.CheckedLevel = 0;
                contentInfo.Title = title;
                contentInfo.Body = content;

                //contentInfo.SourceId = SourceManager.CaiJi;

                var theContentId = await _contentRepository.InsertAsync(siteInfo, channelInfo, contentInfo);
                channelIdAndContentIdList.Add(new KeyValuePair<int, int>(contentInfo.ChannelId, theContentId));

                return (true, title, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, string.Empty, ex.Message);
            }
        }

        public async Task ExportAsync(Rule rule, string filePath)
        {
            await FileUtils.WriteTextAsync(filePath, TranslateUtils.JsonSerialize(rule));
        }

        public async Task ImportAsync(int siteId, string filePath, bool overwrite)
        {
            var json = await FileUtils.ReadTextAsync(filePath);
            var rule = TranslateUtils.JsonDeserialize<Rule>(json);

            if (rule != null)
            {
                rule.SiteId = siteId;
                rule.ChannelId = siteId;
                rule.CreatedDate = DateTime.Now;
                rule.LastModifiedDate = DateTime.Now;
                rule.LastGatherDate = null;

                var srcRule = await _ruleRepository.GetByRuleNameAsync(siteId, rule.RuleName);
                if (srcRule != null)
                {
                    if (overwrite)
                    {
                        await _ruleRepository.DeleteAsync(srcRule.Id);
                    }
                    else
                    {
                        rule.RuleName = await _ruleRepository.GetImportRuleNameAsync(siteId, rule.RuleName);
                    }
                }

                await _ruleRepository.InsertAsync(rule);
            }
        }
    }
}
