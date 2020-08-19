using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using Datory;
using SSCMS.Gather.Models;
using SSCMS.Utils;

namespace SSCMS.Gather.Core
{
    public static class WebClientUtils
    {
        public static bool GetRemoteHtml(string url, Charset charset, string cookieString, out string content, out string errorMessage)
        {
            content = string.Empty;
            errorMessage = string.Empty;
            for (var i = 0; i < 2; i++)
            {
                try
                {
                    content = GetRemoteFileSource(url, charset, cookieString);
                    return true;
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                }
            }
            return false;
        }

        // 获取指定网页的HTML代码
        private static string GetRemoteFileSource(string url, Charset charset, string cookieString)
        {
            try
            {
                string retval;
                var uri = new Uri(PageUtils.AddProtocolToUrl(url.Trim()));
                var hwReq = (HttpWebRequest)WebRequest.Create(uri);
                if (!string.IsNullOrEmpty(cookieString))
                {
                    hwReq.Headers.Add("Cookie", cookieString);
                }
                var hwRes = (HttpWebResponse)hwReq.GetResponse();
                hwReq.Method = "Get";
                //hwReq.ContentType = "text/html";
                hwReq.KeepAlive = false;

                using (var reader = new System.IO.StreamReader(hwRes.GetResponseStream(), GetEncoding(charset)))
                {
                    retval = reader.ReadToEnd();
                }

                return retval;
            }
            catch
            {
                throw new Exception($"页面地址“{url}”无法访问！");
            }
        }

        public static Encoding GetEncoding(Charset type)
        {
            if (type == Charset.Utf8)
            {
                return new UTF8Encoding(false);
            }
            return Encoding.GetEncoding(type.GetValue());
        }

        public static string GetRemoteFileSource(string url, Charset charset)
        {
            return GetRemoteFileSource(url, charset, string.Empty);
        }

        public static HttpStatusCode GetRemoteUrlStatusCode(string url)
        {
            var uri = new Uri(PageUtils.AddProtocolToUrl(url.Trim()));
            var req = (HttpWebRequest)WebRequest.Create(uri);
            req.Method = "HEAD";                           //设置请求方式为请求头，这样就不需要把整个网页下载下来 
            req.KeepAlive = false;
            var res = (HttpWebResponse)req.GetResponse();
            return res.StatusCode;
        }

        public static bool SaveRemoteFileToLocal(string remoteUrl, string localFileName)
        {
            try
            {
                var myWebClient = new WebClient();
                myWebClient.DownloadFile(remoteUrl, localFileName);
            }
            catch
            {
                throw new Exception($"页面地址“{remoteUrl}”无法访问！");
            }
            return true;
        }

        public static bool Post(string url, NameValueCollection arguments, out string retval)
        {
            try
            {
                var builder = new StringBuilder();
                foreach (string key in arguments.Keys)
                {
                    builder.Append($"{key}={arguments[key]}&");
                }
                if (builder.Length > 0) builder.Length -= 1;
                var postData = Encoding.UTF8.GetBytes(builder.ToString());
                var webClient = new WebClient();
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");//采取POST方式必须加的header
                var responseData = webClient.UploadData(url, "POST", postData);//得到返回字符流  
                retval = Encoding.UTF8.GetString(responseData);//解码

                return true;
            }
            catch (Exception ex)
            {
                retval = ex.Message;
            }
            return false;
        }

        public static bool Post(string url, string requestData, out string retval)
        {
            try
            {
                var postData = Encoding.UTF8.GetBytes(requestData);
                var webClient = new WebClient();
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");//采取POST方式必须加的header
                var responseData = webClient.UploadData(url, "POST", postData);//得到返回字符流  
                retval = Encoding.UTF8.GetString(responseData);//解码

                return true;
            }
            catch (Exception ex)
            {
                retval = ex.Message;
            }
            return false;
        }

        public static bool Get(string url, string requestData, out string retval)
        {
            try
            {
                var postData = Encoding.UTF8.GetBytes(requestData);
                var webClient = new WebClient();
                var responseData = webClient.UploadData(url, "GET", postData);//得到返回字符流  
                retval = Encoding.UTF8.GetString(responseData);//解码

                return true;
            }
            catch (Exception ex)
            {
                retval = ex.Message;
            }
            return false;
        }
    }
}
