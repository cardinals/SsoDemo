using System;
using System.Collections.Generic;
using System.Text;

namespace SiteClient.Core.Model
{
    public class LoginModel
    {
        public string UserName { get; set; }
        public string PassWord { get; set; }
        public bool RememberMe { get; set; }
    }
}
