using Client.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Core.Model
{
    public class AuthCallbackRequestModel
    {
        //public int UserId { get; set; }
        public int SsoUserId { get; set; }
        public AuthTypeEnum AuthType { get; set; }
    }
}
