using System;
using System.Collections.Generic;
using System.Text;

namespace SiteServer.Core.Model
{
    public class AuthRedirectToSiteResponseModel:BaseResponseModel
    {
        public string UserToken { get; set; }
    }
}
