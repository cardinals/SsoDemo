using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SiteServer.Core.Entity
{
    public class UserMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SsoUserId { get; set; }
      
        public int UserId { get; set; }
        public Guid Token  { get; set; }
        /// <summary>
        /// 接入方的配置Id
        /// </summary>
        public int SourceSiteConfigId { get; set; }
        /// <summary>
        /// 被接入方的配置Id
        /// </summary>
        public int TargetSiteConfigId { get; set; }
    }
}
