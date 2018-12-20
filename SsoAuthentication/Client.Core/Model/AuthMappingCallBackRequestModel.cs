using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Core.Model
{
    public class AuthMappingCallBackRequestModel
    {
        public string AppKey { get; set; }
        public int SsoUserId { get; set; }
    }
}
