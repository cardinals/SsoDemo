using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Core.Entity
{
    public class GenericAttribute
    {
        public int Id { get; set; }
        public int EntityId { get; set; }
        /// <summary>
        /// 表名称
        /// </summary>
        public string KeyGroup { get; set; }
        /// <summary>
        /// 字段名
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 字段中文名称
        /// </summary>
        public string KeyName { get; set; }
        public string Value { get; set; }
    }
}
