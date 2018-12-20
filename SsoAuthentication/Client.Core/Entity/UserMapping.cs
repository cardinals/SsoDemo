using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Client.Core.Entity
{
    public class UserMapping
    {
        [Key]
        public int UserId { get; set; }
        public int SsoUserId { get; set; }
    }
}
