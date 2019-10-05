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
                sql.Append("role_id=@role_id,");
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
                    //获取权限菜单
                    var menus = conn.GetList<Menu>();
                    var auths = conn.Query<Menu>(@"SELECT
	                                                    t2.*
                                                    FROM
	                                                    `authority` t1
                                                    INNER JOIN menu t2 ON t1.menu_id = t2.menu_id
                                                    WHERE
	                                                    t1.role_id = @role_id
                                                    ORDER BY
	                                                    t2.sort_no ASC", new { rst.Data.role_id }).ToList();
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

        public ReturnResult<List<User>> GetList(User model)
        {
            var rst = new ReturnResult<List<User>>() { Message = "暂无记录" };

            try
            {
                var sql = new StringBuilder(@"SELECT
	                                                t1.*,t2.name role_name
                                                FROM
	                                                User t1
                                                LEFT JOIN Role t2 ON t1.role_id = t2.role_id ");
                if (model.page_index > 0)
                {
                    if (!string.IsNullOrWhiteSpace(model.search))
                    {
                        model.search = $"%{model.search}%";
                        sql.Append("where t1.user_name like @search ");
                        sql.Append("or t1.wechat like @search ");
                        sql.Append("or t1.tel like @search ");
                    }
                    rst.Data = conn.GetListPaged<User>(model.page_index, model.page_size, sql.ToString(), out int totalCount, "id asc", model).ToList();
                    rst.PageInfo = new PageInfo()
                    {
                        PageIndex = model.page_index,
                        PageSize = model.page_size,
                        TotalCount = totalCount
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
