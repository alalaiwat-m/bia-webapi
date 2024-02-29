using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using BusinessClassLibrary.LogicClass.Common;
using Microsoft.IdentityModel.Tokens;

namespace BACWebAPI.Models.Common
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var strauthToken = "";
            IEnumerable<string> authToken;
            if (actionContext.Request.Headers.TryGetValues(StaticContent.authToken, out authToken))
                strauthToken = authToken.FirstOrDefault();

            if (!string.IsNullOrEmpty(strauthToken))
            {
                var dtCurrentTime = DateTime.Now;
                //strauthToken = EncryptDecrypt.Decrypt(strauthToken);
                var dt = new DateTime(Convert.ToInt64(strauthToken));

                //HEADER VALIDATE
                if (false && dtCurrentTime.AddMinutes(-KeyValues.ValidAuthTime) > dt &&
                    dt > dtCurrentTime.AddMinutes(KeyValues.ValidAuthTime))
                    actionContext.Response = actionContext.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest, StaticContent.InvalidRequestMsg);

                //MODAL VALIDATE
                if (actionContext.ModelState.IsValid == false)
                    actionContext.Response = actionContext.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest, actionContext.ModelState);
            }
            else
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest, StaticContent.InvalidHeaderMsg);
            }
        }
    }

    public class demo
    {
        private const string Secret =
            "db3OIsj+BXE9NZDy0t8W3TcNekrF+2d/1sFnWG4HnV8TZY30iTOdtVWJG8abWvB1GlOgJuQZdcF2Luqm/hccMw==";

        public static string GenerateToken(string username, int expireMinutes = 20)
        {
            var symmetricKey = Convert.FromBase64String(Secret);
            var tokenHandler = new JwtSecurityTokenHandler();

            var now = DateTime.UtcNow;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username)
                }),

                Expires = now.AddMinutes(Convert.ToInt32(expireMinutes)),

                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(symmetricKey),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var stoken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(stoken);

            return token;
        }
    }
}