using Microsoft.Extensions.Options;
using Client.Core.Helpers;
using System;
using System.IO;
using System.Net;
using Client.Core.Model;
using Client.Core.Constants;

namespace Client.Core.Helpers
{
    public class HttpPostHelper
    {
        private readonly ComputeHashHelper computeHashHelper;
        private readonly SsoConfig ssoConfigs;
        public HttpPostHelper(IOptions<SsoConfig> _ssoConfigs, ComputeHashHelper _computeHashHelper)
        {
            ssoConfigs = _ssoConfigs.Value;
            computeHashHelper = _computeHashHelper;
        }

        public string Send(String target, String data)
        {
            string responseData = string.Empty;
            Byte[] body = computeHashHelper.Charset.GetBytes(data);
            String hash = computeHashHelper.ComputeHash(body);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(target);
            request.Method = "POST";
            request.ContentLength = body.Length;
            request.Headers[HttpHeaders.AppKey] = ssoConfigs.AppKey;
            //request.Headers[HttpHeaders.DataType] = "JSON";
            request.ContentType = "application/json";
            request.Headers[HttpHeaders.AcceptEncoding] = "gzip";   //support gzip
            request.Headers["x-requested-with"] = "XMLHttpRequest";
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.Headers[HttpHeaders.Disgest] = hash;
            using (Stream os = request.GetRequestStream())
            {
                os.Write(body, 0, body.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), computeHashHelper.Charset))
                {
                    responseData = reader.ReadToEnd().ToString();
                }
            }
            return responseData;
        }
    }

    //public class HttpHeaders
    //{
    //    public const String AcceptEncoding = "Accept-Encoding";
    //    public const String AcceptLanguage = "Accept-Language";
    //    public const String ContentEncoding = "Content-Encoding";
    //    //public const String Api = "API";
    //    public const String AppKey = "AppKey";
    //    public const String SiteToken = "SiteToken";
    //    public const String DataType = "DataType";
    //    public const String Disgest = "Digest";
    //}
}
