using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Core.Model
{
    public class AuthSessionRequestModel
    {
        public string SiteToken { get; set; }
        public string UserToken { get; set; }
        public int SsoUserId { get; set; }
        public string TargetUrl { get; set; }
        //public string FailUrl { get; set; }
    }
}
