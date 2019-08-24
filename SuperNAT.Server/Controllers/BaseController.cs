using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Server.Controllers
{
    //[Authorize]
    public class BaseController : ControllerBase
    {
        protected JsonResult Json(object content)
        {
            return new JsonResult(content, new JsonSerializerSettings() { DateFormatString = "yyyy-MM-dd HH:mm:ss" });
        }
    }
}
