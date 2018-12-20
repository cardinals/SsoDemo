using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Core.Model
{
    public class LoginModel
    {
        public string ReturnUrl { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }
        public bool RememberMe { get; set; }
    }
}
