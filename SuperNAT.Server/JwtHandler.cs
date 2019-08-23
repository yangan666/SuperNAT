using SuperNAT.Common.Models;
using SuperNAT.Server.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SuperNAT.Server
{
    public class JwtHandler
    {
        public static string GetToken(JwtSetting jwtSetting, User user)
        {
            //创建用户身份标识，可按需要添加更多信息
            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("id", user.id.ToString(), ClaimValueTypes.Integer32), // 用户id
                new Claim("name", user.user_name), // 用户名
                //new Claim("admin", user.IsAdmin.ToString(),ClaimValueTypes.Boolean) // 是否是管理员
            };

            //创建令牌
            var token = new JwtSecurityToken(
                    issuer: jwtSetting.Issuer,
                    audience: jwtSetting.Audience,
                    signingCredentials: jwtSetting.Credentials,
                    claims: claims,
                    notBefore: DateTime.Now,
                    expires: DateTime.Now.AddSeconds(jwtSetting.ExpireSeconds)
                );

            string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return jwtToken;
        }
    }
}
