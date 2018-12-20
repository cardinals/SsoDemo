using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Core.Model
{
    public class BaseResponseModel
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 错误码
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 错误提示信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 错误提示中的相关动态参数数据
        /// </summary>
        public Object[] MessageData { get; set; }
    }
}
