using Dapper;
using Dapper.Contrib.Extensions;
using SuperNAT.Common;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Dal
{
    public class UserDal : BaseDal<User>
    {
        public ApiResult<User> Login(User model, Trans t = null)
        {
            var rst = new ApiResult<User>() { Message = "用户名或密码错误" };

            try
            {
                CreateMySqlConnection(t);
                model.password = EncryptHelper.MD5Encrypt(model.password);
                var sql = new StringBuilder("select * from user where password=@password ");
                if (string.IsNullOrEmpty(model.user_name) && string.IsNullOrEmpty(model.user_id))
                    throw new Exception("用户名或用户ID两者不能全为空");
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
                rst.Data = conn.QueryFirstOrDefault<User>(sql.ToString(), model, t?.DbTrans);
                if (rst.Data != null)
                {
                    if (rst.Data.is_disabled)
                    {
                        rst.Result = false;
                        rst.Message = "您的帐号已被禁用，请联系管理员解除禁用！";
                        return rst;
                    }
                    rst.Result = true;
                    rst.Message = "登录成功";
                }
            }
            catch (Exception ex)
            {
                rst.Message = $"{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }

        public ApiResult<bool> UpdateLastLogin(User model, Trans t = null)
        {
            var rst = new ApiResult<bool>() { Message = "更新失败" };

            try
            {
                CreateMySqlConnection(t);
                var sql = new StringBuilder();
                sql.Append("update user set ");
                sql.Append("last_login_time=@last_login_time,");
                sql.Append("login_times=login_times+1 ");
                sql.Append("where user_id=@user_id ");
                if (conn.Execute(sql.ToString(), model, t?.DbTrans) > 0)
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

        public ApiResult<bool> UpdateUser(User model, Trans t = null)
        {
            var rst = new ApiResult<bool>() { Message = "更新失败" };

            try
            {
                CreateMySqlConnection(t);
                var sql = new StringBuilder();
                sql.Append("update user set ");
                sql.Append("user_name=@user_name,");
                sql.Append("role_id=@role_id,");
                sql.Append("password=@password,".If(!string.IsNullOrWhiteSpace(model.password)));
                sql.Append("wechat=@wechat,");
                sql.Append("email=@email,");
                sql.Append("tel=@tel ");
                sql.Append("where user_id=@user_id ");
                if (conn.Execute(sql.ToString(), model, t?.DbTrans) > 0)
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

        public ApiResult<bool> DisableUser(User model, Trans t = null)
        {
            var rst = new ApiResult<bool>() { Message = "更新失败" };

            try
            {
                CreateMySqlConnection(t);
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

        public ApiResult<User> GetUserInfo(string user_id, Trans t = null)
        {
            var rst = new ApiResult<User>() { Message = "用户名或密码错误" };

            try
            {
                CreateMySqlConnection(t);
                rst.Data = conn.QueryFirstOrDefault<User>("select * from user where user_id=@user_id ", new { user_id }, t?.DbTrans);
                if (rst.Data != null)
                {
                    //获取权限菜单
                    var menus = conn.GetAll<Menu>();
                    var auths = conn.Query<Menu>(@"SELECT
	                                                    t2.*
                                                    FROM
	                                                    `authority` t1
                                                    INNER JOIN menu t2 ON t1.menu_id = t2.menu_id
                                                    WHERE
	                                                    t1.role_id = @role_id
                                                    ORDER BY
	                                                    t2.sort_no ASC", new { rst.Data.role_id }, t?.DbTrans).ToList();
                    var children = auths.FindAll(c => !string.IsNullOrWhiteSpace(c.pid));
                    foreach (var item in children)
                    {
                        //只选中一个子级菜单的情况，父级菜单没入库，需要手动找出来
                        var father = auths.Find(c => c.menu_id == item.pid);
                        if (father == null)
                        {
                            father = menus.FirstOrDefault(c => c.menu_id == item.pid);
                            if (father != null)
                            {
                                auths.Add(father);
                            }
                        }
                    }
                    rst.Data.menu_list = auths.OrderBy(c => c.sort_no).ToList();
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

        public ApiResult<List<User>> GetList(User model, Trans t = null)
        {
            var rst = new ApiResult<List<User>>() { Message = "暂无记录" };

            try
            {
                CreateMySqlConnection(t);
                var sql = new StringBuilder(@"SELECT
	                                                t1.*,t2.name role_name
                                                FROM
	                                                User t1
                                                LEFT JOIN Role t2 ON t1.role_id = t2.role_id ");
                if (model.page_index > 0)
                {
                    sql.Append($"where {"t1.user_name,t1.wechat,t1.email,t1.tel".ToLikeString("or", "search")} ".If(!string.IsNullOrWhiteSpace(model.search)));
                    model.search = $"%{model.search}%";
                    rst.Data = conn.GetListPaged<User>(model.page_index, model.page_size, sql.ToString(), out int totalCount, "id desc", model, t?.DbTrans).ToList();
                    rst.PageInfo = new PageInfo()
                    {
                        PageIndex = model.page_index,
                        PageSize = model.page_size,
                        TotalCount = totalCount
                    };
                }
                else
                {
                    rst.Data = GetAll("", null, t).ToList();
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
