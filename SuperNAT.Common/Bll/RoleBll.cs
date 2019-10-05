using Dapper;
using SuperNAT.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Common.Bll
{
    public class RoleBll : BaseBll<Role>
    {
        public ReturnResult<bool> AddRole(Role model)
        {
            var rst = new ReturnResult<bool>() { Message = "添加失败" };

            using var t = conn.BeginTransaction();
            try
            {
                if (conn.Insert(model, t) > 0)
                {
                    //插入角色菜单关联表
                    model.menu_ids?.ForEach(c => conn.Insert(new Authority() { menu_id = c, role_id = model.role_id }, t));
                    t.Commit();
                    rst.Result = true;
                    rst.Message = "添加成功";
                }
            }
            catch (Exception ex)
            {
                t.Rollback();
                rst.Message = $"添加失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }

        public ReturnResult<bool> UpdateRole(Role model)
        {
            var rst = new ReturnResult<bool>() { Message = "更新失败" };

            using var t = conn.BeginTransaction();
            try
            {
                if (conn.Update(model) > 0)
                {
                    //删除替换
                    conn.DeleteList<Authority>("where role_id=@role_id", new { model.role_id }, t);
                    //插入角色菜单关联表
                    model.menu_ids?.ForEach(c => conn.Insert(new Authority() { menu_id = c, role_id = model.role_id }, t));
                    t.Commit();
                    rst.Result = true;
                    rst.Message = "更新成功";
                }
            }
            catch (Exception ex)
            {
                t.Rollback();
                rst.Message = $"更新失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }

        public ReturnResult<Role> GetRole(Role model)
        {
            var rst = new ReturnResult<Role>() { Message = "暂无记录" };

            try
            {
                rst.Data = conn.Get<Role>(model.id);
                if (rst.Data != null)
                {
                    //获取角色列表
                    rst.Data.menu_ids = conn.GetList<Authority>("where role_id=@role_id",new { model.role_id }).Select(c => c.menu_id).ToList();
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

        public ReturnResult<List<Role>> GetList(Role model)
        {
            var rst = new ReturnResult<List<Role>>() { Message = "暂无记录" };

            try
            {
                var sql = new StringBuilder(@"select * from role ");
                if (model.page_index > 0)
                {
                    if (!string.IsNullOrWhiteSpace(model.search))
                    {
                        model.search = $"%{model.search}%";
                        sql.Append("where name like @search ");
                        sql.Append("or remark like @search ");
                    }
                    rst.Data = conn.GetListPaged<Role>(model.page_index, model.page_size, sql.ToString(), out int totalCount, "id asc", model).ToList();
                    rst.PageInfo = new PageInfo()
                    {
                        PageIndex = model.page_index,
                        PageSize = model.page_size,
                        TotalCount = totalCount
                    };
                    rst.Result = true;
                    rst.Message = "获取成功";
                }
                else
                {
                    sql.Append("order by id ");
                    rst.Data = conn.Query<Role>(sql.ToString()).ToList();
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
