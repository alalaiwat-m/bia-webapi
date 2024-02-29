using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.ModelBinding;
using BACWebAPI.Models.Service;
using BusinessClassLibrary.EntityClass;
using BusinessClassLibrary.LogicClass.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BACWebAPI.Models.Data
{
    public class InstagramLogic
    {
        private readonly APIRequestLOG objAPIRequestLOG = new APIRequestLOG();
        private readonly clsAPILOG objclsAPILOG = new clsAPILOG();
        private readonly clsCommonFunction objCommonFunction = new clsCommonFunction();
        private readonly clsEmailService objEmailService = new clsEmailService();
        private readonly clsInstagramDTO objInstagramDTO = new clsInstagramDTO();

        public void GetInstagramFeed(InstagramRequest instagramRequest,
            ref CommonResponse<InstagramFeedResponse> objResponse, ModelStateDictionary modalState)
        {
            var objError = new List<clsMessage>();
            var Instagram = new instaFeedData();
            if (modalState.IsValid)
            {
                try
                {
                    Instagram = BindInstagramFeed(instagramRequest);

                    var ResInsta = new InstagramFeedResponse();
                    var lstData = new List<Instadata>();
                    if (Instagram?.data != null)
                        foreach (var Data in Instagram.data.Where(x => x.media_type != "VIDEO"))
                            lstData.Add(new Instadata
                            {
                                caption = Data.caption,
                                images = Data.media_url
                            });

                    ResInsta.data = lstData.ToList();

                    if (instagramRequest.count > 0) ResInsta.data = ResInsta.data.Take(instagramRequest.count).ToList();

                    //ResInsta.user = (Instagram?.data?.Count > 0 ? Instagram?.data[0].user : null);

                    objResponse = new CommonResponse<InstagramFeedResponse>
                        {success = true, message = "Successfully.", data = ResInsta, count = ResInsta.data.Count};
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Trace.Warn("Get :: " + ex.Message);
                    objError.Add(new clsMessage {code = 500, message = "Internal Server Error."});
                    objResponse = new CommonResponse<InstagramFeedResponse> {success = false, error = objError};
                }
            }
            else
            {
                Common.clsCommonFunction.SetModelError(modalState, ref objError);
                objResponse = new CommonResponse<InstagramFeedResponse> {success = false, error = objError};
            }

            //REQUEST RESPONSE LOGS
            objclsAPILOG.InsertAPILOG(clsAPILOG.GenerateLogContent(instagramRequest, "GetInstagramFeed"),
                clsAPILOG.GenerateLogContent(objResponse, "GetInstagramFeed"));
        }


        public instaFeedData CallInstagramAPI(InstagramRequest request, ref string strInstagramResponse)
        {
            var strCurrentTime = "";
            DateTime dtCurrentDate;
            var Instagram = new instaFeedData();
            try
            {
                var ENGsettings = objEmailService.GetSystemSettings();
                var systemSettingDate = ENGsettings.Where(w => w.key == "IFTKN" && w.isarabic == false).Select(a =>
                    new systemSetting
                    {
                        value = a.value,
                        createdDate = a.createdDate
                    }).FirstOrDefault();

                var result = DateTime.Compare(systemSettingDate.createdDate.AddMonths(1), DateTime.Now);

                var url = "https://graph.instagram.com/me/media?fields=id,caption,media_type,media_url&access_token=" +
                          systemSettingDate.value;
                strInstagramResponse =
                    objCommonFunction.SendRequest(url, "", clsEnum.RequestMethod.Get, true, false, true);

                if (result < 0 || strInstagramResponse == "BadRequest") //strInstagramResponse == "BadRequest"
                {
                    url = "https://graph.instagram.com/refresh_access_token?grant_type=ig_refresh_token&access_token=" +
                          systemSettingDate.value;
                    var strRefreshResponse = objCommonFunction.SendRequest(url, "", clsEnum.RequestMethod.Get, true);

                    var instaRefreshFeed = new instaRefreshFeed();
                    instaRefreshFeed = JsonConvert.DeserializeObject<instaRefreshFeed>(strRefreshResponse);

                    url = "https://graph.instagram.com/me/media?fields=id,caption,media_type,media_url&access_token=" +
                          instaRefreshFeed.access_token;
                    strInstagramResponse =
                        objCommonFunction.SendRequest(url, "", clsEnum.RequestMethod.Get, true, false, true);

                    objEmailService.UpdateSystemSettings(instaRefreshFeed.access_token, "IFTKN");
                }

                Instagram = JsonConvert.DeserializeObject<instaFeedData>(strInstagramResponse);
                dtCurrentDate = objCommonFunction.GetCurrentBahrainTime(out strCurrentTime, out dtCurrentDate);

                objAPIRequestLOG.Data = "";
                objAPIRequestLOG.URL = KeyValues.InstagramServiceURL + KeyValues.InstagramAPIKey;
                objAPIRequestLOG.RequestedDate = dtCurrentDate;
                var strRequest = JsonConvert.SerializeObject(objAPIRequestLOG);
                objInstagramDTO.InsertInstagramThirdPartyRequestResponse(strRequest, strInstagramResponse,
                    request.Culture);
            }
            catch (Exception ex)
            {
                HttpContext.Current.Trace.Warn(" Exception :CallInstagramAPI :: : " + ex.Message);
            }

            return Instagram;
        }

        public instaFeedData BindInstagramFeed(InstagramRequest request)
        {
            var strInstagramFeedCache = "";
            var Instagram = new instaFeedData();

            try
            {
                strInstagramFeedCache =
                    (string) Common.clsCommonFunction.GetCacheValue(CacheKey.InstagramFeed + "_" + request.Culture);
            }
            catch (Exception ex)
            {
                strInstagramFeedCache = "";
            }

            if (!string.IsNullOrEmpty(strInstagramFeedCache))
            {
                var objParseArrivalResult = JObject.Parse(Convert.ToString(strInstagramFeedCache));
                Instagram = JsonConvert.DeserializeObject<instaFeedData>(Convert.ToString(objParseArrivalResult));
            }
            else
            {
                //List<DBCache> objResponseDB = objInstagramDTO.GetInstagramResponse(KeyValues.InstagramValidCacheAgeMinute, CacheKey.InstagramFeed + "_" + request.Culture);
                var objResponseDB =
                    objInstagramDTO.GetInstagramResponse(KeyValues.InstagramValidCacheAgeMinute, request.Culture);

                if (objResponseDB != null && objResponseDB.Count > 0)
                {
                    var intValidTime = 0;
                    foreach (var item in objResponseDB)
                    {
                        strInstagramFeedCache = Convert.ToString(item.data);
                        intValidTime = Convert.ToInt32(item.mintime);
                    }

                    try
                    {
                        Instagram = JsonConvert.DeserializeObject<instaFeedData>(strInstagramFeedCache);
                    }
                    catch (Exception ex)
                    {
                        Instagram = null;
                    }

                    Common.clsCommonFunction.AddInCacheMemory(CacheKey.InstagramFeed + "_" + request.Culture,
                        strInstagramFeedCache, intValidTime);
                }

                if (Instagram == null || Instagram.data == null)
                {
                    Instagram = CallInstagramAPI(request, ref strInstagramFeedCache);
                    Common.clsCommonFunction.AddInCacheMemory(CacheKey.InstagramFeed + "_" + request.Culture,
                        strInstagramFeedCache, KeyValues.InstagramValidCacheAgeMinute);
                }
            }


            return Instagram;
        }
    }
}