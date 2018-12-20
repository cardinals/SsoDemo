using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SiteClient.Core.Entity
{
    public class UserMapping
    {
        public int UserId { get; set; }
        [Key]
        public int SsoUserId { get; set; }
    }
}
