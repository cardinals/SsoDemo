using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Core.Model
{
    public class AuthMappingRequestModel
    {
        /// <summary>
        /// 授权页面显示 client 的用户id
        /// </summary>
        public string UserId { get; set; }
        public string AppKey { get; set; }
        public string TargetUrl { get; set; }
    }
}
