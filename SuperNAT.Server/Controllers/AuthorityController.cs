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
    public class AuthorityController : BaseController
    {
        [HttpPost]
        [Route("Add")]
        public IActionResult Add(Authority model)
        {
            var rst = new ApiResult<bool>();

            var bll = new AuthorityBll();
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
        public IActionResult Delete(Authority model)
        {
            var bll = new AuthorityBll();
            var rst = bll.Delete(model);

            return Json(rst);
        }

        [HttpPost]
        [Route("GetOne")]
        public IActionResult GetOne(Authority model)
        {
            if (model.id == 0)
            {
                var defalut = new ApiResult<Authority>()
                {
                    Result = true,
                    Data = new Authority()
                };
                return Json(defalut);
            }
            var bll = new AuthorityBll();
            var rst = bll.GetOne(model);
            return Json(rst);
        }

        [HttpPost]
        [Route("GetList")]
        public IActionResult GetList(Authority model)
        {
            var bll = new AuthorityBll();
            var rst = bll.GetList("");
            return Json(rst);
        }
    }
}