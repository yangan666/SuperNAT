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
    public class MapBll
    {
        private MapDal mapDal = new MapDal();

        public ReturnResult<bool> Add(Map model)
        {
            using (mapDal)
            {
                return mapDal.Add(model);
            }
        }

        public ReturnResult<bool> Update(Map model)
        {
            using (mapDal)
            {
                return mapDal.Update(model);
            }
        }

        public ReturnResult<bool> Delete(Map model)
        {
            using (mapDal)
            {
                return mapDal.Delete(model);
            }
        }

        public ReturnResult<Map> GetOne(Map model)
        {
            using (mapDal)
            {
                return mapDal.GetOne(model);
            }
        }

        public ReturnResult<List<Map>> GetList(string where)
        {
            using (mapDal)
            {
                return mapDal.GetList(where);
            }
        }

        public ReturnResult<List<Map>> GetList(Map model)
        {
            using (mapDal)
            {
                return mapDal.GetList(model);
            }
        }

        public ReturnResult<List<Map>> GetMapList(string secret)
        {
            using (mapDal)
            {
                return mapDal.GetMapList(secret);
            }
        }
    }
}
