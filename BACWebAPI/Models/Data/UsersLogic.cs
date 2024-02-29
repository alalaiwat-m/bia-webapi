using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.ModelBinding;
using BACWebAPI.Models.Common;
using BACWebAPI.Models.JWT;
using BACWebAPI.Models.Service;
using BusinessClassLibrary.EntityClass;

namespace BACWebAPI.Models.Data
{
    public class UsersLogic
    {
        private clsCommonFunction objCommonFunction = new clsCommonFunction();
        private readonly clsUserService objUserService = new clsUserService();

        public void validateUser(LoginRequest loginRequest, ref MobileAPIResponse<LoginResponse> objResponse,
            ModelStateDictionary modalState)
        {
            var objError = new List<clsMessage>();
            var blnSuccess = false;
            var strMessage = "";

            if (modalState.IsValid)
            {
                try
                {
                    var loginResponse = new LoginResponse();

                    // PASSWORD ENCRYPTION LOGIC HERE
                    var strPassword = loginRequest.password;

                    var usersDTO = objUserService.GetUserByUsernamePwd(loginRequest.userName, strPassword)
                        .FirstOrDefault();
                    if (usersDTO != null && usersDTO.userid > 0)
                    {
                        var strToken = JWTToken.GenerateToken(usersDTO.userName);
                        loginResponse.token = strToken;
                        loginResponse.isValid = true;
                        loginResponse.userName = usersDTO.userName;
                        strMessage = "Login Successfully";
                        blnSuccess = true;
                    }
                    else
                    {
                        strMessage = "Please enter valid details.";
                    }

                    objResponse = new MobileAPIResponse<LoginResponse>
                        {success = blnSuccess, message = strMessage, data = loginResponse};
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Trace.Warn("Get :: " + ex.Message);

                    objError.Add(new clsMessage {code = 500, message = "Internal Server Error."});
                    objResponse = new MobileAPIResponse<LoginResponse> {success = false, error = objError};
                }
            }
            else
            {
                clsCommonFunction.SetModelError(modalState, ref objError);
                objResponse = new MobileAPIResponse<LoginResponse> {success = false, error = objError};
            }
        }
    }
}