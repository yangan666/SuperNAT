using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SuperNAT.Common;
using SuperNAT.Bll;
using SuperNAT.Model;
using SuperNAT.Server.Models;

namespace SuperNAT.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : BaseController
    {
        [HttpPost]
        [Route("Add")]
        public IActionResult Add(Role model)
        {
            var rst = new ApiResult<bool>();

            var bll = new RoleBll();
            if (model.id == 0)
            {
                model.role_id = EncryptHelper.CreateGuid();
                rst = bll.AddRole(model);
            }
            else
            {
                rst = bll.UpdateRole(model);
            }

            return Json(rst);
        }

        [HttpPost]
        [Route("Delete")]
        public IActionResult Delete(Role model)
        {
            var bll = new RoleBll();
            var rst = bll.Delete(model);

            return Json(rst);
        }

        [HttpPost]
        [Route("GetOne")]
        public IActionResult GetOne(Role model)
        {
            if (model.id == 0)
            {
                var defalut = new ApiResult<Role>()
                {
                    Result = true,
                    Data = new Role()
                };
                return Json(defalut);
            }
            var bll = new RoleBll();
            var rst = bll.GetRole(model);
            return Json(rst);
        }

        [HttpPost]
        [Route("GetList")]
        public IActionResult GetList(Role model)
        {
            var bll = new RoleBll();
            var rst = bll.GetList(model);
            return Json(rst);
        }
    }
}
