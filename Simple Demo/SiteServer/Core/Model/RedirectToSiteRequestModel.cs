using System;
using System.Collections.Generic;
using System.Text;

namespace SiteServer.Core.Model
{
    public class RedirectToSiteRequestModel
    {
        public string AppKey { get; set; }
        public string UserToken { get; set; }
        public int UserId { get; set; }
        public int SsoUserId { get; set; }
        public string TargetUrl { get; set; }
        public string FailUrl { get; set; }
    }
}
