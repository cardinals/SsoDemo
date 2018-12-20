using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Core.Model
{
    public class AuthMappingRequestModel
    {
        public string SiteToken { get; set; }
        public int SsoUserId { get; set;}
        public string TargetUrl { get; set; }
    }
}
