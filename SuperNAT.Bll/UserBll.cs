using Dapper;
using SuperNAT.Common;
using SuperNAT.Dal;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Bll
{
    public class UserBll
    {
        private UserDal userDal = new UserDal();

        public ReturnResult<bool> Add(User model)
        {
            using (userDal)
            {
                return userDal.Add(model);
            }
        }

        public ReturnResult<bool> Update(User model)
        {
            using (userDal)
            {
                return userDal.Update(model);
            }
        }

        public ReturnResult<bool> Delete(User model)
        {
            using (userDal)
            {
                return userDal.Delete(model);
            }
        }

        public ReturnResult<User> GetOne(User model)
        {
            using (userDal)
            {
                return userDal.GetOne(model);
            }
        }

        public ReturnResult<List<User>> GetList(string where)
        {
            using (userDal)
            {
                return userDal.GetList(where);
            }
        }

        public ReturnResult<User> Login(User model)
        {
            using (userDal)
            {
                return userDal.Login(model);
            }
        }

        public ReturnResult<bool> UpdateUser(User model)
        {
            using (userDal)
            {
                return userDal.UpdateUser(model);
            }
        }

        public ReturnResult<bool> DisableUser(User model)
        {
            using (userDal)
            {
                return userDal.DisableUser(model);
            }
        }

        public ReturnResult<User> GetUserInfo(string user_id)
        {
            using (userDal)
            {
                return userDal.GetUserInfo(user_id);
            }
        }

        public ReturnResult<List<User>> GetList(User model)
        {
            using (userDal)
            {
                return userDal.GetList(model);
            }
        }
    }
}
