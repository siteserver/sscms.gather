using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Datory;
using SSCMS.Gather.Models;
using SSCMS.Utils;

namespace SSCMS.Gather.Core
{
    public static class WebClientUtils
    {
        public static async Task<Result> GetRemoteHtmlAsync(string url, Charset charset, string cookieString)
        {
            var result = new Result
            {
                IsSuccess = false,
                Content = string.Empty,
                ErrorMessage = string.Empty,
            };

            for (var i = 0; i < 2; i++)
            {
                try
                {
                    result.Content = await GetStringAsync(url, charset);
                    result.IsSuccess = true;
                    break;
                }
                catch (Exception ex)
                {
                    result.ErrorMessage = ex.Message;
                }
            }

            return result;
        }

        public static async Task<string> GetStringAsync(string url, Charset charset)
        {
            try
            {
                string html;

                if (charset == Charset.Utf8)
                {
                    using (var client = new HttpClient())
                    {
                        html = await client.GetStringAsync(url);
                    }
                }
                else
                {
                    using (var client = new HttpClient())
                    {
                        var bytes = await client.GetByteArrayAsync(url);
                        html = ConvertBytesToString(bytes, charset);
                    }
                }

                return html;
            }
            catch (Exception ex)
            {
                throw new Exception($"页面地址“{url}”无法访问，{ex.Message}！");
            }
        }

        private static string ConvertBytesToString(byte[] bytes, Charset charset)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var fromEncoding = System.Text.Encoding.GetEncoding(charset.GetValue().ToUpper());
            var toEncoding = Encoding.GetEncoding("UTF-8");
            var toBytes = Encoding.Convert(fromEncoding, toEncoding, bytes);
            return toEncoding.GetString(toBytes);
        }

        public static async Task<bool> DownloadAsync(string remoteUrl, string filePath)
        {
            try
            {
                FileUtils.DeleteFileIfExists(filePath);

                using (var client = new HttpClient())
                {
                    using (var stream = await client.GetStreamAsync(remoteUrl))
                    {
                        using (var fs = new FileStream(filePath, FileMode.CreateNew))
                        {
                            await stream.CopyToAsync(fs);
                        }
                    }
                }
            }
            catch
            {
                throw new Exception($"页面地址“{remoteUrl}”无法访问！");
            }
            return true;
        }
    }
}
