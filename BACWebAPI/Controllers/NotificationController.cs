using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using BACWebAPI.Models.Data;
using BusinessClassLibrary.EntityClass;
using BusinessClassLibrary.LogicClass.Common;

namespace BACWebAPI.Controllers
{
    public class NotificationController : ApiController
    {
        private readonly NotificationLogic objNotificationLogic = new NotificationLogic();
        private CommonResponse<Alert> objResponse = new CommonResponse<Alert>();

        [AcceptVerbs(clsEnum.RequestMethod.Post)]
        [ActionName(MethodConst.AddNotification)]
        public HttpResponseMessage Create(AlertRequest request)
        {
            objNotificationLogic.AddNotification(request, ref objResponse, ModelState);
            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
        }

        [AcceptVerbs(clsEnum.RequestMethod.Get)]
        [ActionName(MethodConst.Notification)]
        [AllowAnonymous]
        public HttpResponseMessage get()
        {
            var objlstResponse = new CommonResponse<List<Alert>>();

            objNotificationLogic.getAllAlertsDetails(ref objlstResponse);
            HttpContext.Current.Trace.Warn("Response :: " + objlstResponse);

            return Request.CreateResponse(HttpStatusCode.OK, objlstResponse);
        }
    }
}