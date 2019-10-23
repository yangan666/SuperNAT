using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SuperNAT.Common.Bll;
using SuperNAT.Common.Models;

namespace SuperNAT.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServerConfigController : BaseController
    {
        [HttpPost]
        [Route("Add")]
        public IActionResult Add(ServerConfig model)
        {
            var rst = new ReturnResult<bool>();

            using var bll = new ServerConfigBll();
            if (model.id == 0)
            {
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
        public IActionResult Delete(ServerConfig model)
        {
            using var bll = new ServerConfigBll();
            var rst = bll.Delete(model);

            return Json(rst);
        }

        [HttpPost]
        [Route("GetOne")]
        public IActionResult GetOne(ServerConfig model)
        {
            if (model.id == 0)
            {
                var defalut = new ReturnResult<ServerConfig>()
                {
                    Result = true,
                    Data = new ServerConfig()
                };
                return Json(defalut);
            }
            using var bll = new ServerConfigBll();
            var rst = bll.GetOne(model);
            return Json(rst);
        }

        [HttpPost]
        [Route("GetList")]
        public IActionResult GetList(ServerConfig model)
        {
            using var bll = new ServerConfigBll();
            var rst = bll.GetList(model);
            return Json(rst);
        }
    }
}
