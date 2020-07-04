using Dapper;
using SuperNAT.Common;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Dal
{
    public class RoleDal : BaseDal<Role>
    {
        public ApiResult<List<Role>> GetList(Role model, Trans t = null)
        {
            var rst = new ApiResult<List<Role>>() { Message = "暂无记录" };

            try
            {
                CreateMySqlConnection(t);
                var sql = new StringBuilder(@"select * from role ");
                if (model.page_index > 0)
                {
                    sql.Append($"where {"name,remark".ToLikeString("or", "search")} ".If(!string.IsNullOrWhiteSpace(model.search)));
                    model.search = $"%{model.search}%";
                    rst.Data = conn.GetListPaged<Role>(model.page_index, model.page_size, sql.ToString(), out int totalCount, "id asc", model, t?.DbTrans).ToList();
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
                    rst.Data = conn.Query<Role>(sql.ToString(), null, t?.DbTrans).ToList();
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
