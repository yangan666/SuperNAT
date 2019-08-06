using Dapper;
using SuperNAT.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Common.Bll
{
    public class UserBll : BaseBll
    {
        public ReturnResult<bool> Add(User model)
        {
            var rst = new ReturnResult<bool>();

            try
            {
                rst.Result = conn.Insert(model) > 0;
                rst.Message = "添加成功";
            }
            catch (Exception ex)
            {
                rst.Message = $"添加失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }

        public ReturnResult<List<User>> GetList(User model)
        {
            var rst = new ReturnResult<List<User>>();

            try
            {
                rst.Data = conn.GetList<User>("", model).ToList();
                rst.Result = true;
                rst.Message = "获取成功";
            }
            catch (Exception ex)
            {
                rst.Message = $"获取失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }
    }
}
