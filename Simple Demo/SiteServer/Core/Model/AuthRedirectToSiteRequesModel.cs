using System;
using System.Collections.Generic;
using System.Text;

namespace SiteServer.Core.Model
{
    public class AuthRedirectToSiteRequesModel
    {
        //public string SiteToken { get; set; }
        public int UserId { get; set; }
        public int SsoUserId { get; set; }
        public string TargetUrl { get; set; }
        public string FailUrl { get; set; }
    }
}
