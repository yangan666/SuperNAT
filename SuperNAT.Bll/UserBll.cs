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

        public ApiResult<bool> Add(User model)
        {
            using (Trans t = new Trans())
            {
                var user = userDal.GetAll("where user_name=@user_name", model, t);
                if (user.Any())
                    return ApiResult<bool>.Fail("用户名已存在！");
                var add = userDal.Add(model, t);
                if (!add.Result)
                    return add;

                t.Commit();
                add.Message = "注册成功";
                return add;
            }
        }

        public ApiResult<bool> Update(User model)
        {
            using (userDal)
            {
                return userDal.Update(model);
            }
        }

        public ApiResult<bool> Delete(User model)
        {
            using (userDal)
            {
                return userDal.Delete(model);
            }
        }

        public ApiResult<User> GetOne(User model)
        {
            using (userDal)
            {
                return userDal.GetOne(model);
            }
        }

        public ApiResult<List<User>> GetList(string where)
        {
            using (userDal)
            {
                return userDal.GetList(where);
            }
        }

        public ApiResult<User> Login(User model)
        {
            using (Trans t = new Trans())
            {
                var login = userDal.Login(model, t);
                if (!login.Result)
                    return login;

                login.Data.last_login_time = DateTime.Now;
                userDal.UpdateLastLogin(login.Data, t);

                t.Commit();
                return login;
            }
        }

        public ApiResult<bool> UpdateUser(User model)
        {
            using (userDal)
            {
                return userDal.UpdateUser(model);
            }
        }

        public ApiResult<bool> DisableUser(User model)
        {
            using (userDal)
            {
                return userDal.DisableUser(model);
            }
        }

        public ApiResult<User> GetUserInfo(string user_id)
        {
            using (userDal)
            {
                return userDal.GetUserInfo(user_id);
            }
        }

        public ApiResult<List<User>> GetList(User model)
        {
            using (userDal)
            {
                return userDal.GetList(model);
            }
        }
    }
}
