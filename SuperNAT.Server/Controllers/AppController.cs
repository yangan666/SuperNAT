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
    public class AppController
    {
        [HttpPost]
        [Route("Add")]
        public IActionResult Add(App model)
        {
            var rst = new ReturnResult<bool>();

            using var bll = new AppBll();
            if (model.id == 0)
            {
                model.secret = Guid.NewGuid().ToString("N");
                rst = bll.Add(model);
            }
            else
            {
                rst = bll.Update(model);
            }

            return new JsonResult(rst);
        }

        [HttpPost]
        [Route("Delete")]
        public IActionResult Delete(App model)
        {
            using var bll = new AppBll();
            var rst = bll.Delete(model);

            return new JsonResult(rst);
        }

        [HttpPost]
        [Route("GetOne")]
        public IActionResult GetOne(App model)
        {
            if (model.id == 0)
            {
                var defalut = new ReturnResult<App>()
                {
                    Result = true,
                    Data = new App()
                };
                return new JsonResult(defalut);
            }
            using var bll = new AppBll();
            var rst = bll.GetOne(model);
            return new JsonResult(rst);
        }

        [HttpPost]
        [Route("GetList")]
        public IActionResult GetList(App model)
        {
            using var bll = new AppBll();
            var rst = bll.GetList(model);
            return new JsonResult(rst);
        }
    }
