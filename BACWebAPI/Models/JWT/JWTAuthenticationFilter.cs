using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace BACWebAPI.Models.JWT
{
    public class JWTAuthenticationFilter : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(HttpActionContext ctx)
        {
            if (!ctx.RequestContext.Principal.Identity.IsAuthenticated)
                base.HandleUnauthorizedRequest(ctx);
            else
                // Authenticated, but not AUTHORIZED.  Return 403 instead!
                ctx.Response = ctx.Request.CreateErrorResponse(HttpStatusCode.Forbidden,
                    "You are not authorized to use this method.");
        }
    }
}