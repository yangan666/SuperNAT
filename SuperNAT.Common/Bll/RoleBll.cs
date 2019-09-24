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
