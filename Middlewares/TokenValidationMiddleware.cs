using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace YazlabBirSonProje.Middlewares
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Token kontrolü
            if (context.Session.GetString("Token") != null)
            {
                var tokenExpiration = context.Session.GetString("TokenExpiration");

                if (DateTime.TryParse(tokenExpiration, out DateTime expiration))
                {
                    if (DateTime.Now > expiration)
                    {
                        context.Session.Clear();
                        context.Response.Redirect("/Customer/Login");
                        return;
                    }
                    else
                    {
                        context.Session.SetString("TokenExpiration", DateTime.Now.AddMinutes(5).ToString());
                    }
                }
            }

            await _next(context);
        }
    }
}
