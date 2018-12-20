using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Core.Entity
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        #region 登陆状态信息，后续单独存放
        public Guid? UserToken { get; set; }
        public DateTime ExpiredTime { get; set; }
        public bool Active { get; set; } 
        #endregion
    }
}
