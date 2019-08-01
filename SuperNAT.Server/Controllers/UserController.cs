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
    public class UserController : ControllerBase
    {
        [HttpPost]
        [Route("Add")]
        public IActionResult Add(User model)
        {
            using var bll = new UserBll();
            var rst = bll.Add(model);
            return new JsonResult(rst);
        }
    }
}
