using Dapper;
using SuperNAT.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Common.Bll
{
    public class UserBll : BaseBll
    {
        public ReturnResult<bool> Add(User model)
        {
            var rst = new ReturnResult<bool>();

            try
            {
                model.password = EncryptHelper.MD5Encrypt(model.password);
                rst.Result = conn.Insert(model) > 0;
                rst.Message = "添加成功";
            }
            catch (Exception ex)
            {
                rst.Message = $"添加失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }

        public ReturnResult<bool> Update(User model)
        {
            var rst = new ReturnResult<bool>();

            try
            {
                if (!string.IsNullOrEmpty(model.password))
                {
                    model.password = EncryptHelper.MD5Encrypt(model.password);
                }
                rst.Result = conn.Update(model) > 0;
                rst.Message = "更新成功";
            }
            catch (Exception ex)
            {
                rst.Message = $"更新失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }

        public ReturnResult<bool> Delete(User model)
        {
            var rst = new ReturnResult<bool>();

            try
            {
                rst.Result = conn.Delete(model) > 0;
                rst.Message = "删除成功";
            }
            catch (Exception ex)
            {
                rst.Message = $"删除失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }

        public ReturnResult<User> Login(User model)
        {
            var rst = new ReturnResult<User>();

            try
            {
                rst.Data = conn.QueryFirstOrDefault<User>("select * from user where user_name=@user_name and password=@password", model);
                rst.Result = true;
                rst.Message = "获取成功";
            }
            catch (Exception ex)
            {
                rst.Message = $"获取失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }

        public ReturnResult<User> GetOne(User model)
        {
            var rst = new ReturnResult<User>();

            try
            {
                rst.Data = conn.Get<User>(model.id);
                if (rst.Data != null)
                {
                    rst.Data.password = "";
                }
                rst.Result = true;
                rst.Message = "获取成功";
            }
            catch (Exception ex)
            {
                rst.Message = $"获取失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }

        public ReturnResult<List<User>> GetList(User model)
        {
            var rst = new ReturnResult<List<User>>();

            try
            {
                rst.Data = conn.GetList<User>("", model).ToList();
                rst.Result = true;
                rst.Message = "获取成功";
            }
            catch (Exception ex)
            {
                rst.Message = $"获取失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }
    }
}
