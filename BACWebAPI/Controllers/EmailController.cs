using System.Net;
using System.Net.Http;
using System.Web.Http;
using BACWebAPI.Models.Data;
using BusinessClassLibrary.EntityClass;
using BusinessClassLibrary.LogicClass.Common;

namespace BACWebAPI.Controllers
{
    public class EmailController : ApiController
    {
        private readonly EmailLogic objEmailLogic = new EmailLogic();
        private CommonResponse<EmailResponse> objResponse = new CommonResponse<EmailResponse>();

        [AcceptVerbs(clsEnum.RequestMethod.Post)]
        [ActionName(MethodConst.GetEmail)]
        public HttpResponseMessage GetEmailContent(EmailRequest request)
        {
            var objEmailDetailsResponse = new CommonResponse<EmailDetails>();

            objEmailLogic.GetEmailByCode(request, ref objEmailDetailsResponse, ModelState);
            return Request.CreateResponse<dynamic>(HttpStatusCode.OK, objEmailDetailsResponse);
        }

        // POST: api/Email
        [AcceptVerbs(clsEnum.RequestMethod.Post)]
        [ActionName(MethodConst.UpsertEmail)]
        public HttpResponseMessage Post(EmailRequest request)
        {
            objEmailLogic.AddEmail(request, ref objResponse, ModelState);
            return Request.CreateResponse<dynamic>(HttpStatusCode.OK, objResponse);
        }
    }
}