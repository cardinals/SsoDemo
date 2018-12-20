using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Core.Model
{
    public class AuthMappingCallBackRequestModel
    {
        public string AppKey { get; set; }
        public string SsoUserId { get; set; }
    }
}
