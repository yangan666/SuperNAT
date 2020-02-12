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
    }
}
