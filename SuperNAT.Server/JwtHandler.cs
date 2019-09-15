using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SuperNAT.Common.Models;
using SuperNAT.Server.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
                new Claim("user_id", user.user_id), // 用户id
                new Claim("user_name", user.user_name), // 用户名
                new Claim("is_admin", user.is_admin.ToString(), ClaimValueTypes.Boolean) // 是否是管理员
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

        /// <summary>
        /// 验证身份 验证签名的有效性,
        /// </summary>
        /// <param name="encodeJwt"></param>
        /// <param name="validatePayLoad">自定义各类验证； 是否包含那种申明，或者申明的值， </param>
        /// 例如：payLoad["aud"]?.ToString() == "roberAuddience";
        /// 例如：验证是否过期 等
        /// <returns></returns>
        public static bool Validate(JwtSetting jwtSetting, string encodeJwt, out int code, out string error, out Dictionary<string, object> payLoad)
        {
            try
            {
                var jwtArr = encodeJwt.Split('.');
                var header = JsonConvert.DeserializeObject<Dictionary<string, object>>(Base64UrlEncoder.Decode(jwtArr[0]));
                payLoad = JsonConvert.DeserializeObject<Dictionary<string, object>>(Base64UrlEncoder.Decode(jwtArr[1]));
                var hs256 = new HMACSHA256(Encoding.ASCII.GetBytes(jwtSetting.SecurityKey));

                //首先验证签名是否正确（必须的）
                if (!string.Equals(jwtArr[2], Base64UrlEncoder.Encode(hs256.ComputeHash(Encoding.UTF8.GetBytes(string.Concat(jwtArr[0], ".", jwtArr[1]))))))
                {
                    code = 10001;
                    error = "签名不正确";
                    return false;
                }

                //其次验证是否在有效期内（也应该必须）
                var now = ToUnixEpochDate(DateTime.Now);
                if (!(now >= long.Parse(payLoad["nbf"].ToString()) && now < long.Parse(payLoad["exp"].ToString())))
                {
                    code = 10002;
                    error = "会话超时";
                    return false;
                }

                code = 200;
                error = "";
                return true;
            }
            catch (Exception ex)
            {
                Log4netUtil.Error($"授权验证失败：{ex}");
                code = 10003;
                error = "授权验证失败";
                payLoad = null;
                return false;
            }
        }

        /// <summary>
        /// 获取jwt中的payLoad
        /// </summary>
        /// <param name="encodeJwt"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetPayLoad(string encodeJwt)
        {
            var jwtArr = encodeJwt.Split('.');
            var payLoad = JsonConvert.DeserializeObject<Dictionary<string, object>>(Base64UrlEncoder.Decode(jwtArr[1]));
            return payLoad;
        }

        public static long ToUnixEpochDate(DateTime date) =>
         (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
    }
}
