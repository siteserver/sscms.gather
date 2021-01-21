using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using SSCMS.Gather.Models;
using SSCMS.Models;
using SSCMS.Utils;

namespace SSCMS.Gather.Core
{
    public static class GatherUtils
    {
        private const string PagePlaceHolder = "[SITESERVER_PAGE]";//内容翻页占位符

        /*
         * 通用：.*?
         * 所有链接：<a\s*.*?href=(?:"(?<url>[^"]*)"|'(?<url>[^']*)'|(?<url>\S+)).*?>
         * */

        private const RegexOptions Options = RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace;

        public static List<string> GetImageSrcList(string baseUrl, string html)
        {
            var regex = "(img|input)[^><]*\\s+src\\s*=\\s*(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>[^>\\s]*))";
            return GetUrls(regex, html, baseUrl);
        }

        public static List<string> GetOriginalImageSrcList(string html)
        {
            var regex = "(img|input)[^><]*\\s+src\\s*=\\s*(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>[^>\\s]*))";
            return GetValues("url", regex, html);
        }

        public static List<string> GetLinkHrefList(string baseUrl, string html)
        {
            var regex = "a[^><]*\\s+href\\s*=\\s*(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>[^>\\s]*))";
            return GetUrls(regex, html, baseUrl);
        }

        public static List<string> GetOriginalLinkHrefList(string html)
        {
            var regex = "a[^><]*\\s+href\\s*=\\s*(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>[^>\\s]*))";
            return GetValues("url", regex, html);
        }

        private static List<string> GetUrls(string regex, string html, string baseUrl)
        {
            var urlList = new List<string>();
            if (string.IsNullOrEmpty(regex))
            {
                regex = "<a\\s*.*?href\\s*=\\s*(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>[^>\\s]*)).*?>";
            }
            var groupName = "url";
            var list = GetValues(groupName, regex, html);
            foreach (var rawUrl in list)
            {
                var url = GetUrlByBaseUrl(rawUrl, baseUrl);
                if (!string.IsNullOrEmpty(url) && !urlList.Contains(url))
                {
                    urlList.Add(url);
                }
            }
            return urlList;
        }

        private static string GetUrlWithoutPathInfo(string rawUrl)
        {
            var urlWithoutPathInfo = string.Empty;
            if (rawUrl != null && rawUrl.Trim().Length > 0)
            {
                var schema = "";
                if (rawUrl.ToLower().Contains("://"))
                {
                    schema = rawUrl.ToLower().Substring(0, length: rawUrl.ToLower().IndexOf("://", StringComparison.Ordinal) + 3);
                    urlWithoutPathInfo = rawUrl.Substring(schema.Length);
                }
                if (urlWithoutPathInfo.IndexOf("/", StringComparison.Ordinal) != -1)
                {
                    urlWithoutPathInfo = urlWithoutPathInfo.Substring(0, urlWithoutPathInfo.IndexOf("/", StringComparison.Ordinal));
                }
                if (string.IsNullOrEmpty(urlWithoutPathInfo))
                {
                    urlWithoutPathInfo = rawUrl;
                }
                urlWithoutPathInfo = schema + urlWithoutPathInfo;
            }
            return urlWithoutPathInfo;
        }

        private static string GetUrlWithoutFileName(string rawUrl)
        {
            if (string.IsNullOrEmpty(rawUrl)) return string.Empty;

            var urlWithoutFileName = string.Empty;
            var schema = "";
            if (rawUrl.ToLower().StartsWith("://"))
            {
                schema = rawUrl.ToLower().Substring(0, length: rawUrl.ToLower().IndexOf("://", StringComparison.Ordinal) + 3);
                urlWithoutFileName = rawUrl.Substring(schema.Length);
            }
            if (urlWithoutFileName.IndexOf("/", StringComparison.Ordinal) != -1 && !urlWithoutFileName.EndsWith("/"))
            {
                const string regex = "/(?<filename>[^/]*\\.[^/]*)[^/]*$";
                const RegexOptions options = RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.IgnoreCase;
                var reg = new Regex(regex, options);
                var match = reg.Match(urlWithoutFileName);
                if (match.Success)
                {
                    var fileName = match.Groups["filename"].Value;
                    urlWithoutFileName = urlWithoutFileName.Substring(0, urlWithoutFileName.LastIndexOf(fileName, StringComparison.Ordinal));
                }
            }
            urlWithoutFileName = schema + urlWithoutFileName;
            return urlWithoutFileName;
        }

        private static string RemoveProtocolFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;

            url = url.Trim();
            return PageUtils.IsProtocolUrl(url) ? url.Substring(url.IndexOf("://", StringComparison.Ordinal) + 3) : url;
        }

        public static string GetUrlByBaseUrl(string rawUrl, string baseUrl)
        {
            var url = string.Empty;
            if (!string.IsNullOrEmpty(rawUrl))
            {
                rawUrl = rawUrl.Trim().TrimEnd('#');
            }
            if (!string.IsNullOrEmpty(baseUrl))
            {
                baseUrl = baseUrl.Trim();
            }
            if (!string.IsNullOrEmpty(rawUrl))
            {
                rawUrl = rawUrl.Trim();
                if (PageUtils.IsProtocolUrl(rawUrl))
                {
                    url = rawUrl;
                }
                else if (rawUrl.StartsWith("/"))
                {
                    var domain = GetUrlWithoutPathInfo(baseUrl);
                    url = domain + rawUrl;
                }
                else if (rawUrl.StartsWith("../"))
                {
                    var count = StringUtils.GetStartCount("../", rawUrl);
                    rawUrl = rawUrl.Remove(0, 3 * count);
                    baseUrl = GetUrlWithoutFileName(baseUrl).TrimEnd('/');
                    baseUrl = RemoveProtocolFromUrl(baseUrl);
                    for (var i = 0; i < count; i++)
                    {
                        var j = baseUrl.LastIndexOf('/');
                        if (j != -1)
                        {
                            baseUrl = StringUtils.Remove(baseUrl, j);
                        }
                        else
                        {
                            break;
                        }
                    }
                    url = PageUtils.Combine(PageUtils.AddProtocolToUrl(baseUrl), rawUrl);
                }
                else
                {
                    if (baseUrl != null && baseUrl.EndsWith("/"))
                    {
                        url = baseUrl + rawUrl;
                    }
                    else
                    {
                        var urlWithoutFileName = GetUrlWithoutFileName(baseUrl);
                        if (!urlWithoutFileName.EndsWith("/"))
                        {
                            urlWithoutFileName += "/";
                        }
                        url = urlWithoutFileName + rawUrl;
                    }
                }
            }
            return url;
        }

        public static string GetUrl(string regex, string html, string baseUrl)
        {
            return GetUrlByBaseUrl(GetValue("url", regex, html), baseUrl);
        }

        public static string GetValue(string groupName, string regex, string html)
        {
            var content = string.Empty;
            if (string.IsNullOrEmpty(regex)) return content;
            if (regex.IndexOf("<" + groupName + ">", StringComparison.Ordinal) == -1)
            {
                return regex;
            }

            var reg = new Regex(regex, Options);
            var match = reg.Match(html);
            if (match.Success)
            {
                content = match.Groups[groupName].Value;
            }

            return content;
        }

        private static List<string> GetValues(string groupName, string regex, string html)
        {
            var list = new List<string>();
            if (string.IsNullOrEmpty(regex)) return list;

            var reg = new Regex(regex, Options);

            for (var match = reg.Match(html); match.Success; match = match.NextMatch())
            {
                //list.Add(match.Groups[groupName].Value);
                var theValue = match.Groups[groupName].Value;
                if (!list.Contains(theValue))
                {
                    list.Add(theValue);
                }
            }
            return list;
        }

        public static string Replace(string regex, string input, string replacement)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var reg = new Regex(regex, Options);
            return reg.Replace(input, replacement);
        }

        public static bool IsMatch(string regex, string input)
        {
            var reg = new Regex(regex, Options);
            return reg.IsMatch(input);
        }
        public static IEnumerable<string> StringCollectionToList(string collection)
        {
            return StringCollectionToList(collection, ',');
        }

        private static List<string> StringCollectionToList(string collection, char separator)
        {
            var list = new List<string>();
            if (string.IsNullOrEmpty(collection)) return list;

            var array = collection.Split(separator);
            foreach (var s in array)
            {
                list.Add(s.Trim());
            }
            return list;
        }

        public static string GetRegexString(string normalString)
        {
            var retVal = normalString;
            if (string.IsNullOrEmpty(normalString)) return retVal;

            var replaceChar = new[] { '\\', '^', '$', '.', '{', '[', '(', ')', ']', '}', '+', '?', '!', '#' };
            foreach (var theChar in replaceChar)
            {
                retVal = retVal.Replace(theChar.ToString(), "\\" + theChar);
            }
            retVal = retVal.Replace("*", ".*?");
            retVal = Replace("\\s+", retVal, "\\s+");
            return retVal;
        }

        private static string GetRegexArea(string normalAreaStart, string normalAreaEnd)
        {
            if (!string.IsNullOrEmpty(normalAreaStart) && !string.IsNullOrEmpty(normalAreaEnd))
            {
                return $"{GetRegexString(normalAreaStart)}\\s*(?<area>[\\s\\S]+?)\\s*{GetRegexString(normalAreaEnd)}";
            }
            return string.Empty;
        }

        public static string GetRegexUrl(string normalUrlStart, string normalUrlEnd)
        {
            if (!string.IsNullOrEmpty(normalUrlStart) && !string.IsNullOrEmpty(normalUrlEnd))
            {
                return
                    $"{GetRegexString(normalUrlStart)}(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>\\S+)){GetRegexString(normalUrlEnd)}";
            }
            return string.Empty;
        }

        public static string GetRegexChannel(string normalChannelStart, string normalChannelEnd)
        {
            if (!string.IsNullOrEmpty(normalChannelStart) && !string.IsNullOrEmpty(normalChannelEnd))
            {
                return
                    $"{GetRegexString(normalChannelStart)}\\s*(?<channel>[\\s\\S]+?)\\s*{GetRegexString(normalChannelEnd)}";
            }
            return string.Empty;
        }

        public static string GetRegexTitle(string normalTitleStart, string normalTitleEnd)
        {
            if (!string.IsNullOrEmpty(normalTitleStart) && !string.IsNullOrEmpty(normalTitleEnd))
            {
                return
                    $"{GetRegexString(normalTitleStart)}\\s*(?<title>[\\s\\S]+?)\\s*{GetRegexString(normalTitleEnd)}";
            }
            return string.Empty;
        }

        public static string GetRegexContent(string normalContentStart, string normalContentEnd)
        {
            if (!string.IsNullOrEmpty(normalContentStart) && !string.IsNullOrEmpty(normalContentEnd))
            {
                return
                    $"{GetRegexString(normalContentStart)}\\s*(?<content>[\\s\\S]+?)\\s*{GetRegexString(normalContentEnd)}";
            }
            return string.Empty;
        }

        public static string GetRegexAttributeName(string attributeName, string normalAuthorStart, string normalAuthorEnd)
        {
            if (!string.IsNullOrEmpty(normalAuthorStart) && !string.IsNullOrEmpty(normalAuthorEnd))
            {
                return
                    $"{GetRegexString(normalAuthorStart)}\\s*(?<{attributeName}>[\\s\\S]+?)\\s*{GetRegexString(normalAuthorEnd)}";
            }
            return string.Empty;
        }

        public static List<string> GetGatherUrlList(Rule rule)
        {
            var gatherUrls = new List<string>();
            if (rule.GatherUrlIsCollection)
            {
                gatherUrls.AddRange(StringCollectionToList(rule.GatherUrlCollection, separator: '\n'));
            }

            if (rule.GatherUrlIsSerialize)
            {
                if (rule.SerializeFrom <= rule.SerializeTo)
                {
                    var count = 1;
                    for (var i = rule.SerializeFrom; i <= rule.SerializeTo; i = i + rule.SerializeInterval)
                    {
                        count++;
                        if (count > 200) break;
                        var thePageNumber = i.ToString();
                        if (rule.SerializeIsAddZero && thePageNumber.Length == 1)
                        {
                            thePageNumber = "0" + i;
                        }
                        gatherUrls.Add(rule.GatherUrlSerialize.Replace("*", thePageNumber));
                    }
                }

                if (rule.SerializeIsOrderByDesc)
                {
                    gatherUrls.Reverse();
                }
            }

            return gatherUrls;
        }

        public static List<Item> GetAllItems(Rule rule, ProgressCache cache)
        {
            var gatherUrls = GetGatherUrlList(rule);
            var allItems = new List<Item>();

            foreach (var gatherUrl in gatherUrls)
            {
                cache.IsSuccess = true;
                cache.Message = "获取链接：" + gatherUrl;

                try
                {
                    var items = GetItems(gatherUrl, rule);
                    allItems.AddRange(items);
                }
                catch (Exception ex)
                {
                    cache.IsSuccess = false;
                    cache.Message = ex.Message;
                    cache.FailureMessages.Add(ex.Message);
                }
            }

            if (rule.IsOrderByDesc)
            {
                allItems.Reverse();
            }
            return allItems;
        }

        public static List<Item> GetItems(string gatherUrl, Rule rule)
        {
            if (!WebClientUtils.GetRemoteHtml(gatherUrl, rule.Charset, rule.CookieString, out var pageHtml, out var errorMessage))
            {
                throw new Exception(errorMessage);
            }
            var areaHtml = string.Empty;
            var regexListArea = GetRegexArea(rule.ListAreaStart, rule.ListAreaEnd);
            if (!string.IsNullOrEmpty(regexListArea))
            {
                areaHtml = GetValue("area", regexListArea, pageHtml);
            }

            var listHtml = !string.IsNullOrEmpty(areaHtml) ? areaHtml : pageHtml;

            var regexContentUrl = GetRegexUrl(rule.ContentUrlStart, rule.ContentUrlEnd);
            var regexImageUrl = string.Empty;
            if (rule.ImageSource == ImageSource.List)
            {
                regexImageUrl = GetRegexUrl(rule.ImageUrlStart, rule.ImageUrlEnd);
            }

            var regexTitle = string.Empty;
            if (rule.ContentTitleByList)
            {
                regexTitle = GetRegexTitle(rule.ContentTitleStart, rule.ContentTitleEnd);
            }

            var contentAttributes = ListUtils.GetStringList(rule.ContentAttributes);

            var contentUrls = GetValues("url", regexContentUrl, listHtml);
            var imageUrls = GetValues("url", regexImageUrl, listHtml);
            var titles = GetValues("title", regexTitle, listHtml);

            var attributesDict = new Dictionary<string, List<string>>();
            foreach (var attributeName in contentAttributes)
            {
                var normalByList = GetByListValue(rule, attributeName);
                if (!normalByList) continue;

                var normalStart = GetStartValue(rule, attributeName);
                var normalEnd = GetEndValue(rule, attributeName);
                var regex = GetRegexAttributeName(attributeName, normalStart, normalEnd);
                var values = GetValues(attributeName, regex, listHtml);
                attributesDict[attributeName] = values;
            }

            var myUri = new Uri(gatherUrl);
            var host = myUri.Scheme + "://" + myUri.Host;
            if (!myUri.IsDefaultPort)
            {
                host += ":" + myUri.Port;
            }

            var contentUrlList = new List<string>();
            foreach (var contentUrl in contentUrls)
            {
                if (string.IsNullOrEmpty(contentUrl)) continue;

                var url = string.Empty;
                if (PageUtils.IsProtocolUrl(contentUrl))
                {
                    url = contentUrl;
                }
                else if (contentUrl.StartsWith('/'))
                {
                    url = PageUtils.Combine(host, contentUrl);
                }

                if (string.IsNullOrEmpty(url)) continue;

                if (!contentUrlList.Contains(url))
                {
                    contentUrlList.Add(url);
                }
            }

            var imageUrlList = new List<string>();
            foreach (var imageUrl in imageUrls)
            {
                if (string.IsNullOrEmpty(imageUrl)) continue;

                var url = string.Empty;
                if (PageUtils.IsProtocolUrl(imageUrl))
                {
                    url = imageUrl;
                }
                else if (imageUrl.StartsWith('/'))
                {
                    url = PageUtils.Combine(host, imageUrl);
                }

                if (string.IsNullOrEmpty(url)) continue;

                if (!imageUrlList.Contains(url))
                {
                    imageUrlList.Add(url);
                }
            }

            var items = new List<Item>();
            for (var i = 0; i < contentUrlList.Count; i++)
            {
                var content = new Content();

                var imageUrl = imageUrls.Count > i ? imageUrls[i] : string.Empty;
                var title = titles.Count > i ? titles[i] : string.Empty;

                if (!string.IsNullOrEmpty(imageUrl))
                {
                    if (imageUrl.StartsWith('/'))
                    {
                        imageUrl = PageUtils.Combine(host, imageUrl);
                    }
                }

                content.ImageUrl = imageUrl;
                content.Title = title;

                foreach (var attributeName in contentAttributes)
                {
                    var normalByList = GetByListValue(rule, attributeName);
                    if (!normalByList) continue;

                    var normalDefault = GetDefaultValue(rule, attributeName);
                    var values = attributesDict[attributeName];

                    var value = values.Count > i ? values[i] : normalDefault;
                    content.Set(attributeName, value);
                }

                items.Add(new Item
                {
                    Url = contentUrlList[i],
                    Content = content
                });
            }

            return items;
        }

        public static NameValueCollection GetContentNameValueCollection(Rule rule, Item item)
        {
            var attributes = new NameValueCollection();

            if (!WebClientUtils.GetRemoteHtml(item.Url, rule.Charset, rule.CookieString, out var contentHtml, out var errorMessage))
            {
                throw new Exception(errorMessage);
            }

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

            var title = rule.ContentTitleByList ? item.Content.Title : GetValue("title", regexTitle,  contentHtml);
            var body = GetValue("content", regexContent, contentHtml);
            if (string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(regexContent2))
            {
                body = GetValue("content", regexContent2, contentHtml);
            }
            if (string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(regexContent3))
            {
                body = GetValue("content", regexContent3, contentHtml);
            }

            if (!string.IsNullOrEmpty(regexContentExclude))
            {
                body = Replace(regexContentExclude, body, string.Empty);
            }
            if (!string.IsNullOrEmpty(rule.ContentHtmlClearCollection))
            {
                var htmlClearList = StringCollectionToList(rule.ContentHtmlClearCollection);
                foreach (var htmlClear in htmlClearList)
                {
                    var clearRegex = $@"<{htmlClear}[^>]*>.*?<\/{htmlClear}>";
                    body = Replace(clearRegex, body, string.Empty);
                }
            }
            if (!string.IsNullOrEmpty(rule.ContentHtmlClearTagCollection))
            {
                var htmlClearTagList = StringCollectionToList(rule.ContentHtmlClearTagCollection);
                foreach (var htmlClearTag in htmlClearTagList)
                {
                    var clearRegex = $@"<{htmlClearTag}[^>]*>";
                    body = Replace(clearRegex, body, string.Empty);
                    clearRegex = $@"<\/{htmlClearTag}>";
                    body = Replace(clearRegex, body, string.Empty);
                }
            }

            var contentNextPageUrl = GetUrl(regexNextPage, contentHtml, item.Url);
            if (!string.IsNullOrEmpty(contentNextPageUrl))
            {
                body = GetPageContent(body, rule.Charset, contentNextPageUrl, rule.CookieString, regexContentExclude, rule.ContentHtmlClearCollection, rule.ContentHtmlClearTagCollection, regexContent, regexContent2, regexContent3, regexNextPage);
            }

            var channel = GetValue("channel", regexChannel, contentHtml);

            attributes.Add("Title", title);
            attributes.Add("Channel", channel);
            attributes.Add("Content", body);

            foreach (var attributeName in contentAttributes)
            {
                var normalByList = GetByListValue(rule, attributeName);
                var normalStart = GetStartValue(rule, attributeName);
                var normalEnd = GetEndValue(rule, attributeName);
                var normalDefault = GetDefaultValue(rule, attributeName);
                var regex = GetRegexAttributeName(attributeName, normalStart, normalEnd);
                var value = normalByList ? item.Content.Get<string>(attributeName) : GetValue(attributeName, regex, contentHtml);
                if (string.IsNullOrEmpty(value))
                {
                    value = normalDefault;
                }
                attributes.Set(attributeName, value);
            }

            return attributes;
        }

        public static bool GetByListValue(Rule rule, string attributeName)
        {
            return rule.Get($"{attributeName}ByList", false);
        }

        public static string GetStartValue(Rule rule, string attributeName)
        {
            return rule.Get($"{attributeName}Start", string.Empty);
        }

        public static string GetEndValue(Rule rule, string attributeName)
        {
            return rule.Get($"{attributeName}End", string.Empty);
        }

        public static string GetDefaultValue(Rule rule, string attributeName)
        {
            return rule.Get($"{attributeName}Default", string.Empty);
        }

        public static string GetPageContent(string previousPageContent, Charset charset, string url, string cookieString, string regexContentExclude, string contentHtmlClearCollection, string contentHtmlClearTagCollection, string regexContent, string regexContent2, string regexContent3, string regexNextPage)
        {
            var content = previousPageContent;
            if (!WebClientUtils.GetRemoteHtml(url, charset, cookieString, out var contentHtml, out var errorMessage))
            {
                throw new Exception(errorMessage);
            }
            var nextPageContent = GetValue("content", regexContent, contentHtml);
            if (string.IsNullOrEmpty(nextPageContent) && !string.IsNullOrEmpty(regexContent2))
            {
                nextPageContent = GetValue("content", regexContent2, contentHtml);
            }
            if (string.IsNullOrEmpty(nextPageContent) && !string.IsNullOrEmpty(regexContent3))
            {
                nextPageContent = GetValue("content", regexContent3, contentHtml);
            }

            if (!string.IsNullOrEmpty(nextPageContent))
            {
                if (string.IsNullOrEmpty(content))
                {
                    content += nextPageContent;
                }
                else
                {
                    content += PagePlaceHolder + nextPageContent;
                }
            }

            if (!string.IsNullOrEmpty(regexContentExclude))
            {
                content = Replace(regexContentExclude, content, string.Empty);
            }
            if (!string.IsNullOrEmpty(contentHtmlClearCollection))
            {
                var htmlClearList = StringCollectionToList(contentHtmlClearCollection);
                foreach (var htmlClear in htmlClearList)
                {
                    var clearRegex = $@"<{htmlClear}[^>]*>.*?<\/{htmlClear}>";
                    content = Replace(clearRegex, content, string.Empty);
                }
            }
            if (!string.IsNullOrEmpty(contentHtmlClearTagCollection))
            {
                var htmlClearTagList = StringCollectionToList(contentHtmlClearTagCollection);
                foreach (var htmlClearTag in htmlClearTagList)
                {
                    var clearRegex = $@"<{htmlClearTag}[^>]*>";
                    content = Replace(clearRegex, content, string.Empty);
                    clearRegex = $@"<\/{htmlClearTag}>";
                    content = Replace(clearRegex, content, string.Empty);
                }
            }

            var contentNextPageUrl = GetUrl(regexNextPage, contentHtml, url);
            if (!string.IsNullOrEmpty(contentNextPageUrl))
            {
                if (StringUtils.EqualsIgnoreCase(url, contentNextPageUrl))
                {
                    contentNextPageUrl = string.Empty;
                }
            }
            return !string.IsNullOrEmpty(contentNextPageUrl) ? GetPageContent(content, charset, contentNextPageUrl, cookieString, regexContentExclude, contentHtmlClearCollection, contentHtmlClearTagCollection, regexContent, regexContent2, regexContent3, regexNextPage) : content;
        }

        public static string ReplaceFirst(string input, string replace, string to)
        {
            var pos = input.IndexOf(replace, StringComparison.Ordinal);
            if (pos > 0)
            {
                //取位置前部分+替换字符串+位置（加上查找字符长度）后部分
                return input.Substring(0, pos) + to + input.Substring(pos + replace.Length);
            }
            if (pos == 0)
            {
                return to + input.Substring(replace.Length);
            }
            return input;
        }
    }
}
