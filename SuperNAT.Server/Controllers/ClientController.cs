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
    public class ClientController : BaseController
    {
        [HttpPost]
        [Route("Add")]
        public IActionResult Add(Client model)
        {
            var rst = new ApiResult<bool>();

            var bll = new ClientBll();
            if (model.id == 0)
            {
                model.secret = Guid.NewGuid().ToString("N");
                model.is_online = false;
                model.create_time = DateTime.Now;
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
        public IActionResult Delete(Client model)
        {
            var bll = new ClientBll();
            var rst = bll.Delete(model);

            return Json(rst);
        }

        [HttpPost]
        [Route("GetOne")]
        public IActionResult GetOne(Client model)
        {
            if (model.id == 0)
            {
                var defalut = new ApiResult<Client>()
                {
                    Result = true,
                    Data = new Client()
                };
                return Json(defalut);
            }
            var bll = new ClientBll();
            var rst = bll.GetOne(model);
            return Json(rst);
        }

        [HttpPost]
        [Route("GetList")]
        public IActionResult GetList(Client model)
        {
            var bll = new ClientBll();
            var rst = bll.GetList(model);
            return Json(rst);
        }
    }
}