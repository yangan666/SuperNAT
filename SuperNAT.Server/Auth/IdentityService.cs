using Microsoft.AspNetCore.Http;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace SuperNAT.Server.Auth
{
    public interface IIdentityService
    {
        string GetUserId();

        string GetUserName();
    }

    public class IdentityService : IIdentityService
    {
        private readonly IHttpContextAccessor _context;

        public IdentityService(IHttpContextAccessor context)
        {
            _context = context;
        }
        public string GetUserId()
        {
            return _context.HttpContext.User.FindFirst("user_id")?.Value;
        }

        public string GetUserName()
        {
            return _context.HttpContext.User.FindFirst("user_name")?.Value;
        }
    }
}
