using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Core.Model
{
    public class AuthUserTokenRequestModel
    {
        public string AppKey { get; set; }
        public int SsoUserId { get; set; }
        public string UserToken { get; set; }
    }
}
