using Dapper;
using SuperNAT.Dal;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Bll
{

    public class ServerConfigBll
    {
        private ServerConfigDal serverConfigDal = new ServerConfigDal();

        public ReturnResult<bool> Add(ServerConfig model)
        {
            using (serverConfigDal)
            {
                return serverConfigDal.Add(model);
            }
        }

        public ReturnResult<bool> Update(ServerConfig model)
        {
            using (serverConfigDal)
            {
                return serverConfigDal.Update(model);
            }
        }

        public ReturnResult<bool> Delete(ServerConfig model)
        {
            using (serverConfigDal)
            {
                return serverConfigDal.Delete(model);
            }
        }

        public ReturnResult<ServerConfig> GetOne(ServerConfig model)
        {
            using (serverConfigDal)
            {
                return serverConfigDal.GetOne(model);
            }
        }

        public ReturnResult<List<ServerConfig>> GetList(string where)
        {
            using (serverConfigDal)
            {
                return serverConfigDal.GetList(where);
            }
        }

        public ReturnResult<List<ServerConfig>> GetList(ServerConfig model)
        {
            using (serverConfigDal)
            {
                return serverConfigDal.GetList(model);
            }
        }

        public ReturnResult<string> GetServerConfig()
        {
            var rst = new ReturnResult<string>() { Message = "暂无开放的端口" };
            try
            {
                using (serverConfigDal)
                {
                    var data = serverConfigDal.GetServerConfig().Data ?? new List<ServerConfig>();
                    var strList = data.Select(s => $"{s.protocol}:{s.port}").ToList();
                    if (strList.Any())
                    {
                        rst.Result = true;
                        rst.Data = $"开放端口：{string.Join(" ", strList)}";
                    }
                }
            }
            catch (Exception ex)
            {
                Log4netUtil.Error($"获取失败：{ex}");
            }

            return rst;
        }
    }
}
