using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SuperNAT.Bll;
using SuperNAT.Model;

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
            var rst = new ApiResult<bool>();

            var bll = new ServerConfigBll();
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
            var bll = new ServerConfigBll();
            var rst = bll.Delete(model);

            return Json(rst);
        }

        [HttpPost]
        [Route("GetOne")]
        public IActionResult GetOne(ServerConfig model)
        {
            if (model.id == 0)
            {
                var defalut = new ApiResult<ServerConfig>()
                {
                    Result = true,
                    Data = new ServerConfig()
                };
                return Json(defalut);
            }
            var bll = new ServerConfigBll();
            var rst = bll.GetOne(model);
            return Json(rst);
        }

        [HttpPost]
        [Route("GetList")]
        public IActionResult GetList(ServerConfig model)
        {
            var bll = new ServerConfigBll();
            var rst = bll.GetList(model);
            return Json(rst);
        }

        [HttpPost]
        [Route("GetServerConfig")]
        public IActionResult GetServerConfig()
        {
            var bll = new ServerConfigBll();
            var rst = bll.GetServerConfig();
            return Json(rst);
        }
    }
}
