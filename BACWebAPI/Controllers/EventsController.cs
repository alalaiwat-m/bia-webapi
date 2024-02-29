using System.Collections.Generic;
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
    public class EventsController : ApiController
    {
        private readonly EventsLogic objEventsLogic = new EventsLogic();
        private CommonResponse<List<EventDetail>> objResponse = new CommonResponse<List<EventDetail>>();

        [AcceptVerbs(clsEnum.RequestMethod.Post)]
        [ActionName(MethodConst.GetEvents)]
        public HttpResponseMessage Get(EventsRequest request)
        {
            objEventsLogic.GetEventsLogicDetails(request, ref objResponse, ModelState);
            return Request.CreateResponse<dynamic>(HttpStatusCode.OK, objResponse);
        }
    }
}