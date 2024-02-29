using System.Net;
using System.Net.Http;
using System.Web.Http;
using BACWebAPI.Models.Data;
using BACWebAPI.Models.JWT;
using BusinessClassLibrary.EntityClass;
using BusinessClassLibrary.LogicClass.Common;

namespace BACWebAPI.Controllers
{
    public class WeatherController : ApiController
    {
        private CommonResponse<WeatherResponse> objResponse = new CommonResponse<WeatherResponse>();
        private readonly WeatherLogic objWeatherLogic = new WeatherLogic();

        /// <summary>
        ///     WEATHER DETAILS
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs(clsEnum.RequestMethod.Post)]
        [ActionName(MethodConst.GetWeatherDetails)]
        [JWTAuthenticationFilter]
        public HttpResponseMessage Get(WeatherRequest request)
        {
            objWeatherLogic.GetWeatherDetails(request, ref objResponse, ModelState);
            return Request.CreateResponse<dynamic>(HttpStatusCode.OK, objResponse);
        }
    }
}