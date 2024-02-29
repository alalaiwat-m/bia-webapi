using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.ModelBinding;
using BACWebAPI.Models.Service;
using BusinessClassLibrary.EntityClass;
using BusinessClassLibrary.LogicClass.Common;
using Newtonsoft.Json;

namespace BACWebAPI.Models.Data
{
    public class EventsLogic
    {
        private readonly APIRequestLOG objAPIRequestLOG = new APIRequestLOG();
        private readonly clsAPILOG objclsAPILOG = new clsAPILOG();
        private readonly clsCommonFunction objCommonFunction = new clsCommonFunction();
        private readonly clsEventsDTO objEventsDTO = new clsEventsDTO();

        public void GetEventsLogicDetails(EventsRequest request, ref CommonResponse<List<EventDetail>> objResponse,
            ModelStateDictionary modalState)
        {
            var objError = new List<clsMessage>();
            var Response = new List<EventDetail>();
            if (modalState.IsValid)
            {
                try
                {
                    Response = BindEventsLogic(request);
                    if (Response != null)
                        foreach (var Data in Response)
                            if (!string.IsNullOrEmpty(Data.listing_image))
                            {
                                var listinguri = new Uri(Data.listing_image);
                                if (!string.IsNullOrEmpty(listinguri.Query))
                                    Data.listing_image = HttpUtility.ParseQueryString(listinguri.Query).Get("src");
                            }

                    objResponse = new CommonResponse<List<EventDetail>>
                        {success = true, message = "Successfully.", data = Response};
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Trace.Warn("Get :: " + ex.Message);
                    objError.Add(new clsMessage {code = 500, message = "Internal Server Error."});
                    objResponse = new CommonResponse<List<EventDetail>> {success = false, error = objError};
                }
            }
            else
            {
                Common.clsCommonFunction.SetModelError(modalState, ref objError);
                objResponse = new CommonResponse<List<EventDetail>> {success = false, error = objError};
            }

            //REQUEST RESPONSE LOGS
            objclsAPILOG.InsertAPILOG(clsAPILOG.GenerateLogContent(request, "GetEventsLogicDetails"),
                clsAPILOG.GenerateLogContent(objResponse, "GetEventsLogicDetails"));
        }

        public List<EventDetail> CallEventsLogicAPI(EventsRequest request, ref string strEventsResponse)
        {
            var strCurrentTime = "";
            DateTime dtCurrentDate;
            var Response = new List<EventDetail>();


            try
            {
                var url = request.type == EventType.Listing
                    ? (request.languageid=="1"? KeyValues.EventsListingServiceURL : KeyValues.EventsListingServiceURL_AR)
                    : KeyValues.EventsServiceURL.Replace("[#EVENTID#]", request.recid)
                        .Replace("[#LANGUAGEID#]", request.languageid);
                strEventsResponse = objCommonFunction.SendRequest(url, "", clsEnum.RequestMethod.Get, true);
                Response = JsonConvert.DeserializeObject<List<EventDetail>>(strEventsResponse);

                dtCurrentDate = objCommonFunction.GetCurrentBahrainTime(out strCurrentTime, out dtCurrentDate);

                objAPIRequestLOG.Data = request.ToString();
                objAPIRequestLOG.URL = url;
                objAPIRequestLOG.RequestedDate = dtCurrentDate;
                var strRequest = JsonConvert.SerializeObject(objAPIRequestLOG);
                var strKey = request.type == EventType.Listing
                    ? CacheKey.Events + "_" + request.languageid
                    : CacheKey.EventsDetail + "_" + request.recid + "_" + request.languageid;
                objEventsDTO.InsertEventsThirdPartyRequestResponse(strRequest, strEventsResponse, request.type, strKey);
            }
            catch (Exception ex)
            {
                HttpContext.Current.Trace.Warn(" Exception :CallEventsLogicAPI :: : " + ex.Message);
            }

            return Response;
        }

        public List<EventDetail> BindEventsLogic(EventsRequest request)
        {
            string strEventsLogicCache = "", strEventsDetailLogicCache = "";
            var Response = new List<EventDetail>();

            #region EVENT LISTING

            try
            {
                strEventsLogicCache =
                    (string) Common.clsCommonFunction.GetCacheValue(CacheKey.Events + "_" + request.languageid);
            }
            catch (Exception)
            {
                strEventsLogicCache = "";
            }

            if (!string.IsNullOrEmpty(strEventsLogicCache))
            {
                Response = JsonConvert.DeserializeObject<List<EventDetail>>(strEventsLogicCache);
            }
            else
            {
                var objResponseDB = objEventsDTO.GetEventsResponse("", KeyValues.EventListValidCacheAgeMinute);

                if (objResponseDB != null && objResponseDB.Count > 0)
                {
                    var intValidTime = 0;
                    foreach (var item in objResponseDB)
                    {
                        strEventsLogicCache = Convert.ToString(item.data);
                        intValidTime = Convert.ToInt32(item.mintime);
                    }

                    Response = JsonConvert.DeserializeObject<List<EventDetail>>(strEventsLogicCache);
                    Common.clsCommonFunction.AddInCacheMemory(CacheKey.Events + "_" + request.languageid,
                        strEventsLogicCache, intValidTime);
                }

                if (Response == null || string.IsNullOrEmpty(strEventsLogicCache))
                {
                    request.type = EventType.Listing;
                    Response = CallEventsLogicAPI(request, ref strEventsLogicCache);
                    Common.clsCommonFunction.AddInCacheMemory(CacheKey.Events + "_" + request.languageid,
                        strEventsLogicCache, KeyValues.EventListValidCacheAgeMinute);
                }
            }

            #endregion

            #region EVENT DETAIL

            string strCurrentTime = "", strSelectedRecId = "";
            DateTime dtCurrentDate;

            if (Response != null)
            {
                objCommonFunction.GetCurrentBahrainTime(out strCurrentTime, out dtCurrentDate);

                if (Response != null && Response.Count > 0)
                {
                    var EventsRequest = new List<EventsRequest>();
                    request.type = EventType.Detail;
                    DateTime temp;
                    var TotalDisplayEventCount = 0;
                    var OngingEventResponse = (from a in Response
                        where Convert.ToDateTime(a.fromdate) <= dtCurrentDate &&
                              (DateTime.TryParse(a.todate, out temp)
                                  ? temp >= dtCurrentDate
                                  : DateTime.TryParse(a.todate, out temp))
                              || Convert.ToDateTime(a.fromdate) == dtCurrentDate &&
                              (DateTime.TryParse(a.todate, out temp)
                                  ? temp == dtCurrentDate
                                  : DateTime.TryParse(a.todate, out temp))
                        select a).Distinct().OrderBy(o => o.fromdate).ToList();

                    if (request.Count > 0) OngingEventResponse = OngingEventResponse.Take(request.Count).ToList();

                    var OngoingEventResponseCount = OngingEventResponse?.ToList()?.Count > 0
                        ? OngingEventResponse.ToList().Count
                        : 0;

                    if (request.Count > 0) TotalDisplayEventCount = request.Count - OngoingEventResponseCount;

                    var UpcomingEventResponse = (from b in Response
                        where Convert.ToDateTime(b.fromdate) > dtCurrentDate &&
                              (DateTime.TryParse(b.todate, out temp)
                                  ? temp > dtCurrentDate
                                  : DateTime.TryParse(b.todate, out temp))
                        select b).Distinct().OrderBy(o => o.fromdate).ToList();


                    if (TotalDisplayEventCount > 0)
                    {
                        UpcomingEventResponse = UpcomingEventResponse.Take(TotalDisplayEventCount).ToList();
                        Response = OngingEventResponse.Concat(UpcomingEventResponse).OrderBy(x => x.fromdate).ToList();
                    }
                    else
                    {
                        Response = OngingEventResponse.OrderBy(x => x.fromdate).ToList();
                    }


                    strSelectedRecId = request.recid = string.Join(",", from r in Response select r.recid);
                    HttpContext.Current.Trace.Warn("Event Recid ::" + strSelectedRecId);
                }
            }

            var objResponseCount = new List<EventDetail>();
            try
            {
                strEventsDetailLogicCache =
                    (string) Common.clsCommonFunction.GetCacheValue(CacheKey.EventsDetail + "_" + request.recid + "_" +
                                                                    request.languageid);
                if (!string.IsNullOrEmpty(strEventsDetailLogicCache))
                    objResponseCount = JsonConvert.DeserializeObject<List<EventDetail>>(strEventsDetailLogicCache);
            }
            catch (Exception)
            {
                strEventsDetailLogicCache = "";
            }

            if (!string.IsNullOrEmpty(strEventsDetailLogicCache) && objResponseCount.Count > 0)
            {
                Response = JsonConvert.DeserializeObject<List<EventDetail>>(strEventsDetailLogicCache);
            }
            else
            {
                var objResponseDB = objEventsDTO.GetEventDetailsResponse(EventType.Detail, strSelectedRecId,
                    KeyValues.EventDetailsValidCacheAgeMinute);

                if (objResponseDB != null && objResponseDB.Count > 0)
                {
                    var intValidTime = 0;
                    foreach (var item in objResponseDB)
                    {
                        strEventsDetailLogicCache = Convert.ToString(item.data);
                        intValidTime = Convert.ToInt32(item.mintime);
                    }

                    Response = JsonConvert.DeserializeObject<List<EventDetail>>(strEventsDetailLogicCache);
                    Common.clsCommonFunction.AddInCacheMemory(
                        CacheKey.EventsDetail + "_" + strSelectedRecId + "_" + request.languageid,
                        strEventsDetailLogicCache, intValidTime);
                }

                //if (Response == null || objResponseCount.Count == 0)
                //{
                //    Response = CallEventsLogicAPI(request, ref strEventsDetailLogicCache);
                //    Common.clsCommonFunction.AddInCacheMemory(
                //        CacheKey.EventsDetail + "_" + strSelectedRecId + "_" + request.languageid,
                //        strEventsDetailLogicCache, KeyValues.EventListValidCacheAgeMinute);
                //}
            }

            if (Response != null) Response = Response?.OrderBy(x => x.e_fromdate)?.ToList();

            #endregion

            return Response;
        }
    }
}