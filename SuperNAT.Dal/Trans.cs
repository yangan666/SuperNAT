using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using MySql.Data.MySqlClient;
using SuperNAT.Common;

namespace SuperNAT.Dal
{
    public class Trans : IDisposable
    {
        /// <summary>
        /// 数据库连接
        /// </summary>
        private DbConnection conn;
        /// <summary>
        /// 数据库事务
        /// </summary>
        private DbTransaction dbTrans;
        /// <summary>
        /// 数据库连接
        /// </summary>
        public DbConnection DbConnection
        {
            get { return this.conn; }
        }

        /// <summary>
        /// 数据库事务
        /// </summary>
        public DbTransaction DbTrans
        {
            get { return this.dbTrans; }
        }
        /// <summary>
        /// 是否提交
        /// </summary>
        public bool IsCommit { get; protected set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        public Trans()
        {
            conn = new MySqlConnection(GlobalConfig.ConnetionString);
            conn.Open();
            dbTrans = conn.BeginTransaction();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        public Trans(string connectionString)
        {
            conn = new MySqlConnection(connectionString);
            conn.Open();
            dbTrans = conn.BeginTransaction();
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void Commit()
        {
            dbTrans.Commit();
            this.IsCommit = true;
            this.Close();
        }
        /// <summary>
        /// 回滚事务
        /// </summary>
        public void RollBack()
        {
            dbTrans.Rollback();
            this.IsCommit = false;
            this.Close();
        }
        /// <summary>
        /// 销毁
        /// </summary>
        public void Dispose()
        {
            if (!this.IsCommit && conn.State != System.Data.ConnectionState.Closed)
                dbTrans.Rollback();

            this.Close();
        }
        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            if (conn.State == System.Data.ConnectionState.Open)
            {
                conn.Close();
            }
        }
    }
}
