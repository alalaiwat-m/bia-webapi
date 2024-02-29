using System;
using System.Web;
using BACWebAPI.Models.Common;
using Dapper;
using Newtonsoft.Json;

namespace BACWebAPI.Models.Service
{
    public class clsAPILOG : Connection
    {
        private DynamicParameters builder = new DynamicParameters();
        private string strQuery = "";

        public void InsertAPILOG(string strRequest, string strResponse)
        {
            builder = new DynamicParameters();
            strQuery = @" INSERT INTO apirequestresponselog(request,response,source,createdon)
                                        VALUES (@request,@response,@source,GETUTCDATE()) ";

            ParameterBuilder.AddParameter(builder, "request", strRequest);
            ParameterBuilder.AddParameter(builder, "response", strResponse);
            ParameterBuilder.AddParameter(builder, "source", getRequestDetails());

            ExecuteNonQuery(strQuery, builder);
        }

        public static string GenerateLogContent(object objObject, string strName)
        {
            var strObject = "";
            var strContent = "";
            try
            {
                strObject = JsonConvert.SerializeObject(objObject);

                strContent = "{\"" + strName + "\" : ["
                             + "{\"DATETIME\" : \"" + DateTime.Now + "\"},"
                             + "{\"URL\" : \"" + HttpContext.Current.Request.Url + "\"},"
                             + "{\"PARAMETERS\" : [" + strObject + "]},"
                             + "{\"METHOD\" : \"POST\"}"
                             + "]}";
            }
            catch (Exception ex)
            {
                //
            }

            return strContent;
        }

        public string getRequestDetails()
        {
            var HTTP_HOST = HttpContext.Current.Request.ServerVariables["HTTP_HOST"];
            var HTTP_USER_AGENT = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"];
            var REMOTE_ADDR = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

            var Request = "";
            Request = "{\"HTTP_HOST\" : \"" + HttpContext.Current.Request.ServerVariables["HTTP_HOST"] + "\"";
            Request += ",\"HTTP_USER_AGENT\" : \"" + HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"] +
                       "\"";
            Request += ",\"REMOTE_ADDR\" : \"" + HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"] + "\"}";

            return Request;
        }
    }
}