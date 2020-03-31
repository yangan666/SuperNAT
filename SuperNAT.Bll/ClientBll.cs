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

        public ReturnResult<bool> Add(Client model)
        {
            using (clientDal)
            {
                return clientDal.Add(model);
            }
        }

        public ReturnResult<bool> Update(Client model)
        {
            using (clientDal)
            {
                return clientDal.Update(model);
            }
        }

        public ReturnResult<bool> Delete(Client model)
        {
            using var t = new Trans();
            var res = mapDal.DeleteList("where client_id=@client_id ", new { client_id = model.id });
            if (!res.Result)
            {
                return new ReturnResult<bool>() { Message = "删除失败" };
            }
            res = clientDal.Delete(model);
            t.Commit();
            return res;
        }

        public ReturnResult<Client> GetOne(Client model)
        {
            using (clientDal)
            {
                return clientDal.GetOne(model);
            }
        }

        public ReturnResult<List<Client>> GetList(string where)
        {
            using (clientDal)
            {
                return clientDal.GetList(where);
            }
        }


        public ReturnResult<Client> GetOne(string secret)
        {
            using (clientDal)
            {
                return clientDal.GetOne(secret);
            }
        }

        public ReturnResult<List<Client>> GetList(Client model)
        {
            using (clientDal)
            {
                return clientDal.GetList(model);
            }
        }

        public ReturnResult<bool> UpdateOnlineStatus(Client model)
        {
            using (clientDal)
            {
                return clientDal.UpdateOnlineStatus(model);
            }
        }

        public ReturnResult<bool> UpdateOfflineClient()
        {
            using (clientDal)
            {
                return clientDal.UpdateOfflineClient();
            }
        }
    }
}
