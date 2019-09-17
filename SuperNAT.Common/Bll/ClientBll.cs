using Dapper;
using SuperNAT.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SuperNAT.Common.Bll
{
    public class ClientBll : BaseBll<Client>
    {
        public ReturnResult<Client> GetOne(string secret)
        {
            var rst = new ReturnResult<Client>() { Message = "暂无记录" };

            try
            {
                rst.Data = conn.QueryFirstOrDefault<Client>("select * from client where secret=@secret", new { secret });
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

        public ReturnResult<List<Client>> GetList(Client model)
        {
            var rst = new ReturnResult<List<Client>>() { Message = "暂无记录" };

            try
            {
                if (model.page_index > 0)
                {
                    var where = new StringBuilder();
                    if (!string.IsNullOrWhiteSpace(model.name))
                    {
                        model.name = $"%{model.name}%";
                        where.Append("where t1.name like @name ");
                        where.Append("or t1.remark like @name ");
                        where.Append("or t2.user_name like @name ");
                    }
                    var query = conn.Query<Client>(@"SELECT
	                                                t1.*, t2.user_name
                                                FROM
	                                                client t1
                                                LEFT JOIN `user` t2 ON t1.user_id = t2.user_id " + where.ToString(), model);
                    rst.Data = query.Skip(model.page_size * (model.page_index - 1)).Take(model.page_size).ToList();
                    rst.PageInfo = new PageInfo()
                    {
                        PageIndex = model.page_index,
                        PageSize = model.page_size,
                        TotalCount = query.Count()
                    };
                }
                else
                {
                    rst.Data = conn.Query<Client>(@"SELECT
	                                                t1.*, t2.user_name
                                                FROM
	                                                client t1
                                                LEFT JOIN `user` t2 ON t1.user_id = t2.user_id", model).ToList();
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

        public ReturnResult<bool> UpdateOnlineStatus(Client model)
        {
            var rst = new ReturnResult<bool>() { Message = "更新失败" };

            try
            {
                if (conn.Execute($"update client set is_online=@is_online{(model.is_online ? ",last_heart_time=@last_heart_time" : "")} where secret=@secret", model) > 0)
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
    }
}
