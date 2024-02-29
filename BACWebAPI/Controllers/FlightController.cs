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
    public class FlightController : ApiController
    {
        private clsCommonFunction objCommonFunction = new clsCommonFunction();
        private readonly flightLogic objflight = new flightLogic();
        private CommonResponse<List<FlightRadar>> objFlightLocation = new CommonResponse<List<FlightRadar>>();
        private CommonResponse<List<FlightDetail>> objResponse = new CommonResponse<List<FlightDetail>>();

        [AcceptVerbs(clsEnum.RequestMethod.Post)]
        [ActionName(MethodConst.GetArrivalFlights)]
        public HttpResponseMessage GetArrivalFlight([FromBody] FlightRequest apiRequest)
        {
            objflight.GetArrivalFligths(apiRequest, ref objResponse, ModelState);
            return Request.CreateResponse<dynamic>(HttpStatusCode.OK, objResponse);
        }

        [AcceptVerbs(clsEnum.RequestMethod.Post)]
        [ActionName(MethodConst.GetDepartureFlights)]
        public HttpResponseMessage GetDepartFlight([FromBody] FlightRequest apiRequest)
        {
            objflight.GetDepartureFligths(apiRequest, ref objResponse, ModelState);
            return Request.CreateResponse<dynamic>(HttpStatusCode.OK, objResponse);
        }

        [AcceptVerbs(clsEnum.RequestMethod.Post, clsEnum.RequestMethod.Get)]
        [ActionName(MethodConst.GETFLIGHTLOCATION)]
        [AllowAnonymous]
        public HttpResponseMessage GetFlightRadar()
        {
            objflight.GetFligthsLocations(ref objFlightLocation, ModelState);
            return Request.CreateResponse<dynamic>(HttpStatusCode.OK, objFlightLocation);
        }
    }
}