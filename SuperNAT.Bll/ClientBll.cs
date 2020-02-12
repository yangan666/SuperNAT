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
            using (clientDal)
            {
                return clientDal.Delete(model);
            }
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
