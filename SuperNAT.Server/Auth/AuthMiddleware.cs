using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using SuperNAT.Model;
using SuperNAT.Common;
using SuperNAT.Server.Models;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using SuperNAT.Bll;

namespace SuperNAT.Server.Auth
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JwtSetting _jwtSetting;
        private readonly List<string> _allowAnonymousPathList = new List<string>() { "/Api/User/Login", "/Api/User/Register", "/Api/Map/GetMapList", "/Api/Common/GetSessions" };

        public AuthMiddleware(RequestDelegate next, IOptions<JwtSetting> option)
        {
            _next = next;
            _jwtSetting = option.Value;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var rst = new ApiResult<bool>();

            try
            {
                if (!httpContext.Request.Path.ToString().StartsWith("/Api"))
                {
                    await _next.Invoke(httpContext);
                    return;
                }
                if (_allowAnonymousPathList.Contains(httpContext.Request.Path))
                {
                    await _next.Invoke(httpContext);
                    return;
                }

                //var result = await httpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);

                //if (!result.Succeeded)
                //{
                //    httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                //    await httpContext.Response.WriteAsync("Unauthorized");
                //}
                //else
                //{
                //    httpContext.User = result.Principal;
                //    await _next.Invoke(httpContext);
                //}

                #region 身份验证，并设置用户Ruser值
                var result = httpContext.Request.Headers.TryGetValue("Authorization", out StringValues authStr);
                if (!result || string.IsNullOrEmpty(authStr.ToString()) || authStr.ToString() == "Bearer undefined")
                {
                    //授权不存在
                    rst.Status = 10000;
                    throw new Exception("未登录");
                }
                result = JwtHandler.Validate(_jwtSetting, authStr.ToString().Substring("Bearer ".Length).Trim(), out int code, out string error, out Dictionary<string, object> payLoad);
                if (!result)
                {
                    rst.Status = code;
                    throw new Exception(error);
                }

                #endregion

                #region 权限验证
                //if (!userContext.Authorize(context.Request.Path))
                //{
                //    throw new UnauthorizedAccessException("未授权");
                //}
                #endregion

                //前端F5刷新 重新获取用户信息
                if (httpContext.Request.Path.ToString() == "/Api/User/GetUserInfo")
                {
                    var bll = new UserBll();
                    var user = bll.GetUserInfo(payLoad["user_id"].ToString());
                    if (user.Result)
                    {
                        //密码不传前端
                        user.Data.password = "";
                    }
                    await httpContext.Response.WriteAsync(JsonHelper.Instance.Serialize(user));
                }
                else
                {
                    //验证通过
                    await _next.Invoke(httpContext);
                }
            }
            catch (Exception ex)
            {
                rst.Message = ex.Message;
                await httpContext.Response.WriteAsync(JsonHelper.Instance.Serialize(rst));
            }
        }
    }
}
