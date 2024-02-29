using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Http.ModelBinding;
using BACWebAPI.Models.Service;
using BusinessClassLibrary.EntityClass;
using BusinessClassLibrary.LogicClass.Common;
using Newtonsoft.Json;

namespace BACWebAPI.Models.Data
{
    public class WeatherLogic
    {
        private readonly APIRequestLOG objAPIRequestLOG = new APIRequestLOG();
        private readonly clsAPILOG objclsAPILOG = new clsAPILOG();
        private readonly clsCommonFunction objCommonFunction = new clsCommonFunction();
        private readonly clsWeatherDTO objWeatherDTO = new clsWeatherDTO();

        public void GetWeatherDetails(WeatherRequest request, ref CommonResponse<WeatherResponse> objResponse,
            ModelStateDictionary modalState)
        {
            var objError = new List<clsMessage>();
            var Response = new WeatherResponse();
            if (modalState.IsValid)
            {
                try
                {
                    Response = BindWeather(request);
                    if (Response != null && Response.currentCondition != null)
                    {
                        var strweathericon = string.Empty;
                        objCommonFunction.GetWeatherStatusClasses(Response.currentCondition.weatherIcon,
                            out strweathericon);
                        Response.currentCondition.weatherImageIcon = strweathericon;
                    }

                    objResponse = new CommonResponse<WeatherResponse>
                        {success = true, message = "Successfully.", data = Response};
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Trace.Warn("Get :: " + ex.InnerException);
                    objError.Add(new clsMessage {code = 500, message = "Internal Server Error."});
                    objResponse = new CommonResponse<WeatherResponse> {success = false, error = objError};
                }
            }
            else
            {
                Common.clsCommonFunction.SetModelError(modalState, ref objError);
                objResponse = new CommonResponse<WeatherResponse> {success = false, error = objError};
            }

            //REQUEST RESPONSE LOGS
            objclsAPILOG.InsertAPILOG(clsAPILOG.GenerateLogContent(request, "GetWeatherDetails"),
                clsAPILOG.GenerateLogContent(objResponse, "GetWeatherDetails"));
        }

        public WeatherResponse CallWeatherAPI(WeatherRequest request, ref string strWeatherResponse)
        {
            var strCurrentTime = "";
            DateTime dtCurrentDate;
            var Response = new WeatherResponse();

            try
            {
                var url = KeyValues.WeatherServiceURL.Replace("[#CITY#]",
                              request != null && string.IsNullOrEmpty(request.city) ? KeyValues.WeatherCity : "") +
                          KeyValues.FlightAPIKey + (request != null && request.isArabic ? "&isArabic=true" : "");
                strWeatherResponse = objCommonFunction.SendRequest(url, "", clsEnum.RequestMethod.Get, true);

                Response = JsonConvert.DeserializeObject<WeatherResponse>(strWeatherResponse);

                dtCurrentDate = objCommonFunction.GetCurrentBahrainTime(out strCurrentTime, out dtCurrentDate);

                objAPIRequestLOG.Data = "";
                objAPIRequestLOG.URL = url;
                objAPIRequestLOG.RequestedDate = dtCurrentDate;
                var strRequest = JsonConvert.SerializeObject(objAPIRequestLOG);

                objWeatherDTO.InsertWeatherThirdPartyRequestResponse(strRequest, strWeatherResponse,
                    CacheKey.Weather + "_" + (request.isArabic ? 1 : 0));
            }
            catch (Exception ex)
            {
                HttpContext.Current.Trace.Warn(" Exception :CallWeatherAPI :: : " + ex.Message);
            }

            return Response;
        }

        public WeatherResponse BindWeather(WeatherRequest request)
        {
            var strWeatherCache = "";
            var Response = new WeatherResponse();

            try
            {
                HttpContext.Current.Trace.Warn("Weather Culture ::" + request.isArabic);
                strWeatherCache =
                    (string) Common.clsCommonFunction.GetCacheValue(CacheKey.Weather + "_" +
                                                                    (request.isArabic ? 1 : 0));
            }
            catch (Exception ex)
            {
                strWeatherCache = "";
            }

            if (!string.IsNullOrEmpty(strWeatherCache))
            {
                Response = JsonConvert.DeserializeObject<WeatherResponse>(strWeatherCache);
            }
            else
            {
                var objResponseDB = objWeatherDTO.GetWeatherResponse(KeyValues.WeatherValidCacheAgeMinute,
                    CacheKey.Weather + "_" + (request.isArabic ? 1 : 0));

                if (objResponseDB != null && objResponseDB.Count > 0)
                {
                    var intValidTime = 0;
                    foreach (var item in objResponseDB)
                    {
                        strWeatherCache = Convert.ToString(item.data);
                        intValidTime = Convert.ToInt32(item.mintime);
                    }

                    Response = JsonConvert.DeserializeObject<WeatherResponse>(strWeatherCache);
                    Common.clsCommonFunction.AddInCacheMemory(CacheKey.Weather + "_" + (request.isArabic ? 1 : 0),
                        strWeatherCache, intValidTime);
                }

                if (Response == null || Response.currentCondition == null || string.IsNullOrEmpty(strWeatherCache))
                {
                    Response = CallWeatherAPI(request, ref strWeatherCache);
                    Common.clsCommonFunction.AddInCacheMemory(CacheKey.Weather + "_" + (request.isArabic ? 1 : 0),
                        strWeatherCache, KeyValues.WeatherValidCacheAgeMinute);
                }
            }

            return Response;
        }
    }
}