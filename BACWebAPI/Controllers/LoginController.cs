using System.Net;
using System.Net.Http;
using System.Web.Http;
using BACWebAPI.Models.Data;
using BACWebAPI.Models.JWT;
using BusinessClassLibrary.EntityClass;
using BusinessClassLibrary.LogicClass.Common;

namespace BACWebAPI.Controllers
{
    public class LoginController : ApiController
    {
        private clsCommonFunction objCommonFunction = new clsCommonFunction();
        private MobileAPIResponse<LoginResponse> objResponse = new MobileAPIResponse<LoginResponse>();
        private readonly UsersLogic usersLogic = new UsersLogic();


        // GET: api/Login/5
        [AllowAnonymous]
        public string Get()
        {
            if (true) return JWTToken.GenerateToken("webuser");
        }

        [AcceptVerbs(clsEnum.RequestMethod.Post)]
        [ActionName(MethodConst.GenerateToken)]
        [AllowAnonymous]
        public HttpResponseMessage ValidateAndGenerateToken([FromBody] LoginRequest apiRequest)
        {
            usersLogic.validateUser(apiRequest, ref objResponse, ModelState);
            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
        }
    }
}