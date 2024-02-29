using System.Net;
using System.Net.Http;
using System.Web.Http;
using BACWebAPI.Models.Data;
using BACWebAPI.Models.JWT;
using BusinessClassLibrary.EntityClass;
using BusinessClassLibrary.LogicClass.Common;

namespace BACWebAPI.Controllers
{
    [JWTAuthenticationFilter]
    public class InstagramController : ApiController
    {
        private clsCommonFunction objCommon = new clsCommonFunction();
        private readonly InstagramLogic objInstgramLogic = new InstagramLogic();
        private CommonResponse<InstagramFeedResponse> objResponse = new CommonResponse<InstagramFeedResponse>();

        /// <summary>
        ///     INSTAGRAM FEED
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [AcceptVerbs(clsEnum.RequestMethod.Post)]
        [ActionName(MethodConst.GetInstagramFeed)]
        public HttpResponseMessage Get(InstagramRequest instagramRequest)
        {
            objInstgramLogic.GetInstagramFeed(instagramRequest, ref objResponse, ModelState);
            return Request.CreateResponse<dynamic>(HttpStatusCode.OK, objResponse);
        }
    }
}