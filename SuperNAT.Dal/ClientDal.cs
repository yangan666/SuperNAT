using Dapper;
using SuperNAT.Common;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SuperNAT.Dal
{
    public class ClientDal : BaseDal<Client>
    {
        public ApiResult<Client> GetOne(string secret, Trans t = null)
        {
            var rst = new ApiResult<Client>() { Message = "暂无记录" };

            try
            {
                CreateMySqlConnection(t);
                rst.Data = conn.QueryFirstOrDefault<Client>(@"SELECT
	                                                                t1.*, t2.user_name
                                                                FROM
	                                                                client t1
                                                                LEFT JOIN `user` t2 ON t1.user_id = t2.user_id
                                                                WHERE
	                                                                t1.secret =@secret", new { secret }, t?.DbTrans);
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

        public ApiResult<List<Client>> GetList(Client model, Trans t = null)
        {
            var rst = new ApiResult<List<Client>>() { Message = "暂无记录" };

            try
            {
                CreateMySqlConnection(t);
                var sql = new StringBuilder(@"SELECT
	                                                t1.*, t2.user_name
                                                FROM
	                                                client t1
                                                LEFT JOIN `user` t2 ON t1.user_id = t2.user_id ");
                bool not_admin = !string.IsNullOrWhiteSpace(model.user_id) && !model.is_admin;
                if (model.page_index > 0)
                {
                    sql.Append($"where ({"t1.name,t1.remark,t2.user_name".ToLikeString("or", "search")}) {"and t2.user_id = @user_id ".If(not_admin)}".If(!string.IsNullOrWhiteSpace(model.search), "where t2.user_id = @user_id ".If(not_admin)));
                    model.search = $"%{model.search}%";
                    rst.Data = conn.GetListPaged<Client>(model.page_index, model.page_size, sql.ToString(), out int totalCount, "id desc", model, t?.DbTrans).ToList();
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
                    rst.Data = conn.Query<Client>(sql.ToString(), null, t?.DbTrans).ToList();
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

        public ApiResult<bool> UpdateOnlineStatus(Client model, Trans t = null)
        {
            var rst = new ApiResult<bool>() { Message = "更新失败" };

            try
            {
                CreateMySqlConnection(t);
                if (conn.Execute($"update client set is_online=@is_online{",last_heart_time=@last_heart_time".If(model.is_online)} where secret=@secret", model, t?.DbTrans) > 0)
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

        public ApiResult<bool> UpdateOfflineClient(Trans t = null)
        {
            var rst = new ApiResult<bool>() { Message = "更新失败" };

            try
            {
                CreateMySqlConnection(t);
                var clients = GetAll("where (is_online=@is_online && last_heart_time<@last_heart_time) or last_heart_time is null", new { is_online = true, last_heart_time = DateTime.Now.AddMinutes(-1) }, t).Select(c => c.id).ToList();
                if (clients.Any())
                {
                    int count = conn.Execute($"update client set is_online=0 where id in({string.Join(',', clients)})");
                    if (count > 0)
                    {
                        rst.Result = true;
                        rst.Message = $"更新假在线主机成功条数：{count}";
                    }
                }
                else
                {
                    rst.Message = "暂无需要更新的假在线主机";
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
