using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Common.Bll
{
    public class BaseBll : IDisposable
    {
        public MySqlConnection conn;
        public BaseBll()
        {
            try
            {
                var connStr = "server=127.0.0.1;port=3306;User Id=root;Password=123456;Database=nat;pooling=false;character set=utf8;SslMode=none;";
                conn = new MySqlConnection(connStr);
                conn.Open();
            }
            catch (Exception ex)
            {
                Log4netUtil.Error("在BaseDal中打开连接时出错：" + ex);
            }
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
                Log4netUtil.Error("在BaseBll中关闭连接时出错：" + ex);
            }
        }
    }
}
