using Server.Core.Constants;
using Server.Core.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Server.Core.Helpers
{
    public class HttpPostHelper
    {
        private readonly ComputeHashHelper computeHashHelper;
        public HttpPostHelper(ComputeHashHelper _computeHashHelper)
        {
            computeHashHelper = _computeHashHelper;
        }


        public string Send(String target, SiteConfig siteConfig, String data)
        {
            string responseData = string.Empty;
            Byte[] body = computeHashHelper.Charset.GetBytes(data);
            String hash = computeHashHelper.ComputeHash(body);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(target);
            request.Method = "POST";
            request.ContentLength = body.Length;
            request.Headers[HttpHeaders.SiteToken] = siteConfig.SiteToken;//#TODO MD5加密
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
}
