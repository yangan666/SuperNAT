using Dapper;
using SuperNAT.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Common.Bll
{
    public class ServerConfigBll : BaseBll<ServerConfig>
    {
        public ReturnResult<List<ServerConfig>> GetList(ServerConfig model)
        {
            var rst = new ReturnResult<List<ServerConfig>>() { Message = "暂无记录" };

            try
            {
                var sql = new StringBuilder(@"SELECT * FROM `server_config` ");
                if (model.page_index > 0)
                {
                    if (!string.IsNullOrWhiteSpace(model.search))
                    {
                        model.search = $"%{model.search}%";
                        sql.Append("where protocol like @search ");
                        sql.Append("where port like @search ");
                        sql.Append("where ssl_type like @search ");
                        sql.Append("where certfile like @search ");
                        sql.Append("where certpwd like @search ");
                    }
                    rst.Data = conn.GetListPaged<ServerConfig>(model.page_index, model.page_size, sql.ToString(), out int totalCount, "id asc", model).ToList();
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
                    rst.Data = conn.Query<ServerConfig>(sql.ToString()).ToList();
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
