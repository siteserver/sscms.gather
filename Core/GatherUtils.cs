using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using SSCMS.Gather.Models;
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
            return GetContents("url", regex, html);
        }

        public static List<string> GetLinkHrefList(string baseUrl, string html)
        {
            var regex = "a[^><]*\\s+href\\s*=\\s*(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>[^>\\s]*))";
            return GetUrls(regex, html, baseUrl);
        }

        public static List<string> GetOriginalLinkHrefList(string html)
        {
            var regex = "a[^><]*\\s+href\\s*=\\s*(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>[^>\\s]*))";
            return GetContents("url", regex, html);
        }

        public static List<string> GetFlashSrcList(string baseUrl, string html)
        {
            var regex = "embed\\s+[^><]*src\\s*=\\s*(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>[^>\\s]*))|param\\s+[^><]*value\\s*=\\s*(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>[^>\\s]*))";
            return GetUrls(regex, html, baseUrl);
        }

        public static List<string> GetOriginalFlashSrcList(string html)
        {
            var regex = "embed\\s+[^><]*src\\s*=\\s*(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>[^>\\s]*))|param\\s+[^><]*value\\s*=\\s*(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>[^>\\s]*))";
            return GetContents("url", regex, html);
        }

        public static bool IsImage(string fileExtName)
        {
            if (string.IsNullOrEmpty(fileExtName)) return false;

            var retVal = false;
            fileExtName = fileExtName.ToLower().Trim();
            if (!fileExtName.StartsWith("."))
            {
                fileExtName = "." + fileExtName;
            }
            if (fileExtName == ".bmp" || fileExtName == ".gif" || fileExtName == ".jpg" || fileExtName == ".jpeg" || fileExtName == ".png" || fileExtName == ".pneg" || fileExtName == ".webp")
            {
                retVal = true;
            }
            return retVal;
        }

        public static List<string> GetStyleImageUrls(string baseUrl, string html)
        {
            var list = GetUrls("url\\((?<url>[^\\(\\)]*)\\)", html, baseUrl);
            var urlList = new List<string>();
            foreach (var url in list)
            {
                if (!urlList.Contains(url) && IsImage(PathUtils.GetExtension(url)))
                {
                    urlList.Add(url);
                }
            }
            return urlList;
        }

        public static List<string> GetOriginalStyleImageUrls(string html)
        {
            var list = GetContents("url", "url\\((?<url>[^\\(\\)]*)\\)", html);
            var urlList = new List<string>();
            foreach (var url in list)
            {
                if (!urlList.Contains(url) && IsImage(PathUtils.GetExtension(url)))
                {
                    urlList.Add(url);
                }
            }
            return urlList;
        }

        public static List<string> GetBackgroundImageSrcList(string baseUrl, string html)
        {
            return GetUrls("background\\s*=\\s*(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>[^>\\s]*))", html, baseUrl);
        }

        public static List<string> GetOriginalBackgroundImageSrcList(string html)
        {
            return GetContents("url", "background\\s*=\\s*(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>[^>\\s]*))", html);
        }

        public static List<string> GetCssHrefList(string baseUrl, string html)
        {
            //string regex = "link\\s+[^><]*href\\s*=\\s*(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>\\S+))|@import\\s*url\\((?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>\\S+))\\)";

            return GetUrls("link\\s+[^><]*href\\s*=\\s*(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>[^>\\s]*))|\\@import\\s*url\\s*\\(\\s*(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>.*?))\\s*\\)", html, baseUrl);
        }

        public static List<string> GetOriginalCssHrefList(string html)
        {
            return GetContents("url", "link\\s+[^><]*href\\s*=\\s*(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>[^>\\s]*))|\\@import\\s*url\\s*\\(\\s*(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>.*?))\\s*\\)", html);
        }

        public static List<string> GetScriptSrcList(string baseUrl, string html)
        {
            return GetUrls("script\\s+[^><]*src\\s*=\\s*(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>[^>\\s]*))", html, baseUrl);
        }

        public static List<string> GetOriginalScriptSrcList(string html)
        {
            return GetContents("url", "script\\s+[^><]*src\\s*=\\s*(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>[^>\\s]*))", html);
        }

        //public static List<string> GetTagInnerContents(string tagName, string html)
        //{
        //    return GetContents("content", $"<{tagName}\\s+[^><]*>\\s*(?<content>[\\s\\S]+?)\\s*</{tagName}>", html);
        //}

        //public static List<string> GetTagContents(string tagName, string html)
        //{
        //    var list = new List<string>();

        //    var regex = $@"<({tagName})[^>]*>(.*?)</\1>|<{tagName}[^><]*/>";

        //    var matches = Regex.Matches(html, regex, RegexOptions.IgnoreCase);
        //    foreach (Match match in matches)
        //    {
        //        if (match.Success)
        //        {
        //            list.Add(match.Result("$0"));
        //        }
        //    }

        //    return list;
        //}

        //public static string GetTagName(string html)
        //{
        //    var match = Regex.Match(html, "<([^>\\s]+)[\\s\\SS]*>", RegexOptions.IgnoreCase);
        //    if (match.Success)
        //    {
        //        return match.Result("$1");
        //    }
        //    return string.Empty;
        //}

        //public static string GetInnerContent(string tagName, string html)
        //{
        //    var regex = $"<{tagName}[^><]*>(?<content>[\\s\\S]+?)</{tagName}>";
        //    return GetContent("content", regex, html);
        //}

        //public static string GetAttributeContent(string attributeName, string html)
        //{
        //    var regex =
        //        $"<[^><]+\\s*{attributeName}\\s*=\\s*(?:\"(?<value>[^\"]*)\"|'(?<value>[^']*)'|(?<value>[^>\\s]*)).*?>";
        //    return GetContent("value", regex, html);
        //}

        public static List<string> GetUrls(string html, string baseUrl)
        {
            var regex = "<a\\s*.*?href\\s*=\\s*(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>[^>\\s]*)).*?>";
            return GetUrls(regex, html, baseUrl);
        }

        public static List<string> GetUrls(string regex, string html, string baseUrl)
        {
            var urlList = new List<string>();
            if (string.IsNullOrEmpty(regex))
            {
                regex = "<a\\s*.*?href\\s*=\\s*(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>[^>\\s]*)).*?>";
            }
            var groupName = "url";
            var list = GetContents(groupName, regex, html);
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

        public static string GetUrlWithoutPathInfo(string rawUrl)
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

        public static string GetUrlWithoutFileName(string rawUrl)
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

        public static string RemoveProtocolFromUrl(string url)
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
            return GetUrlByBaseUrl(GetContent("url", regex, html), baseUrl);
        }

        public static string GetContent(string groupName, string regex, string html)
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

        public static string Replace(string regex, string input, string replacement)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var reg = new Regex(regex, Options);
            return reg.Replace(input, replacement);
        }

        //public static string Replace(string regex, string input, string replacement, int count)
        //{
        //    if (count == 0)
        //    {
        //        return Replace(regex, input, replacement);
        //    }

        //    if (string.IsNullOrEmpty(input)) return input;
        //    var reg = new Regex(regex, Options);
        //    return reg.Replace(input, replacement, count);
        //}

        public static bool IsMatch(string regex, string input)
        {
            var reg = new Regex(regex, Options);
            return reg.IsMatch(input);
        }

        public static List<string> GetContents(string groupName, string regex, string html)
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

        public static string RemoveScripts(string html)
        {
            return Replace("<script[^><]*>.*?<\\/script>", html, string.Empty);
        }

        //public static string RemoveImages(string html)
        //{
        //    return Replace("<img[^><]*>", html, string.Empty);
        //}

        public static IEnumerable<string> StringCollectionToList(string collection)
        {
            return StringCollectionToList(collection, ',');
        }

        public static List<string> StringCollectionToList(string collection, char separator)
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

        public static string GetRegexArea(string normalAreaStart, string normalAreaEnd)
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

        public static List<string> GetContentUrlList(Rule rule, string regexListArea, string regexUrlInclude, ProgressCache cache)
        {
            var gatherUrls = GetGatherUrlList(rule);
            var contentUrls = new List<string>();

            foreach (var gatherUrl in gatherUrls)
            {
                cache.IsSuccess = true;
                cache.Message = "获取链接：" + gatherUrl;

                try
                {
                    var urls = GetContentUrls(gatherUrl, rule.Charset, rule.CookieString, regexListArea, regexUrlInclude);
                    contentUrls.AddRange(urls);
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
                contentUrls.Reverse();
            }
            return contentUrls;
        }

        public static List<string> GetContentUrls(string gatherUrl, Charset charset, string cookieString, string regexListArea, string regexUrlInclude)
        {
            var contentUrls = new List<string>();
            if (!WebClientUtils.GetRemoteHtml(gatherUrl, charset, cookieString, out var listHtml, out var errorMessage))
            {
                throw new Exception(errorMessage);
            }
            var areaHtml = string.Empty;

            if (!string.IsNullOrEmpty(regexListArea))
            {
                areaHtml = GetContent("area", regexListArea, listHtml);
            }

            var urlsList = GetUrls(!string.IsNullOrEmpty(areaHtml) ? areaHtml : listHtml, gatherUrl);

            var isInclude = !string.IsNullOrEmpty(regexUrlInclude);

            foreach (var url in urlsList)
            {
                if (!string.IsNullOrEmpty(url))
                {
                    var contentUrl = url.Replace("&amp;", "&");
                    if (isInclude && !IsMatch(regexUrlInclude, contentUrl))
                    {
                        continue;
                    }
                    if (!contentUrls.Contains(contentUrl))
                    {
                        contentUrls.Add(contentUrl);
                    }
                }
            }
            return contentUrls;
        }

        public static NameValueCollection GetContentNameValueCollection(Charset charset, string url, string cookieString, string regexContentExclude, string contentHtmlClearCollection, string contentHtmlClearTagCollection, string regexTitle, string regexContent, string regexContent2, string regexContent3, string regexNextPage, string regexChannel, List<string> contentAttributes, Rule rule)
        {
            var attributes = new NameValueCollection();

            if (!WebClientUtils.GetRemoteHtml(url, charset, cookieString, out var contentHtml, out var errorMessage))
            {
                throw new Exception(errorMessage);
            }
            var title = GetContent("title", regexTitle, contentHtml);
            var content = GetContent("content", regexContent, contentHtml);
            if (string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(regexContent2))
            {
                content = GetContent("content", regexContent2, contentHtml);
            }
            if (string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(regexContent3))
            {
                content = GetContent("content", regexContent3, contentHtml);
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
                content = GetPageContent(content, charset, contentNextPageUrl, cookieString, regexContentExclude, contentHtmlClearCollection, contentHtmlClearTagCollection, regexContent, regexContent2, regexContent3, regexNextPage);
            }

            var channel = GetContent("channel", regexChannel, contentHtml);

            attributes.Add("Title", title);
            attributes.Add("Channel", channel);
            attributes.Add("Content", content);

            foreach (var attributeName in contentAttributes)
            {
                var normalStart = GetStartValue(rule, attributeName);
                var normalEnd = GetEndValue(rule, attributeName);
                var regex = GetRegexAttributeName(attributeName, normalStart, normalEnd);
                var value = GetContent(attributeName, regex, contentHtml);
                attributes.Set(attributeName, value);
            }

            return attributes;
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
            var nextPageContent = GetContent("content", regexContent, contentHtml);
            if (string.IsNullOrEmpty(nextPageContent) && !string.IsNullOrEmpty(regexContent2))
            {
                nextPageContent = GetContent("content", regexContent2, contentHtml);
            }
            if (string.IsNullOrEmpty(nextPageContent) && !string.IsNullOrEmpty(regexContent3))
            {
                nextPageContent = GetContent("content", regexContent3, contentHtml);
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
    }
}
