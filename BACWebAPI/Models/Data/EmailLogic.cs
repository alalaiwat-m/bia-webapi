using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.ModelBinding;
using BACWebAPI.Models.Common;
using BACWebAPI.Models.Service;
using BusinessClassLibrary.EntityClass;

namespace BACWebAPI.Models.Data
{
    public class EmailLogic
    {
        private clsCommonFunction objCommonFunction = new clsCommonFunction();
        private readonly clsEmailService objEmailService = new clsEmailService();

        public void AddEmail(EmailRequest emailRequest, ref CommonResponse<EmailResponse> objResponse,
            ModelStateDictionary modalState)
        {
            var objError = new List<clsMessage>();
            var blnSuccess = false;
            var strMessage = "";

            if (modalState.IsValid)
            {
                try
                {
                    HttpContext.Current.Trace.Warn("Method Value :::" + emailRequest.emailContent);
                    objEmailService.UpsertEmail(emailRequest.code, emailRequest.emailContent, emailRequest.subject,
                        emailRequest.name, emailRequest.ccEmail, emailRequest.bccEmail, emailRequest.isArabic);
                    strMessage = "Email updated successfully";
                    blnSuccess = true;
                    objResponse = new CommonResponse<EmailResponse>
                        {success = blnSuccess, message = strMessage, data = null};
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Trace.Warn("Get :: " + ex.Message);

                    objError.Add(new clsMessage {code = 500, message = "Internal Server Error."});
                    objResponse = new CommonResponse<EmailResponse> {success = false, error = objError};
                }
            }
            else
            {
                clsCommonFunction.SetModelError(modalState, ref objError);
                objResponse = new CommonResponse<EmailResponse> {success = false, error = objError};
            }
        }

        public void GetEmailByCode(EmailRequest emailRequest, ref CommonResponse<EmailDetails> objResponse,
            ModelStateDictionary modalState)
        {
            var objError = new List<clsMessage>();
            var blnSuccess = false;
            var strMessage = "";

            if (modalState.IsValid)
            {
                try
                {
                    var emailDetails = objEmailService.GetEmailTemplateByCode(emailRequest.code, emailRequest.isArabic);
                    strMessage = "Email get successfully";
                    blnSuccess = true;
                    objResponse = new CommonResponse<EmailDetails>
                        {success = blnSuccess, message = strMessage, data = emailDetails.FirstOrDefault()};
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Trace.Warn("Get :: " + ex.Message);

                    objError.Add(new clsMessage {code = 500, message = "Internal Server Error."});
                    objResponse = new CommonResponse<EmailDetails> {success = false, error = objError};
                }
            }
            else
            {
                clsCommonFunction.SetModelError(modalState, ref objError);
                objResponse = new CommonResponse<EmailDetails> {success = false, error = objError};
            }
        }
    }
}