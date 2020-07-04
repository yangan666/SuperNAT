using Dapper;
using Dapper.Contrib.Extensions;
using MySql.Data.MySqlClient;
using SuperNAT.Common;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SuperNAT.Dal
{
    public class BaseDal<T> : IDisposable where T : class, new()
    {
        public DbConnection conn;
        private static Dictionary<string, string> tableDict = new Dictionary<string, string>();
        public BaseDal()
        {

        }

        public string GetTableName()
        {
            var type = typeof(T);
            if (tableDict.ContainsKey(type.Name))
            {
                return tableDict[type.Name];
            }
            var name = ((TableAttribute)type.GetCustomAttribute(typeof(TableAttribute), true))?.Name ?? type.Name;
            lock (tableDict)
                tableDict.Add(type.Name, name);
            return name.StartsWith("`") && name.EndsWith("`") ? name : $"`{name}`";
        }

        public DbConnection CreateMySqlConnection(Trans t = null)
        {
            if (t == null)
            {
                conn = new MySqlConnection(GlobalConfig.ConnetionString);
                conn.Open();
            }
            else
            {
                conn = t.DbConnection;
            }
            return conn;
        }

        public ApiResult<bool> Add(T model, Trans t = null)
        {
            var rst = new ApiResult<bool>() { Message = "添加失败" };

            try
            {
                CreateMySqlConnection(t);
                if (conn.Insert(model, t?.DbTrans) > 0)
                {
                    rst.Result = true;
                    rst.Message = "添加成功";
                }
            }
            catch (Exception ex)
            {
                rst.Message = $"添加失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }

        public virtual ApiResult<bool> AddList(List<T> list, Trans t = null)
        {
            var rst = new ApiResult<bool>() { Message = "批量添加失败" };

            try
            {
                CreateMySqlConnection(t);
                conn.Insert(list, t?.DbTrans);
                rst.Result = true;
                rst.Message = "批量添加成功";
            }
            catch (Exception ex)
            {
                rst.Message = $"添加失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }

        public ApiResult<bool> Update(T model, Trans t = null)
        {
            var rst = new ApiResult<bool>() { Message = "更新失败" };

            try
            {
                CreateMySqlConnection(t);
                if (conn.Update(model, t?.DbTrans))
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

        public ApiResult<bool> Delete(T model, Trans t = null)
        {
            var rst = new ApiResult<bool>() { Message = "删除失败" };

            try
            {
                CreateMySqlConnection(t);
                if (conn.Delete(model, t?.DbTrans))
                {
                    rst.Result = true;
                    rst.Message = "删除成功";
                }
            }
            catch (Exception ex)
            {
                rst.Message = $"删除失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }

        public ApiResult<bool> DeleteCustom(string where, object model, Trans t = null)
        {
            var rst = new ApiResult<bool>() { Message = "批量删除失败" };

            try
            {
                CreateMySqlConnection(t);
                conn.Execute($"delete from {GetTableName()} {where}", model, t?.DbTrans);
                rst.Result = true;
                rst.Message = "删除成功";
            }
            catch (Exception ex)
            {
                rst.Message = $"批量删除失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }

        public ApiResult<T> GetOne(IModel model, Trans t = null)
        {
            var rst = new ApiResult<T>() { Message = "暂无记录" };

            try
            {
                CreateMySqlConnection(t);
                rst.Data = conn.Get<T>(model.id, t?.DbTrans);
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

        public virtual ApiResult<List<T>> GetList(string where, string orderBy = "", int pageIndex = 0, int pageSize = 10, object parameters = null, Trans t = null)
        {
            var rst = new ApiResult<List<T>>() { Message = "暂无记录" };

            try
            {
                CreateMySqlConnection(t);
                if (pageIndex > 0)
                {
                    var sql = $"select * from {GetTableName()} {where}";
                    rst.Data = conn.GetListPaged<T>(pageIndex, pageSize, sql, out int totalCount, orderBy, parameters, t?.DbTrans).ToList();
                    rst.PageInfo = new PageInfo()
                    {
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        TotalCount = totalCount
                    };
                }
                else
                {
                    rst.Data = GetAll($"{where} {$"order by {orderBy}".If(!string.IsNullOrEmpty(orderBy))}", parameters, t).ToList();
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

        public virtual List<T> GetAll(string where, object parameters = null, Trans t = null)
        {
            CreateMySqlConnection(t);
            return conn.Query<T>($"select * from {GetTableName()} {where}", parameters, t?.DbTrans).ToList();
        }

        public void Dispose()
        {
            try
            {
                if (conn != null && conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }
                conn?.Dispose();
            }
            catch (Exception ex)
            {
                Log4netUtil.Error("在BaseDal中关闭连接时出错：" + ex);
            }
        }
    }

    public static class MySimpleCRUD
    {
        public static IEnumerable<T> GetListPaged<T>(this IDbConnection connection, int pageIndex, int pageSize, string sql, out int count, string orderby = "", object parameters = default, DbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (!string.IsNullOrWhiteSpace(orderby))
            {
                orderby = "order by " + string.Join(",", orderby.Split(',').Select(s => $" t1.{s.Trim()}"));
            }
            var pageSql = $"select sql_calc_found_rows * from ({sql}) t1 {orderby} limit {(pageIndex - 1) * pageSize},{pageSize};select found_rows()";
            var query = connection.QueryMultiple(pageSql, parameters, transaction, commandTimeout, commandType);
            var table = query.Read<T>();
            count = query.ReadFirstOrDefault<int>();
            return table;
        }
    }
}
