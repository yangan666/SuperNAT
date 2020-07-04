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
    public class ClientBll
    {
        private ClientDal clientDal = new ClientDal();
        private MapDal mapDal = new MapDal();

        public ApiResult<bool> Add(Client model)
        {
            using (clientDal)
            {
                return clientDal.Add(model);
            }
        }

        public ApiResult<bool> Update(Client model)
        {
            using (clientDal)
            {
                return clientDal.Update(model);
            }
        }

        public ApiResult<bool> Delete(Client model)
        {
            using var t = new Trans();
            var res = mapDal.DeleteCustom("where client_id=@client_id ", new { client_id = model.id });
            if (!res.Result)
            {
                return new ApiResult<bool>() { Message = "删除失败" };
            }
            res = clientDal.Delete(model);
            t.Commit();
            return res;
        }

        public ApiResult<Client> GetOne(Client model)
        {
            using (clientDal)
            {
                return clientDal.GetOne(model);
            }
        }

        public ApiResult<List<Client>> GetList(string where)
        {
            using (clientDal)
            {
                return clientDal.GetList(where);
            }
        }


        public ApiResult<Client> GetOne(string secret)
        {
            using (clientDal)
            {
                return clientDal.GetOne(secret);
            }
        }

        public ApiResult<List<Client>> GetList(Client model)
        {
            using (clientDal)
            {
                return clientDal.GetList(model);
            }
        }

        public ApiResult<bool> UpdateOnlineStatus(Client model)
        {
            using (clientDal)
            {
                return clientDal.UpdateOnlineStatus(model);
            }
        }

        public ApiResult<bool> UpdateOfflineClient()
        {
            using (clientDal)
            {
                return clientDal.UpdateOfflineClient();
            }
        }
    }
}
