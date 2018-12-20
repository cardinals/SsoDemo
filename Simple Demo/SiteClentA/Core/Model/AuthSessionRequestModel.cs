using System;
using System.Collections.Generic;
using System.Text;

namespace SiteClient.Core.Model
{
    public class AuthSessionRequestModel
    {
        public string SiteToken { get; set; }
        public string UserToken { get; set; }
        public int UserId { get; set; }
        public int OtherUserId { get; set; }
        public int SsoUserId { get; set; }
        public string TargetUrl { get; set; }
        public string FailUrl { get; set; }
    }
}
