using Dapper;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Dal
{
    public class ServerConfigDal : BaseDal<ServerConfig>
    {
        public ReturnResult<List<ServerConfig>> GetList(ServerConfig model, Trans t = null)
        {
            var rst = new ReturnResult<List<ServerConfig>>() { Message = "暂无记录" };

            try
            {
                conn = CreateMySqlConnection(t);
                var sql = new StringBuilder(@"SELECT * FROM `server_config` ");
                if (model.page_index > 0)
                {
                    if (!string.IsNullOrWhiteSpace(model.search))
                    {
                        model.search = $"%{model.search}%";
                        sql.Append("where protocol like @search ");
                        sql.Append("or port like @search ");
                        sql.Append("or ssl_type like @search ");
                        sql.Append("or certfile like @search ");
                        sql.Append("or certpwd like @search ");
                    }
                    rst.Data = conn.GetListPaged<ServerConfig>(model.page_index, model.page_size, sql.ToString(), out int totalCount, "id asc", model, t?.DbTrans).ToList();
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
                    rst.Data = conn.Query<ServerConfig>(sql.ToString(), null, t?.DbTrans).ToList();
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
