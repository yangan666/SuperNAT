using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Model
{
    public class ApiResult<T>
    {
        public ApiResult()
        {
            Result = false;
        }
        public bool Result { get; set; }
        public int Status { get; set; }

        public T Data { get; set; }

        public string Message { get; set; }

        public PageInfo PageInfo { get; set; }
        public static ApiResult<T> Ok(T data, string message = "业务成功")
        {
            return new ApiResult<T>() { Result = true, Data = data, Message = message };
        }

        public static ApiResult<T> Fail(string message = "业务失败")
        {
            return new ApiResult<T>() { Message = message };
        }
    }
    public class PageInfo
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int PageCount => Math.Max((TotalCount + PageSize - 1) / PageSize, 1);
        public int TotalCount { get; set; }
    }
}
