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
    public class MenuBll
    {
        private MenuDal menuDal = new MenuDal();

        public ApiResult<bool> Add(Menu model)
        {
            using (menuDal)
            {
                return menuDal.Add(model);
            }
        }

        public ApiResult<bool> Update(Menu model)
        {
            using (menuDal)
            {
                return menuDal.Update(model);
            }
        }

        public ApiResult<bool> Delete(Menu model)
        {
            using (menuDal)
            {
                return menuDal.Delete(model);
            }
        }

        public ApiResult<Menu> GetOne(Menu model)
        {
            using (menuDal)
            {
                return menuDal.GetOne(model);
            }
        }

        public ApiResult<List<Menu>> GetList(string where)
        {
            using (menuDal)
            {
                return menuDal.GetList(where);
            }
        }

        public ApiResult<List<Menu>> GetList(Menu model)
        {
            using (menuDal)
            {
                return menuDal.GetList(model);
            }
        }
    }
}
