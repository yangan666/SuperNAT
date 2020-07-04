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
    public class UserController : BaseController
    {
        private readonly JwtSetting _jwtSetting;
        public UserController(IOptions<JwtSetting> option)
        {
            _jwtSetting = option.Value;
        }

        [HttpPost]
        [Route("Login")]
        public IActionResult Login(User model)
        {
            var bll = new UserBll();
            var rst = bll.Login(model);
            if (rst.Result)
            {
                //构造jwt token
                rst.Data.token = JwtHandler.GetToken(_jwtSetting, rst.Data);
            }
            return Json(rst);
        }

        [HttpPost]
        [Route("GetUserInfo")]
        public IActionResult GetUserInfo()
        {
            //AuthMiddleware处理了
            return Json(new ApiResult<User>());
        }

        [HttpPost]
        [Route("Add")]
        public IActionResult Add(User model)
        {
            var rst = new ApiResult<bool>();

            var bll = new UserBll();
            if (model.id == 0)
            {
                model.user_id = EncryptHelper.CreateGuid();
                if (string.IsNullOrEmpty(model.password))
                {
                    model.password = "123456";
                }
                model.password = EncryptHelper.MD5Encrypt(model.password);
                rst = bll.Add(model);
            }
            else
            {
                if (!string.IsNullOrEmpty(model.password))
                {
                    model.password = EncryptHelper.MD5Encrypt(model.password);
                }
                rst = bll.UpdateUser(model);
            }

            return Json(rst);
        }

        [HttpPost]
        [Route("Register")]
        public IActionResult Register(User model)
        {
            var rst = new ApiResult<bool>();

            var bll = new UserBll();
            model.user_id = EncryptHelper.CreateGuid();
            model.role_id = GlobalConfig.RegRoleId;
            if (string.IsNullOrEmpty(model.password))
            {
                model.password = "123456";
            }
            model.password = EncryptHelper.MD5Encrypt(model.password);
            rst = bll.Add(model);

            return Json(rst);
        }

        [HttpPost]
        [Route("Delete")]
        public IActionResult Delete(User model)
        {
            var bll = new UserBll();
            var rst = bll.Delete(model);

            return Json(rst);
        }

        [HttpPost]
        [Route("Disable")]
        public IActionResult Disable(User model)
        {
            var bll = new UserBll();
            model.is_disabled = !model.is_disabled;
            var rst = bll.DisableUser(model);
            var text = model.is_disabled ? "禁用" : "启用";
            rst.Message = rst.Result ? $"{text}成功" : $"{text}失败";

            return Json(rst);
        }

        [HttpPost]
        [Route("GetOne")]
        public IActionResult GetOne(User model)
        {
            if (model.id == 0)
            {
                var defalut = new ApiResult<User>()
                {
                    Result = true,
                    Data = new User()
                };
                return Json(defalut);
            }
            var bll = new UserBll();
            var rst = bll.GetOne(model);
            if (rst.Result)
            {
                //密码不传前端
                rst.Data.password = "";
            }
            return Json(rst);
        }

        [HttpPost]
        [Route("GetList")]
        public IActionResult GetList(User model)
        {
            var bll = new UserBll();
            var rst = bll.GetList(model);
            return Json(rst);
        }
    }
}
