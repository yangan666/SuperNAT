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
    public class RoleBll
    {
        private RoleDal roleDal = new RoleDal();
        private AuthorityDal authorityDal = new AuthorityDal();

        public ApiResult<bool> Add(Role model)
        {
            using (roleDal)
            {
                return roleDal.Add(model);
            }
        }

        public ApiResult<bool> Update(Role model)
        {
            using (roleDal)
            {
                return roleDal.Update(model);
            }
        }

        public ApiResult<bool> Delete(Role model)
        {
            using (roleDal)
            {
                return roleDal.Delete(model);
            }
        }

        public ApiResult<Role> GetOne(Role model)
        {
            using (roleDal)
            {
                return roleDal.GetOne(model);
            }
        }

        public ApiResult<List<Role>> GetList(string where)
        {
            using (roleDal)
            {
                return roleDal.GetList(where);
            }
        }

        public ApiResult<bool> AddRole(Role model)
        {
            var rst = new ApiResult<bool>() { Message = "添加失败" };

            try
            {
                using Trans t = new Trans();
                if (roleDal.Add(model, t).Result)
                {
                    //插入角色菜单关联表
                    model.menu_ids?.ForEach(c => authorityDal.Add(new Authority() { menu_id = c, role_id = model.role_id }, t));
                    t.Commit();
                    rst.Result = true;
                    rst.Message = "添加成功";
                }
            }
            catch (Exception ex)
            {
                rst.Message = $"添加失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }

        public ApiResult<bool> UpdateRole(Role model)
        {
            var rst = new ApiResult<bool>() { Message = "更新失败" };

            try
            {
                using Trans t = new Trans();
                if (roleDal.Update(model, t).Result)
                {
                    //删除替换
                    authorityDal.DeleteCustom("where role_id=@role_id", new { model.role_id }, t);
                    //插入角色菜单关联表
                    model.menu_ids?.ForEach(c => authorityDal.Add(new Authority() { menu_id = c, role_id = model.role_id }, t));
                    t.Commit();
                    rst.Result = true;
                    rst.Message = "更新成功";
                }
            }
            catch (Exception ex)
            {
                rst.Message = $"更新失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }

        public ApiResult<Role> GetRole(Role model)
        {
            using (Trans t = new Trans())
            {
                var rst = roleDal.GetOne(model, t);
                if (!rst.Result)
                    return rst;
                //获取角色列表
                rst.Data.menu_ids = authorityDal.GetAll("where role_id=@role_id", new { model.role_id }, t).Select(c => c.menu_id).ToList();
                t.Commit();

                rst.Result = true;
                rst.Message = "获取成功";
                return rst;
            }
        }

        public ApiResult<List<Role>> GetList(Role model)
        {
            using (roleDal)
            {
                return roleDal.GetList(model);
            }
        }
    }
}
