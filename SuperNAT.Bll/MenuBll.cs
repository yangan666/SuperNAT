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

        public ReturnResult<bool> Add(Menu model)
        {
            using (menuDal)
            {
                return menuDal.Add(model);
            }
        }

        public ReturnResult<bool> Update(Menu model)
        {
            using (menuDal)
            {
                return menuDal.Update(model);
            }
        }

        public ReturnResult<bool> Delete(Menu model)
        {
            using (menuDal)
            {
                return menuDal.Delete(model);
            }
        }

        public ReturnResult<Menu> GetOne(Menu model)
        {
            using (menuDal)
            {
                return menuDal.GetOne(model);
            }
        }

        public ReturnResult<List<Menu>> GetList(string where)
        {
            using (menuDal)
            {
                return menuDal.GetList(where);
            }
        }

        public ReturnResult<List<Menu>> GetList(Menu model)
        {
            using (menuDal)
            {
                return menuDal.GetList(model);
            }
        }
    }
}
