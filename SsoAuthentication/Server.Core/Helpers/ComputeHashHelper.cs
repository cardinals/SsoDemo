using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Server.Core.Helpers
{
    public class ComputeHashHelper
    {
        private readonly HashAlgorithm DigestProvider = new MD5CryptoServiceProvider();
        public readonly Encoding Charset = Encoding.UTF8;

        #region hash签名
        public String ComputeHash(Byte[] body)
        {
            Byte[] hashBytes = DigestProvider.ComputeHash(body);
            String hash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
            return hash;
        }

        public String ComputeHash(Stream stream)
        {
            Byte[] body = StreamToBytes(stream);
            Byte[] hashBytes = DigestProvider.ComputeHash(body);
            String hash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
            return hash;
        }
        #endregion

        #region 判断签名是否正确
        public bool IsComputeHash(string digest, Byte[] body)
        {
            var tempDigest = ComputeHash(body);
            return tempDigest == digest;
        }
        public bool IsComputeHash(string digest, Stream stream)
        {
            var tempDigest = ComputeHash(stream);
            return tempDigest == digest;
        }
        #endregion

        private byte[] StreamToBytes(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            // 设置当前流的位置为流的开始 
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;

        }
    }
}
