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
    public class MapDal : BaseDal<Map>
    {
        public ApiResult<Map> IsExit(Map model, Trans t = null)
        {
            var rst = new ApiResult<Map>() { Message = "暂无记录" };

            try
            {
                CreateMySqlConnection(t);
                var sql = new StringBuilder("select * from map where ");
                sql.Append("remote=@remote and remote_port=@remote_port ".If(model.protocol.Contains("http")));
                sql.Append("remote_port=@remote_port ".If(model.protocol == "tcp" || model.protocol == "udp"));
                sql.Append("and id!=@id".If(model.id > 0));
                rst.Data = conn.QueryFirstOrDefault<Map>(sql.ToString(), model, t?.DbTrans);
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

        public ApiResult<Map> GetOne(Map model, Trans t = null)
        {
            var rst = new ApiResult<Map>() { Message = "暂无记录" };

            try
            {
                CreateMySqlConnection(t);
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

        public ApiResult<List<Map>> GetList(Map model, Trans t = null)
        {
            var rst = new ApiResult<List<Map>>() { Message = "暂无记录" };

            try
            {
                CreateMySqlConnection(t);
                var sql = new StringBuilder(@"SELECT
	                                                t1.*, t2.`name` client_name,
                                                    t2.user_id,
                                                    t2.is_online,
	                                                t3.user_name
                                                FROM
	                                                `map` t1
                                                INNER JOIN client t2 ON t1.client_id = t2.id
                                                INNER JOIN `user` t3 ON t2.user_id = t3.user_id ");
                bool not_admin = !string.IsNullOrWhiteSpace(model.user_id) && !model.is_admin;
                if (model.page_index > 0)
                {
                    sql.Append($"where ({"t1.name,t1.local,t1.remote,t2.name,t3.user_name".ToLikeString("or", "search")}) {"and t3.user_id = @user_id ".If(not_admin)}".If(!string.IsNullOrWhiteSpace(model.search), "where t3.user_id = @user_id ".If(not_admin)));
                    model.search = $"%{model.search}%";
                    rst.Data = conn.GetListPaged<Map>(model.page_index, model.page_size, sql.ToString(), out int totalCount, "id desc", model, t?.DbTrans).ToList();
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

        public ApiResult<List<Map>> GetMapList(string secret, Trans t = null)
        {
            var rst = new ApiResult<List<Map>>() { Message = "暂无记录" };

            try
            {
                CreateMySqlConnection(t);
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
