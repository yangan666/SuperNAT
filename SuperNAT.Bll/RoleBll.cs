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

        public ReturnResult<bool> Add(Role model)
        {
            using (roleDal)
            {
                return roleDal.Add(model);
            }
        }

        public ReturnResult<bool> Update(Role model)
        {
            using (roleDal)
            {
                return roleDal.Update(model);
            }
        }

        public ReturnResult<bool> Delete(Role model)
        {
            using (roleDal)
            {
                return roleDal.Delete(model);
            }
        }

        public ReturnResult<Role> GetOne(Role model)
        {
            using (roleDal)
            {
                return roleDal.GetOne(model);
            }
        }

        public ReturnResult<List<Role>> GetList(string where)
        {
            using (roleDal)
            {
                return roleDal.GetList(where);
            }
        }

        public ReturnResult<bool> AddRole(Role model)
        {
            var rst = new ReturnResult<bool>() { Message = "添加失败" };

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

        public ReturnResult<bool> UpdateRole(Role model)
        {
            var rst = new ReturnResult<bool>() { Message = "更新失败" };

            try
            {
                using Trans t = new Trans();
                if (roleDal.Update(model, t).Result)
                {
                    //删除替换
                    authorityDal.DeleteList("where role_id=@role_id", new { model.role_id }, t);
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

        public ReturnResult<Role> GetRole(Role model)
        {
            using (roleDal)
            {
                return roleDal.GetRole(model);
            }
        }

        public ReturnResult<List<Role>> GetList(Role model)
        {
            using (roleDal)
            {
                return roleDal.GetList(model);
            }
        }
    }
}
