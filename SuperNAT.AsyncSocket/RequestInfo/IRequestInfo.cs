using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.AsyncSocket
{
    /// <summary>
    /// 数据实体
    /// </summary>
    public interface IRequestInfo
    {
        /// <summary>
        /// 接收数据
        /// </summary>
        byte[] Raw { get; set; }
        /// <summary>
        /// 解析结果
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 解析信息
        /// </summary>
        public string Message { get; set; }
    }
}
