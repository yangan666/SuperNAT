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
    public class AuthorityBll
    {
        private AuthorityDal authorityDal = new AuthorityDal();

        public ApiResult<bool> Add(Authority model)
        {
            using (authorityDal)
            {
                return authorityDal.Add(model);
            }
        }

        public ApiResult<bool> Update(Authority model)
        {
            using (authorityDal)
            {
                return authorityDal.Update(model);
            }
        }

        public ApiResult<bool> Delete(Authority model)
        {
            using (authorityDal)
            {
                return authorityDal.Delete(model);
            }
        }

        public ApiResult<Authority> GetOne(Authority model)
        {
            using (authorityDal)
            {
                return authorityDal.GetOne(model);
            }
        }

        public ApiResult<List<Authority>> GetList(string where)
        {
            using (authorityDal)
            {
                return authorityDal.GetList(where);
            }
        }
    }
}
