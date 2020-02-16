using Dapper;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Dal
{
    public class MapDal : BaseDal<Map>
    {
        public ReturnResult<Map> GetOne(Map model, Trans t = null)
        {
            var rst = new ReturnResult<Map>() { Message = "暂无记录" };

            try
            {
                conn = CreateMySqlConnection(t);
                rst.Data = conn.QueryFirstOrDefault<Map>(@"SELECT
	                                                            t1.*, t2.`name` client_name,
                                                                t2.user_id,
	                                                            t3.user_name
                                                            FROM
	                                                            `map` t1
                                                            INNER JOIN client t2 ON t1.client_id = t2.id
                                                            INNER JOIN `user` t3 ON t2.user_id = t3.user_id
                                                            WHERE t1.id=@id", new { model.id }, t?.DbTrans);
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

        public ReturnResult<List<Map>> GetList(Map model, Trans t = null)
        {
            var rst = new ReturnResult<List<Map>>() { Message = "暂无记录" };

            try
            {
                conn = CreateMySqlConnection(t);
                var sql = new StringBuilder(@"SELECT
	                                                t1.*, t2.`name` client_name,
                                                    t2.user_id,
                                                    t2.is_online,
	                                                t3.user_name
                                                FROM
	                                                `map` t1
                                                INNER JOIN client t2 ON t1.client_id = t2.id
                                                INNER JOIN `user` t3 ON t2.user_id = t3.user_id ");
                bool is_admin = !string.IsNullOrWhiteSpace(model.user_id) && !model.is_admin;
                if (model.page_index > 0)
                {
                    if (!string.IsNullOrWhiteSpace(model.search))
                    {
                        model.search = $"%{model.search}%";
                        sql.Append("where (t1.name like @search ");
                        sql.Append("or t1.local like @search ");
                        sql.Append("or t1.remote like @search ");
                        sql.Append("or t2.name like @search ");
                        sql.Append("or t3.user_name like @search) ");
                        sql.Append(is_admin ? "and t3.user_id = @user_id " : "");
                    }
                    else
                    {
                        sql.Append(is_admin ? "where t3.user_id = @user_id " : "");
                    }
                    rst.Data = conn.GetListPaged<Map>(model.page_index, model.page_size, sql.ToString(), out int totalCount, "user_id, client_id, remote asc", model, t?.DbTrans).ToList();
                    rst.PageInfo = new PageInfo()
                    {
                        PageIndex = model.page_index,
                        PageSize = model.page_size,
                        TotalCount = totalCount
                    };
                }
                else
                {
                    sql.Append("order by t2.user_id,t1.client_id,t1.remote asc");
                    rst.Data = conn.Query<Map>(sql.ToString(), null, t?.DbTrans).ToList();
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

        public ReturnResult<List<Map>> GetMapList(string secret, Trans t = null)
        {
            var rst = new ReturnResult<List<Map>>() { Message = "暂无记录" };

            try
            {
                conn = CreateMySqlConnection(t);
                rst.Data = conn.Query<Map>(@"SELECT
	                                            t1.*, t2.`name` client_name,
                                                t2.user_id,
	                                            t3.user_name
                                            FROM
	                                            `map` t1
                                            INNER JOIN client t2 ON t1.client_id = t2.id
                                            INNER JOIN `user` t3 ON t2.user_id = t3.user_id
                                            WHERE t2.secret = @secret
                                            ORDER BY t2.user_id,t1.client_id,t1.remote", new { secret }, t?.DbTrans).ToList();
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
