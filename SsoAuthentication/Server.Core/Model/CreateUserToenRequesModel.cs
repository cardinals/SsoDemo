using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Core.Model
{
    public class CreateUserToenRequesModel
    {
        /// <summary>
        /// 授权页面需要
        /// </summary>
        public int UserId { get; set; }
        public int SsoUserId { get; set; }
        public string TargetUrl { get; set; }
        //public string FailUrl { get; set; }
    }
}
