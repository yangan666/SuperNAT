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

        public ReturnResult<bool> UpdateUser(User model)
        {
            var rst = new ReturnResult<bool>() { Message = "更新失败" };

            try
            {
                var sql = new StringBuilder();
                sql.Append("update user set ");
                sql.Append("user_name=@user_name,");
                if (!string.IsNullOrWhiteSpace(model.password))
                {
                    sql.Append("password=@password,");
                }
                sql.Append("wechat=@wechat,");
                sql.Append("tel=@tel ");
                sql.Append("where user_id=@user_id ");
                if (conn.Execute(sql.ToString(), model) > 0)
                {
                    rst.Result = true;
                    rst.Message = "更新成功";
                }
            }
            catch (Exception ex)
            {
                rst.Message = $"更新失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }

        public ReturnResult<bool> DisableUser(User model)
        {
            var rst = new ReturnResult<bool>() { Message = "更新失败" };

            try
            {
                var sql = new StringBuilder();
                sql.Append("update user set ");
                sql.Append("is_disabled=@is_disabled ");
                sql.Append("where user_id=@user_id ");
                if (conn.Execute(sql.ToString(), model) > 0)
                {
                    rst.Result = true;
                    rst.Message = "更新成功";
                }
            }
            catch (Exception ex)
            {
                rst.Message = $"更新失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }

        public ReturnResult<User> GetUserInfo(string user_id)
        {
            var rst = new ReturnResult<User>() { Message = "用户名或密码错误" };

            try
            {
                rst.Data = conn.QueryFirstOrDefault<User>("select * from user where user_id=@user_id ", new { user_id });
                if (rst.Data != null)
                {
                    rst.Result = true;
                    rst.Message = "获取成功";
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
                if (model.page_index > 0)
                {
                    var where = new StringBuilder();
                    if (!string.IsNullOrWhiteSpace(model.user_name))
                    {
                        model.user_name = $"%{model.user_name}%";
                        where.Append("where user_name like @user_name ");
                        where.Append("or wechat like @user_name ");
                        where.Append("or tel like @user_name ");
                    }
                    rst.Data = conn.GetListPaged<User>(model.page_index, model.page_size, where.ToString(), "id asc", model).ToList();
                    rst.PageInfo = new PageInfo()
                    {
                        PageIndex = model.page_index,
                        PageSize = model.page_size,
                        TotalCount = conn.RecordCount<User>("")
                    };
                }
                else
                {
                    rst.Data = conn.GetList<User>("", model).ToList();
                }
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
