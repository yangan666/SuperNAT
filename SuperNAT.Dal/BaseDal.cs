using Dapper;
using MySql.Data.MySqlClient;
using SuperNAT.Common;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace SuperNAT.Dal
{
    public class BaseDal<T> : IDisposable
    {
        public DbConnection conn;
        public BaseDal()
        {

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

        public ReturnResult<bool> Add(T model, Trans t = null)
        {
            var rst = new ReturnResult<bool>() { Message = "添加失败" };

            try
            {
                conn = CreateMySqlConnection(t);
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

        public ReturnResult<bool> Update(T model, Trans t = null)
        {
            var rst = new ReturnResult<bool>() { Message = "更新失败" };

            try
            {
                conn = CreateMySqlConnection(t);
                if (conn.Update(model, t?.DbTrans) > 0)
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

        public ReturnResult<bool> Delete(T model, Trans t = null)
        {
            var rst = new ReturnResult<bool>() { Message = "删除失败" };

            try
            {
                conn = CreateMySqlConnection(t);
                if (conn.Delete(model, t?.DbTrans) > 0)
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

        public ReturnResult<bool> DeleteList(string where, object param, Trans t = null)
        {
            var rst = new ReturnResult<bool>() { Message = "批量删除失败" };

            try
            {
                conn = CreateMySqlConnection(t);
                if (conn.DeleteList<T>(where, param, t?.DbTrans) > 0)
                {
                    rst.Result = true;
                    rst.Message = "批量删除成功";
                }
            }
            catch (Exception ex)
            {
                rst.Message = $"批量删除失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }

        public ReturnResult<T> GetOne(IModel model, Trans t = null)
        {
            var rst = new ReturnResult<T>() { Message = "暂无记录" };

            try
            {
                conn = CreateMySqlConnection(t);
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

        public ReturnResult<List<T>> GetList(string where, Trans t = null)
        {
            var rst = new ReturnResult<List<T>>() { Message = "暂无记录" };

            try
            {
                conn = CreateMySqlConnection(t);
                rst.Data = conn.GetList<T>(where, null, t?.DbTrans).ToList();
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
