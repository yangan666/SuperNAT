using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.AsyncSocket
{
    /// <summary>
    /// 过滤器
    /// </summary>
    /// <typeparam name="TRequestInfo"></typeparam>
    public interface IReceiveFilter<TRequestInfo> where TRequestInfo : IRequestInfo, new()
    {
        /// <summary>
        /// 下一个过滤器
        /// </summary>
        IReceiveFilter<TRequestInfo> NextReceiveFilter { get; }
        /// <summary>
        /// 切包方法
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        TRequestInfo Filter(ref SequenceReader<byte> reader);
        /// <summary>
        /// 重置变量
        /// </summary>
        void Reset();
    }
}
