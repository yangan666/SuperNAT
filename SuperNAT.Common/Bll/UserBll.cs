using Dapper;
using SuperNAT.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Common.Bll
{
    public class UserBll : BaseBll<User>
    {
        public ReturnResult<User> Login(User model)
        {
            var rst = new ReturnResult<User>() { Message = "用户名或密码错误" };

            try
            {
                model.password = EncryptHelper.MD5Encrypt(model.password);
                var sql = new StringBuilder("select * from user where password=@password ");
                if (!string.IsNullOrEmpty(model.user_id))
                {
                    sql.Append("and user_id=@user_id");
                }
                else if (!string.IsNullOrEmpty(model.user_name))
                {
                    sql.Append("and user_name=@user_name");
                }
                else
                {
                    rst.Status = 10005;
                    rst.Message = "信息已失效请刷新界面重新登录！";
                    return rst;
                }
                rst.Data = conn.QueryFirstOrDefault<User>(sql.ToString(), model);
                if (rst.Data != null)
                {
                    rst.Result = true;
                    rst.Message = "登录成功";
                }
            }
            catch (Exception ex)
            {
                rst.Message = $"服务异常：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }

        public ReturnResult<List<User>> GetList(User model)
        {
            var rst = new ReturnResult<List<User>>() { Message = "暂无记录" };

            try
            {
                rst.Data = conn.GetList<User>("", model).ToList();
                if (rst.Data != null)
                {
                    rst.Result = true;
                    rst.Message = "获取成功";
                }
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
