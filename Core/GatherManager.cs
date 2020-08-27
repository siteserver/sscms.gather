using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SSCMS.Enums;
using SSCMS.Gather.Abstractions;
using SSCMS.Gather.Models;
using SSCMS.Models;
using SSCMS.Repositories;
using SSCMS.Services;
using SSCMS.Utils;
using Config = SSCMS.Gather.Models.Config;

namespace SSCMS.Gather.Core
{
    public class GatherManager : IGatherManager
    {
        public const string PluginId = "sscms.gather";

        public const string StatusProgress = "progress";
        public const string StatusSuccess = "success";
        public const string StatusFailure = "failure";

        private readonly IPathManager _pathManager;
        private readonly ICacheManager<ProgressCache> _cacheManager;
        private readonly ITaskManager _taskManager;
        private readonly ISiteRepository _siteRepository;
        private readonly IChannelRepository _channelRepository;
        private readonly IContentRepository _contentRepository;
        private readonly IPluginConfigRepository _pluginConfigRepository;
        private readonly IRuleRepository _ruleRepository;

        public GatherManager(IPathManager pathManager, ICacheManager<ProgressCache> cacheManager, ITaskManager taskManager, ISiteRepository siteRepository, IChannelRepository channelRepository, IContentRepository contentRepository, IPluginConfigRepository pluginConfigRepository, IRuleRepository ruleRepository)
        {
            _pathManager = pathManager;
            _cacheManager = cacheManager;
            _taskManager = taskManager;
            _siteRepository = siteRepository;
            _channelRepository = channelRepository;
            _contentRepository = contentRepository;
            _pluginConfigRepository = pluginConfigRepository;
            _ruleRepository = ruleRepository;
        }

        public const string PermissionsAdd = "gather_add";
        public const string PermissionsList = "gather_list";

        public async Task<Config> GetConfigAsync(int siteId)
        {
            return await _pluginConfigRepository.GetConfigAsync<Config>(PluginId, siteId) ?? new Config();
        }

        public async Task<bool> SetConfigAsync(int siteId, Config config)
        {
            return await _pluginConfigRepository.SetConfigAsync(PluginId, siteId, config);
        }

        public ProgressCache InitCache(string guid, string message)
        {
            if (string.IsNullOrEmpty(guid)) return null;

            var cache = new ProgressCache
            {
                Status = StatusProgress,
                IsSuccess = true,
                Message = message,
                FailureMessages = new List<string>()
            };
            _cacheManager.AddOrUpdateSliding(guid, cache, 60);
            return cache;
        }

        public ProgressCache GetCache(string guid)
        {
            return _cacheManager.Get(guid);
        }

        public void Start(int adminId, int siteId, int ruleId, string guid, bool isCli)
        {
            _taskManager.Queue(async _ => { await GatherChannelsAsync(adminId, siteId, ruleId, guid, isCli); });
        }

        public string Single(int adminId, int siteId, int ruleId, int channelId, List<string> urls)
        {
            var guid = StringUtils.Guid();
            _taskManager.Queue(async _ => { await GatherContentsAsync(adminId, siteId, ruleId, channelId, guid, urls); });
            return guid;
        }

        public async Task GatherChannelsAsync(int adminId, int siteId, int ruleId, string guid, bool isCli)
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

            var regexUrlInclude = GatherUtils.GetRegexString(rule.UrlInclude);
            var regexTitleInclude = GatherUtils.GetRegexString(rule.TitleInclude);
            var regexContentExclude = GatherUtils.GetRegexString(rule.ContentExclude);
            var regexListArea = GatherUtils.GetRegexArea(rule.ListAreaStart, rule.ListAreaEnd);
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
            var attributesDict = TranslateUtils.JsonDeserialize<Dictionary<string, string>>(rule.ContentAttributesXml);

            var contentUrls = GatherUtils.GetContentUrlList(rule, regexListArea, regexUrlInclude, cache);

            cache.TotalCount = rule.GatherNum > 0 ? rule.GatherNum : contentUrls.Count;
            cache.IsSuccess = true;
            cache.Message = "开始采集内容...";
            //if (isCli) await CliUtils.PrintLine(cache.Message);

            var channelIdAndContentIdList = new List<KeyValuePair<int, int>>();

            foreach (var contentUrl in contentUrls)
            {
                var result = await GatherOneAsync(siteInfo, channelInfo, rule.IsSaveImage,
                    rule.IsSetFirstImageAsImageUrl, rule.IsSaveFiles, rule.IsEmptyContentAllowed,
                    rule.IsSameTitleAllowed, rule.IsChecked, rule.Charset, contentUrl, rule.CookieString,
                    regexTitleInclude, regexContentExclude, rule.ContentHtmlClearCollection,
                    rule.ContentHtmlClearTagCollection, rule.ContentReplaceFrom, rule.ContentReplaceTo, regexTitle,
                    regexContent, regexContent2, regexContent3, regexNextPage, regexChannel, contentAttributes,
                    attributesDict, channelIdAndContentIdList, adminId);
                if (result.Success)
                {
                    cache.SuccessCount++;
                    cache.IsSuccess = true;
                    cache.Message = $"采集成功：{result.Title}";
                    //if (isCli) await CliUtils.PrintLine(cache.Message);
                }
                else
                {
                    cache.FailureCount++;
                    cache.IsSuccess = false;
                    cache.Message = result.ErrorMessage;
                    //if (isCli) await CliUtils.PrintErrorAsync($"采集失败：{errorMessage}");
                    cache.FailureMessages.Add(result.ErrorMessage);
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
            //if (isCli) await CliUtils.PrintLine(cache.Message);
        }

        public async Task GatherContentsAsync(int adminId, int siteId, int ruleId, int channelId, string guid, List<string> urls)
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
            var attributesDict = TranslateUtils.JsonDeserialize<Dictionary<string, string>>(rule.ContentAttributesXml);

            cache.TotalCount = rule.GatherNum > 0 ? rule.GatherNum : urls.Count;
            cache.IsSuccess = true;
            cache.Message = "开始采集内容...";

            var channelIdAndContentIdList = new List<KeyValuePair<int, int>>();

            foreach (var contentUrl in urls)
            {
                var result = await GatherOneAsync(siteInfo, channelInfo, rule.IsSaveImage,
                    rule.IsSetFirstImageAsImageUrl, rule.IsSaveFiles, rule.IsEmptyContentAllowed,
                    rule.IsSameTitleAllowed, rule.IsChecked, rule.Charset, contentUrl, rule.CookieString,
                    regexTitleInclude, regexContentExclude, rule.ContentHtmlClearCollection,
                    rule.ContentHtmlClearTagCollection, rule.ContentReplaceFrom, rule.ContentReplaceTo, regexTitle,
                    regexContent, regexContent2, regexContent3, regexNextPage, regexChannel, contentAttributes,
                    attributesDict, channelIdAndContentIdList, adminId);
                if (result.Success)
                {
                    cache.SuccessCount++;
                    cache.IsSuccess = true;
                    cache.Message = $"采集成功：{result.Title}";
                }
                else
                {
                    cache.FailureCount++;
                    cache.IsSuccess = false;
                    cache.Message = result.ErrorMessage;
                    cache.FailureMessages.Add(result.ErrorMessage);
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
        }

        private async Task<(bool Success, string Title, string ErrorMessage)> GatherOneAsync(Site siteInfo, Channel channelInfo, bool isSaveImage, bool isSetFirstImageAsImageUrl, bool isSaveFiles, bool isEmptyContentAllowed, bool isSameTitleAllowed, bool isChecked, Charset charset, string url, string cookieString, string regexTitleInclude, string regexContentExclude, string contentHtmlClearCollection, string contentHtmlClearTagCollection, string contentReplaceFrom, string contentReplaceTo, string regexTitle, string regexContent, string regexContent2, string regexContent3, string regexNextPage, string regexChannel, IEnumerable<string> contentAttributes, Dictionary<string, string> attributesDict, ICollection<KeyValuePair<int, int>> channelIdAndContentIdList, int adminId)
        {
            try
            {
                //TODO:采集文件、链接标题为内容标题、链接提示为内容标题
                //string extension = PathUtils.GetExtension(url);
                //if (!EFileSystemTypeUtils.IsTextEditable(extension))
                //{
                //    if (EFileSystemTypeUtils.IsImageOrFlashOrPlayer(extension))
                //    {

                //    }
                //}
                if (!WebClientUtils.GetRemoteHtml(url, charset, cookieString, out var contentHtml, out var errorMessage))
                {
                    return (false, string.Empty, errorMessage);
                }
                var title = GatherUtils.GetContent("title", regexTitle, contentHtml);
                var content = GatherUtils.GetContent("content", regexContent, contentHtml);
                if (string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(regexContent2))
                {
                    content = GatherUtils.GetContent("content", regexContent2, contentHtml);
                }
                if (string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(regexContent3))
                {
                    content = GatherUtils.GetContent("content", regexContent3, contentHtml);
                }

                //如果标题或内容为空，返回false并退出
                if (string.IsNullOrEmpty(title))
                {
                    errorMessage = $"无法获取标题：{url}";
                    return (false, title, errorMessage);
                }
                if (isEmptyContentAllowed == false && string.IsNullOrEmpty(content))
                {
                    errorMessage = $"无法获取内容正文：{url}";
                    return (false, title, errorMessage);
                }

                title = StringUtils.StripTags(title);

                if (!string.IsNullOrEmpty(regexTitleInclude))
                {
                    if (GatherUtils.IsMatch(regexTitleInclude, title) == false)
                    {
                        errorMessage = $"标题不符合要求：{url}";
                        return (false, title, errorMessage);
                    }
                }
                if (!string.IsNullOrEmpty(regexContentExclude))
                {
                    content = GatherUtils.Replace(regexContentExclude, content, string.Empty);
                }
                if (!string.IsNullOrEmpty(contentHtmlClearCollection))
                {
                    var htmlClearList = GatherUtils.StringCollectionToList(contentHtmlClearCollection);
                    foreach (var htmlClear in htmlClearList)
                    {
                        var clearRegex = $@"<{htmlClear}[^>]*>.*?<\/{htmlClear}>";
                        content = GatherUtils.Replace(clearRegex, content, string.Empty);
                    }
                }
                if (!string.IsNullOrEmpty(contentHtmlClearTagCollection))
                {
                    var htmlClearTagList = GatherUtils.StringCollectionToList(contentHtmlClearTagCollection);
                    foreach (var htmlClearTag in htmlClearTagList)
                    {
                        var clearRegex = $@"<{htmlClearTag}[^>]*>";
                        content = GatherUtils.Replace(clearRegex, content, string.Empty);
                        clearRegex = $@"<\/{htmlClearTag}>";
                        content = GatherUtils.Replace(clearRegex, content, string.Empty);
                    }
                }

                if (!string.IsNullOrEmpty(contentReplaceFrom))
                {
                    var fromList = ListUtils.GetStringList(contentReplaceFrom);
                    var isMulti = false;
                    if (!string.IsNullOrEmpty(contentReplaceTo) && contentReplaceTo.IndexOf(value: ',') != -1)
                    {
                        if (StringUtils.GetCount(",", contentReplaceTo) + 1 == fromList.Count)
                        {
                            isMulti = true;
                        }
                    }
                    if (isMulti == false)
                    {
                        foreach (var from in fromList)
                        {
                            title = GatherUtils.Replace($"({from.Replace(" ", "\\s")})(?!</a>)(?![^><]*>)", title, contentReplaceTo);
                            content = GatherUtils.Replace($"({from.Replace(" ", "\\s")})(?!</a>)(?![^><]*>)", content, contentReplaceTo);
                        }
                    }
                    else
                    {
                        var tos = ListUtils.GetStringList(contentReplaceTo);
                        for (var i = 0; i < fromList.Count; i++)
                        {
                            title = GatherUtils.Replace($"({fromList[i].Replace(" ", "\\s")})(?!</a>)(?![^><]*>)", title, tos[i]);
                            content = GatherUtils.Replace($"({fromList[i].Replace(" ", "\\s")})(?!</a>)(?![^><]*>)", content, tos[i]);
                        }
                    }
                }

                var contentNextPageUrl = GatherUtils.GetUrl(regexNextPage, contentHtml, url);
                if (!string.IsNullOrEmpty(contentNextPageUrl))
                {
                    try
                    {
                        content = GatherUtils.GetPageContent(content, charset, contentNextPageUrl, cookieString, regexContentExclude, contentHtmlClearCollection, contentHtmlClearTagCollection, regexContent, regexContent2, regexContent3, regexNextPage);
                    }
                    catch (Exception ex)
                    {
                        errorMessage = ex.Message;
                        return (false, title, errorMessage);
                    }
                }

                var channel = GatherUtils.GetContent("channel", regexChannel, contentHtml);
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
                            SiteId = siteInfo.Id
                        };

                        newChannelInfo.ParentId = channelInfo.Id;
                        newChannelInfo.ChannelName = channel;
                        newChannelInfo.ContentModelPluginId = channelInfo.ContentModelPluginId;

                        channelId = await _channelRepository.InsertAsync(newChannelInfo);
                    }
                    else
                    {
                        channelId = channelIdByNodeName;
                    }
                }

                if (!isSameTitleAllowed)
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
                    SiteId = siteInfo.Id,
                    ChannelId = channelId,
                    AdminId = adminId,
                    AddDate = DateTime.Now,
                    LastEditAdminId = adminId,
                    Checked = isChecked,
                    CheckedLevel = 0,
                    Title = title
                };

                foreach (var attributeName in contentAttributes)
                {
                    if (!StringUtils.EqualsIgnoreCase(attributeName, nameof(Content.Title)) && !StringUtils.EqualsIgnoreCase(attributeName, nameof(Content.Body)))
                    {
                        var normalStart = GatherUtils.GetStartValue(attributesDict, attributeName);
                        var normalEnd = GatherUtils.GetEndValue(attributesDict, attributeName);

                        //采集为空时的默认值
                        var normalDefault = GatherUtils.GetDefaultValue(attributesDict, attributeName);

                        var regex = GatherUtils.GetRegexAttributeName(attributeName, normalStart, normalEnd);
                        var value = GatherUtils.GetContent(attributeName, regex, contentHtml);

                        //采集为空时的默认值
                        if (string.IsNullOrEmpty(value))
                        {
                            value = normalDefault;
                        }

                        if (StringUtils.EqualsIgnoreCase(nameof(Content.AddDate), attributeName))
                        {
                            value = StringUtils.ReplaceFirst("：", value, ":");
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
                                var attachmentUrl = GatherUtils.GetUrlByBaseUrl(value, url);

                                var fileExtension = PageUtils.GetExtensionFromUrl(attachmentUrl);
                                var fileName =
                                    $"{StringUtils.GetShortGuid(false)}{fileExtension}";

                                var directoryPath = await _pathManager.GetUploadDirectoryPathAsync(siteInfo, UploadType.Image);
                                var filePath = PathUtils.Combine(directoryPath, fileName);
                                DirectoryUtils.CreateDirectoryIfNotExists(filePath);
                                try
                                {
                                    WebClientUtils.SaveRemoteFileToLocal(attachmentUrl, filePath);
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
                                var attachmentUrl = GatherUtils.GetUrlByBaseUrl(value, url);
                                var fileExtension = PageUtils.GetExtensionFromUrl(attachmentUrl);
                                var fileName = $"{StringUtils.GetShortGuid(false)}{fileExtension}";

                                var directoryPath = await _pathManager.GetUploadDirectoryPathAsync(siteInfo, UploadType.Video);
                                var filePath = PathUtils.Combine(directoryPath, fileName);
                                DirectoryUtils.CreateDirectoryIfNotExists(filePath);
                                try
                                {
                                    WebClientUtils.SaveRemoteFileToLocal(attachmentUrl, filePath);
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
                                var attachmentUrl = GatherUtils.GetUrlByBaseUrl(value, url);
                                var fileExtension = PageUtils.GetExtensionFromUrl(attachmentUrl);
                                var fileName = $"{StringUtils.GetShortGuid(false)}{fileExtension}";

                                var directoryPath = await _pathManager.GetUploadDirectoryPathAsync(siteInfo, UploadType.File);
                                var filePath = PathUtils.Combine(directoryPath, fileName);
                                DirectoryUtils.CreateDirectoryIfNotExists(filePath);
                                try
                                {
                                    WebClientUtils.SaveRemoteFileToLocal(attachmentUrl, filePath);
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
                        else
                        {
                            contentInfo.Set(attributeName, value);
                        }
                    }
                }

                var firstImageUrl = string.Empty;
                if (isSaveImage)
                {
                    var originalImageSrcList = GatherUtils.GetOriginalImageSrcList(content);
                    var imageSrcList = GatherUtils.GetImageSrcList(url, content);
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
                                WebClientUtils.SaveRemoteFileToLocal(imageSrc, filePath);
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
                else if (isSetFirstImageAsImageUrl)
                {
                    var imageSrcList = GatherUtils.GetImageSrcList(url, content);
                    if (imageSrcList.Count > 0)
                    {
                        firstImageUrl = imageSrcList[index: 0];
                    }
                }

                if (isSetFirstImageAsImageUrl && string.IsNullOrEmpty(contentInfo.ImageUrl))
                {
                    contentInfo.ImageUrl = firstImageUrl;
                }

                if (isSaveFiles)
                {
                    var originalLinkHrefList = GatherUtils.GetOriginalLinkHrefList(content);
                    var linkHrefList = GatherUtils.GetLinkHrefList(url, content);
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
                                WebClientUtils.SaveRemoteFileToLocal(linkHref, filePath);
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
    }
}
