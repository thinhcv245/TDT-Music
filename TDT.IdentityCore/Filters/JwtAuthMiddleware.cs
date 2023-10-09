﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDT.IdentityCore.Utils;

namespace TDT.IdentityCore.Filters
{
    public class JwtAuthMiddleware
    {
        private readonly RequestDelegate _next;
        public JwtAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IConfiguration cfg)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = SecurityHelper.ValidateToken(cfg, token);
            if (userId != null)
            {
                // attach user to context on successful jwt validation
                //context.Items["User"] = userService.GetById(userId.Value);
            }

            await _next(context);
        }
    }
}
