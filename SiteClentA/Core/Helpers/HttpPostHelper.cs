using Microsoft.Extensions.Options;
using SiteClient.Core.Model;
using SiteServer.Core.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SiteClient.Core.Helpers
{
    public class HttpPostHelper
    {
        private static readonly ComputeHashHelper computeHashHelper = new ComputeHashHelper();
        private static readonly Encoding Charset = Encoding.UTF8;
        private readonly SsoConfig ssoConfigs;
        public HttpPostHelper(IOptions<SsoConfig> _ssoConfigs)
        {
            ssoConfigs = _ssoConfigs.Value;
        }

        public string Send(String target, String data)
        {
            string responseData = string.Empty;
            Byte[] body = Charset.GetBytes(data);
            String hash = computeHashHelper.ComputeHash(body);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(target);
            request.Method = "POST";
            request.ContentLength = body.Length;
            request.Headers[HttpHeaders.AppKey] = ssoConfigs.AppKey;
            request.Headers[HttpHeaders.DataType] = "JSON";
            request.Headers[HttpHeaders.AcceptEncoding] = "gzip";   //support gzip
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.Headers[HttpHeaders.Disgest] = hash;
            using (Stream os = request.GetRequestStream())
            {
                os.Write(body, 0, body.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Charset))
                {
                    responseData = reader.ReadToEnd().ToString();
                }
            }
            return responseData;
        }
    }

    public class HttpHeaders
    {
        public const String AcceptEncoding = "Accept-Encoding";
        public const String AcceptLanguage = "Accept-Language";
        public const String ContentEncoding = "Content-Encoding";
        //public const String Api = "API";
        public const String AppKey = "AppKey";
        public const String SiteToken = "SiteToken";
        public const String DataType = "DataType";
        public const String Disgest = "Digest";
    }
}
