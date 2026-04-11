using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Phoenix.Hemisphere.Server.Middlewares
{
    public class SecureAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context != null)
            {
                // Auth logic
                var key = context.HttpContext.Request.Headers.FirstOrDefault(d => d.Key == "X-SeeDu-Key");
                var signature = context.HttpContext.Request.Headers.FirstOrDefault(d => d.Key == "X-SeeDu-Signature");
                if (string.IsNullOrEmpty(key.Value) || string.IsNullOrEmpty(signature.Value))
                {
                    context.Result = new UnauthorizedResult();
                }
            }
        }
    }
}
