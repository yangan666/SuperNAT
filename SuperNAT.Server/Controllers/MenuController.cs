using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SuperNAT.Common;
using SuperNAT.Bll;
using SuperNAT.Model;

namespace SuperNAT.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : BaseController
    {
        [HttpPost]
        [Route("Add")]
        public IActionResult Add(Menu model)
        {
            var rst = new ApiResult<bool>();

            var bll = new MenuBll();
            if (model.id == 0)
            {
                model.menu_id = EncryptHelper.CreateGuid();
                rst = bll.Add(model);
            }
            else
            {
                rst = bll.Update(model);
            }

            return Json(rst);
        }

        [HttpPost]
        [Route("Delete")]
        public IActionResult Delete(Menu model)
        {
            var bll = new MenuBll();
            var rst = bll.Delete(model);

            return Json(rst);
        }

        [HttpPost]
        [Route("GetOne")]
        public IActionResult GetOne(Menu model)
        {
            if (model.id == 0)
            {
                var defalut = new ApiResult<Menu>()
                {
                    Result = true,
                    Data = new Menu()
                };
                return Json(defalut);
            }
            var bll = new MenuBll();
            var rst = bll.GetOne(model);
            return Json(rst);
        }

        [HttpPost]
        [Route("GetList")]
        public IActionResult GetList(Menu model)
        {
            var bll = new MenuBll();
            var rst = bll.GetList(model);
            return Json(rst);
        }

        [HttpPost]
        [Route("GetParentList")]
        public IActionResult GetParentList()
        {
            var bll = new MenuBll();
            var rst = bll.GetList("where pid is null or pid = '' order by sort_no");
            return Json(rst);
        }

        [HttpPost]
        [Route("GetAll")]
        public IActionResult GetAll()
        {
            var bll = new MenuBll();
            var rst = bll.GetList("order by sort_no");
            return Json(rst);
        }
    }
}
