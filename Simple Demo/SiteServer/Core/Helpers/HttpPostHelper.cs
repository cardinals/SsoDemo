using Microsoft.Extensions.Options;
using SiteClient.Core.Model;
using SiteServer.Core.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SiteServer.Core.Helpers
{
    public class HttpPostHelper
    {
        private static readonly HashAlgorithm DigestProvider = new MD5CryptoServiceProvider();
        private static readonly Encoding Charset = Encoding.UTF8;
       

        public string Send(String target, SiteConfig siteConfig, String data)
        {
            string responseData = string.Empty;
            Byte[] body = Charset.GetBytes(data);
            String hash = ComputeHash(body);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(target);
            request.Method = "POST";
            request.ContentLength = body.Length;
            request.Headers[HttpHeaders.SiteToken] = siteConfig.SiteToken;//#TODO MD5加密
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

        private String ComputeHash(Byte[] body)
        {
            Byte[] hashBytes = DigestProvider.ComputeHash(body);
            String hash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
            return hash;
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
