using Dapper;
using SuperNAT.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Common.Bll
{
    public class MenuBll : BaseBll<Menu>
    {
        public ReturnResult<List<Menu>> GetList(Menu model)
        {
            var rst = new ReturnResult<List<Menu>>() { Message = "暂无记录" };

            try
            {
                var sql = new StringBuilder(@"SELECT
	                                                t1.*, t2.title p_title
                                                FROM
	                                                Menu t1
                                                LEFT JOIN Menu t2 ON t1.pid = t2.menu_id ");
                if (model.page_index > 0)
                {
                    if (!string.IsNullOrWhiteSpace(model.search))
                    {
                        model.search = $"%{model.search}%";
                        sql.Append("where t1.title like @search ");
                        sql.Append("or t1.name like @search ");
                        sql.Append("or t1.path like @search ");
                        sql.Append("or t1.component like @search ");
                        sql.Append("or t2.title like @search ");
                    }
                    rst.Data = conn.GetListPaged<Menu>(model.page_index, model.page_size, sql.ToString(), out int totalCount, "sort_no asc", model).ToList();
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
                    sql.Append("order by t1.id ");
                    rst.Data = conn.Query<Menu>(sql.ToString()).ToList();
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
