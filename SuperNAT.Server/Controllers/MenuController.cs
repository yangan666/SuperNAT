using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SuperNAT.Common;
using SuperNAT.Common.Bll;
using SuperNAT.Common.Models;

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
            var rst = new ReturnResult<bool>();

            using var bll = new MenuBll();
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
            using var bll = new MenuBll();
            var rst = bll.Delete(model);

            return Json(rst);
        }

        [HttpPost]
        [Route("GetOne")]
        public IActionResult GetOne(Menu model)
        {
            if (model.id == 0)
            {
                var defalut = new ReturnResult<Menu>()
                {
                    Result = true,
                    Data = new Menu()
                };
                return Json(defalut);
            }
            using var bll = new MenuBll();
            var rst = bll.GetOne(model);
            return Json(rst);
        }

        [HttpPost]
        [Route("GetList")]
        public IActionResult GetList(Menu model)
        {
            using var bll = new MenuBll();
            var rst = bll.GetList(model);
            return Json(rst);
        }

        [HttpPost]
        [Route("GetParentList")]
        public IActionResult GetParentList()
        {
            using var bll = new MenuBll();
            var rst = bll.GetList("where pid is null or pid = ''");
            return Json(rst);
        }
    }
}
