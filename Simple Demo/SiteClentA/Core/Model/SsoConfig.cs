using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiteClient.Core.Model
{
    public class SsoConfig
    {
        /// <summary>
        /// SSO跳转内部地址
        /// </summary>
        public string RedirectToSite { get; set; }
        /// <summary>
        /// SSO跳转验证地址
        /// </summary>
        public string AuthRedirectToSite { get; set; }
        /// <summary>
        /// 验证失败跳转地址
        /// </summary>
        public string AuthFail { get; set; }
        /// <summary>
        /// client 向 server 请求时的 key
        /// </summary>
        public string AppKey { get; set; }
        /// <summary>
        /// server 向 client 请求时的 key
        /// </summary>
        public string SiteToken { get; set; }
    }
}
