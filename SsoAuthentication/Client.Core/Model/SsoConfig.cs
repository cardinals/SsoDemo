using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Core.Model
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
        public string CreateUserToen { get; set; }
        /// <summary>
        /// 联合登录接入地址
        /// </summary>
        public string AuthMapping { get; set; }

        /// <summary>
        /// 联合登录回调验证地址
        /// </summary>
        public string AuthMappingCallBack { get; set; }
        /// <summary>
        /// 验证 UserToken 是否生效
        /// </summary>
        public string AuthUserToken { get; set; }
        /// <summary>
        /// sso 登录地址
        /// </summary>
        public string Login { get; set; }
        /// <summary>
        /// sso 退出登录地址
        /// </summary>
        public string LogOut { get; set; }
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
