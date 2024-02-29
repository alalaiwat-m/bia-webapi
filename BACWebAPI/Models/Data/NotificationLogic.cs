using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http.ModelBinding;
using BACWebAPI.Models.Service;
using BusinessClassLibrary.EntityClass;
using BusinessClassLibrary.LogicClass.Common;
using static BusinessClassLibrary.LogicClass.Common.clsEnum;
using clsCommonFunction = BACWebAPI.Models.Common.clsCommonFunction;

namespace BACWebAPI.Models.Data
{
    public class NotificationLogic
    {
        private AlertContent alertContent;
        private readonly flightLogic flightLogic = new flightLogic();
        private readonly clsEmailService objEmailService = new clsEmailService();
        private readonly clsNotificationService objNotificationService = new clsNotificationService();

        public void AddNotification(AlertRequest alertRequest, ref CommonResponse<Alert> objResponse,
            ModelStateDictionary modalState)
        {
            var objError = new List<clsMessage>();
            bool blnSuccess = false, blnNotificationSent = false;
            var strMessage = "";

            if (modalState.IsValid)
            {
                try
                {
                    var lstFlightData = new List<FlightDetail>();
                    if (alertRequest.source == FlightType.Arrival)
                        lstFlightData = flightLogic.BindArrivalFlights();
                    else
                        lstFlightData = flightLogic.BindDepartureFlights();
                    if (lstFlightData != null && lstFlightData.Count > 0)
                    {
                        var validData = lstFlightData
                            .Where(s => s.flight.Replace(" ", "") == alertRequest.flightnumber &&
                                        s.remarks == alertRequest.currentstatus &&
                                        s.scheduledDate == alertRequest.scheduledate).ToList();
                        if (validData != null && validData.Count > 0)
                        {
                            var lstAlert = objNotificationService.getNotificationByFlightDetails(
                                alertRequest.flightnumber, alertRequest.scheduledate, alertRequest.value,
                                alertRequest.type, alertRequest.airlinecode, alertRequest.source);

                            if (lstAlert != null && lstAlert.Count > 0)
                            {
                                if (alertRequest.type == AlertType.TWITTER)
                                    strMessage = alertRequest.isarabic
                                        ? "لقد سجلت لهذه الرحلة بالفعل بحساب \"Twitter\" نفسه."
                                        : "You have already registered with same twitter account for this flight.";
                                else if (alertRequest.type == AlertType.EMAIL)
                                    strMessage = alertRequest.isarabic
                                        ? "لقد سجلت لهذه الرحلة بالفعل بعنوان البريد الإلكتروني نفسه."
                                        : "You have already registered with same email for this flight.";
                                else if (alertRequest.type == AlertType.SMS)
                                    strMessage = alertRequest.isarabic
                                        ? "لقد سجلت لهذه الرحلة بالفعل برقم الهاتف النقّال نفسه."
                                        : "You have already registered with same mobile number for this flight.";
                                blnSuccess = false;
                            }
                            else
                            {
                                objNotificationService.InsertFlightNotification(alertRequest.airlinecode,
                                    alertRequest.flightnumber, alertRequest.currentstatus, alertRequest.scheduledate,
                                    alertRequest.value, alertRequest.type, alertRequest.source, alertRequest.isarabic);
                                bindAlertContent();
                                string strTweetMsg = "", strFromTo = "";
                                if (alertRequest.source == FlightType.Arrival)
                                    strFromTo = alertRequest.origin + " - BAH";
                                else
                                    strFromTo = "BAH - " + alertRequest.origin;
                                if (alertRequest.isarabic)
                                    strTweetMsg = alertContent.NewRegisteredTweetMsgAr
                                        .Replace("[#USERNAME#]",
                                            alertRequest.type == AlertType.TWITTER
                                                ? "@" + alertRequest.value.TrimStart('@')
                                                : "")
                                        .Replace("[#FLIGHTFROMTO#]", strFromTo)
                                        .Replace("[#FLIGHTNO#]", alertRequest.flightnumber);
                                else
                                    strTweetMsg = alertContent.NewRegisteredTweetMsg
                                        .Replace("[#USERNAME#]",
                                            alertRequest.type == AlertType.TWITTER
                                                ? "@" + alertRequest.value.TrimStart('@')
                                                : "")
                                        .Replace("[#FLIGHTFROMTO#]", strFromTo)
                                        .Replace("[#FLIGHTNO#]", alertRequest.flightnumber);
                                if (alertRequest.type == AlertType.TWITTER)
                                {
                                    var objTwitterAuthenticate = new TwitterAuthenticate();

                                    //objTwitterAuthenticate.findUserTwitter("https://api.twitter.com/1.1/statuses/user_timeline.json", alertRequest.value.TrimStart('@'));

                                    var twitter = new TwitterAPI();

                                    var response = twitter.Tweet(strTweetMsg, alertRequest.value.TrimStart('@'));
                                    if (response != null)
                                        HttpContext.Current.Trace.Warn("Twitter Response :: " + response?.Status);
                                }
                                else if (alertRequest.type == AlertType.EMAIL)
                                {
                                    var strEmailContent = alertContent.AlertEmailBody;
                                    var strEmailSubject = alertContent.AlertEmailSubject;
                                    if (alertRequest.isarabic)
                                    {
                                        strEmailContent = alertContent.ArAlertEmailBody;
                                        strEmailSubject = alertContent.ArAlertEmailSubject;
                                    }

                                    strEmailContent = strEmailContent.Replace("[#MESSAGE#]", strTweetMsg);

                                    var dctMessage = new Dictionary<string, string>();
                                    dctMessage.Add("toemail", alertRequest.value);
                                    dctMessage.Add("subject", strEmailSubject);
                                    dctMessage.Add("messagecontent", strEmailContent);

                                    blnNotificationSent = AmazonSES.AmozonSESSendMail(dctMessage);
                                }
                                else if (alertRequest.type == AlertType.SMS)
                                {
                                    AmazonSNS.SendSMSNotification(alertRequest.value, strTweetMsg);
                                }

                                strMessage = alertRequest.isarabic
                                    ? "تم التسجيل بنجاح."
                                    : "You have successfully registered.";
                                blnSuccess = true;
                            }
                        }
                        else
                        {
                            blnSuccess = false;
                            strMessage = alertRequest.isarabic
                                ? "الرحلة المطلوبة غير متوفرة."
                                : "Requested flight is not available.";
                        }
                    }

                    objResponse = new CommonResponse<Alert> {success = blnSuccess, message = strMessage, data = null};
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Trace.Warn("Get :: " + ex.Message);
                    HttpContext.Current.Trace.Warn("Error Message Exception ::" + ex.InnerException);
                    objError.Add(new clsMessage {code = 500, message = "Internal Server Error."});
                    objResponse = new CommonResponse<Alert> {success = false, error = objError};
                }
            }
            else
            {
                clsCommonFunction.SetModelError(modalState, ref objError);
                objResponse = new CommonResponse<Alert> {success = false, error = objError};
            }
        }

        public void getAllAlertsDetails(ref CommonResponse<List<Alert>> objResponse)
        {
            var lstData = objNotificationService.getAllAlerts();

            var lstFlightArrival = flightLogic.BindArrivalFlights();
            var lstFlightDeparture = flightLogic.BindDepartureFlights();

            var lstResultData = new List<Alert>();

            if (lstData != null && lstData.Count > 0)
            {
                foreach (var item in lstData.Where(a => a.source == FlightType.Arrival))
                {
                    var tmpFlightArrival = lstFlightArrival
                        .Where(s => s.flight.Replace(" ", "") == item.flightnumber && s.remarks != item.currentstatus &&
                                    s.scheduledDate == item.scheduledDate)
                        .ToList().FirstOrDefault();
                    if (tmpFlightArrival != null)
                        //int intLastInsertedId = objNotificationService.InsertFlightNotificationHistory(item.flightnotificationid, tmpFlightArrival.remarks, true);
                        lstResultData.Add(new Alert
                        {
                            airlinecode = tmpFlightArrival.airlineCode,
                            currentstatus = tmpFlightArrival.remarks,
                            flightnotificationid = item.flightnotificationid,
                            flightnumber = tmpFlightArrival.flight,
                            isarabic = item.isarabic,
                            rNo = item.rNo,
                            type = item.type,
                            value = item.value,
                            source = item.source,
                            scheduledTime = tmpFlightArrival.scheduledTime,
                            scheduledDate = tmpFlightArrival.scheduledDate,
                            estimatedDate = tmpFlightArrival.estimatedDate,
                            estimatedTime = tmpFlightArrival.estimatedTime,
                            currentTime = tmpFlightArrival.currentTime,
                            currentDate = tmpFlightArrival.currentDate
                        });
                }

                foreach (var item in lstData.Where(a => a.source == FlightType.Departure))
                {
                    var tmpFlightDepart = lstFlightDeparture
                        .Where(s => s.flight.Replace(" ", "") == item.flightnumber && s.remarks != item.currentstatus &&
                                    s.scheduledDate == item.scheduledDate)
                        .ToList().FirstOrDefault();
                    if (tmpFlightDepart != null)
                        lstResultData.Add(new Alert
                        {
                            airlinecode = tmpFlightDepart.airlineCode,
                            currentstatus = tmpFlightDepart.remarks,
                            flightnotificationid = item.flightnotificationid,
                            flightnumber = tmpFlightDepart.flight,
                            isarabic = item.isarabic,
                            rNo = item.rNo,
                            type = item.type,
                            value = item.value,
                            source = item.source,
                            scheduledTime = tmpFlightDepart.scheduledTime,
                            scheduledDate = tmpFlightDepart.scheduledDate,
                            estimatedDate = tmpFlightDepart.estimatedDate,
                            estimatedTime = tmpFlightDepart.estimatedTime,
                            currentTime = tmpFlightDepart.currentTime,
                            currentDate = tmpFlightDepart.currentDate
                        });
                }
            }

            if (lstResultData != null && lstResultData.Count > 0)
            {
                var TweetNotify = lstResultData.Where(t => t.type.Trim() == AlertType.TWITTER).ToList();
                var EmailNotify = lstResultData.Where(t => t.type.Trim() == AlertType.EMAIL).ToList();
                var SMSNotify = lstResultData.Where(t => t.type.Trim() == AlertType.SMS).ToList();
                bindAlertContent();

                if (TweetNotify != null && TweetNotify.Count > 0)
                {
                    var blnTwitterSend = false;
                    var twitter = new TwitterAPI();
                    var strStatus = "";

                    foreach (var item in TweetNotify)
                    {
                        strStatus = item.currentstatus;
                        var strTweetMsg = "";
                        switch (item.currentstatus)
                        {
                            case FlightRemarks.Landed:
                                if (item.isarabic)
                                    strTweetMsg = alertContent.LandedFlightMsgAr;
                                else
                                    strTweetMsg = alertContent.LandedFlightMsg;
                                break;
                            case FlightRemarks.Depart:
                                if (item.isarabic)
                                    strTweetMsg = alertContent.DepartedFlightMsgAr;
                                else
                                    strTweetMsg = alertContent.DepartedFlightMsg;
                                break;
                            case FlightRemarks.Early:
                            case FlightRemarks.OnTime:
                                strStatus = item.source == FlightType.Departure ? "departing" : "arriving";

                                if (item.isarabic)
                                    strTweetMsg = alertContent.EarlyFlightMsgAr;
                                else
                                    strTweetMsg = alertContent.EarlyFlightMsg;
                                break;
                            case FlightRemarks.Cancelled:
                                if (item.isarabic)
                                    strTweetMsg = alertContent.CanceledFlightMsgAr;
                                else
                                    strTweetMsg = alertContent.CanceledFlightMsg;
                                break;
                            case FlightRemarks.Delayed:
                                strStatus = item.source == FlightType.Departure ? item.isarabic ? "المغادرة" :
                                    "departure" :
                                    item.isarabic ? "الوصول" : "arrival";
                                if (item.isarabic)
                                    strTweetMsg = alertContent.DelayedFlightMsgAr;
                                else
                                    strTweetMsg = alertContent.DelayedFlightMsg;
                                break;
                            default:
                                strStatus = item.currentstatus;
                                break;
                        }

                        var strFlightDateTime = "";
                        //strFlightDateTime = (!string.IsNullOrEmpty(item.estimatedTime) ? item.estimatedTime : item.scheduledTime) + " ( " + (!string.IsNullOrEmpty(item.estimatedDate) ? item.estimatedDate : item.scheduledDate) + " )";

                        if (item.currentstatus == FlightRemarks.Landed || item.currentstatus == FlightRemarks.Depart)
                            strFlightDateTime =
                                (!string.IsNullOrEmpty(item.currentTime) ? item.currentTime : item.scheduledTime) +
                                " (" + (!string.IsNullOrEmpty(item.currentDate)
                                    ? item.currentDate
                                    : item.scheduledDate) + ")";
                        else
                            strFlightDateTime =
                                (!string.IsNullOrEmpty(item.estimatedTime) ? item.estimatedTime : item.scheduledTime) +
                                " (" + (!string.IsNullOrEmpty(item.estimatedDate)
                                    ? item.estimatedDate
                                    : item.scheduledDate) + ")";

                        strTweetMsg = strTweetMsg
                            .Replace("[#USERNAME#]", "@" + item.value.TrimStart('@'))
                            .Replace("[#DATETIME#]", strFlightDateTime)
                            .Replace("[#STATUS#]", strStatus)
                            .Replace("[#FLIGHTNO#]", item.flightnumber);

                        var response = twitter.Tweet(strTweetMsg, item.value.TrimStart('@'));
                        var intLastInsertedId =
                            objNotificationService.InsertFlightNotificationHistory(item.flightnotificationid,
                                item.currentstatus, true);
                        lstResultData.Where(x => x.flightnotificationid == item.flightnotificationid).ToList()
                            .ForEach(i => { i.flightnotificationhistoryid = intLastInsertedId; });
                    }
                }

                if (EmailNotify != null && EmailNotify.Count > 0)
                {
                    //Email Logic HERE 
                    var blnEmailSend = false;
                    var strStatus = "";
                    foreach (var item in EmailNotify)
                    {
                        var strEmailContent =
                            item.isarabic ? alertContent.ArAlertEmailBody : alertContent.AlertEmailBody;
                        strStatus = item.currentstatus;
                        var strEmailMsg = "";
                        switch (item.currentstatus)
                        {
                            case FlightRemarks.Landed:
                                if (item.isarabic)
                                    strEmailMsg = alertContent.LandedFlightMsgAr;
                                else
                                    strEmailMsg = alertContent.LandedFlightMsg;
                                break;
                            case FlightRemarks.Depart:

                                if (item.isarabic)
                                    strEmailMsg = alertContent.DepartedFlightMsgAr;
                                else
                                    strEmailMsg = alertContent.DepartedFlightMsg;
                                break;
                            case FlightRemarks.Early:
                            case FlightRemarks.OnTime:
                                strStatus = item.source == FlightType.Departure ? "departing" : "arriving";

                                if (item.isarabic)
                                    strEmailMsg = alertContent.EarlyFlightMsgAr;
                                else
                                    strEmailMsg = alertContent.EarlyFlightMsg;
                                break;
                            case FlightRemarks.Cancelled:
                                if (item.isarabic)
                                    strEmailMsg = alertContent.CanceledFlightMsgAr;
                                else
                                    strEmailMsg = alertContent.CanceledFlightMsg;
                                break;
                            case FlightRemarks.Delayed:
                                strStatus = item.source == FlightType.Departure ? item.isarabic ? "المغادرة" :
                                    "departure" :
                                    item.isarabic ? "الوصول" : "arrival";
                                if (item.isarabic)
                                    strEmailMsg = alertContent.DelayedFlightMsgAr;
                                else
                                    strEmailMsg = alertContent.DelayedFlightMsg;
                                break;
                            default:
                                strStatus = item.currentstatus;
                                break;
                        }

                        var strFlightDateTime = "";

                        if (item.currentstatus == FlightRemarks.Landed || item.currentstatus == FlightRemarks.Depart)
                            strFlightDateTime =
                                (!string.IsNullOrEmpty(item.currentTime) ? item.currentTime : item.scheduledTime) +
                                " (" + (!string.IsNullOrEmpty(item.currentDate)
                                    ? item.currentDate
                                    : item.scheduledDate) + ")";
                        else
                            strFlightDateTime =
                                (!string.IsNullOrEmpty(item.estimatedTime) ? item.estimatedTime : item.scheduledTime) +
                                " (" + (!string.IsNullOrEmpty(item.estimatedDate)
                                    ? item.estimatedDate
                                    : item.scheduledDate) + ")";


                        strEmailMsg = strEmailMsg
                            .Replace("[#DATETIME#]", strFlightDateTime)
                            .Replace("[#STATUS#]", strStatus)
                            .Replace("[#USERNAME#]", "")
                            .Replace("[#FLIGHTNO#]", item.flightnumber);

                        strEmailContent = strEmailContent.Replace("[#MESSAGE#]", strEmailMsg);

                        var dctMessage = new Dictionary<string, string>();

                        dctMessage.Add("toemail", item.value);
                        dctMessage.Add("subject",
                            item.isarabic ? alertContent.ArAlertEmailSubject : alertContent.AlertEmailSubject);
                        dctMessage.Add("messagecontent", strEmailContent);

                        blnEmailSend = AmazonSES.AmozonSESSendMail(dctMessage);
                        if (blnEmailSend)
                        {
                            var intLastInsertedId =
                                objNotificationService.InsertFlightNotificationHistory(item.flightnotificationid,
                                    item.currentstatus, true);
                            lstResultData.Where(x => x.flightnotificationid == item.flightnotificationid).ToList()
                                .ForEach(i => { i.flightnotificationhistoryid = intLastInsertedId; });
                        }
                    }
                }

                #region SMS

                if (SMSNotify != null && SMSNotify.Count > 0)
                {
                    var blnSMSSend = false;
                    //SMS Logic HERE 
                    var objSMSService = new AmazonSNS();
                    var strStatus = "";
                    foreach (var item in SMSNotify)
                    {
                        var strEmailContent = alertContent.AlertEmailBody;
                        strStatus = item.currentstatus;
                        var strSMSMsg = "";
                        switch (item.currentstatus)
                        {
                            case FlightRemarks.Landed:
                                if (item.isarabic)
                                    strSMSMsg = alertContent.LandedFlightMsgAr;
                                else
                                    strSMSMsg = alertContent.LandedFlightMsg;
                                break;
                            case FlightRemarks.Depart:

                                if (item.isarabic)
                                    strSMSMsg = alertContent.DepartedFlightMsgAr;
                                else
                                    strSMSMsg = alertContent.DepartedFlightMsg;
                                break;
                            case FlightRemarks.Early:
                            case FlightRemarks.OnTime:
                                strStatus = item.source == FlightType.Departure ? "departing" : "arriving";

                                if (item.isarabic)
                                    strSMSMsg = alertContent.EarlyFlightMsgAr;
                                else
                                    strSMSMsg = alertContent.EarlyFlightMsg;
                                break;
                            case FlightRemarks.Cancelled:
                                if (item.isarabic)
                                    strSMSMsg = alertContent.CanceledFlightMsgAr;
                                else
                                    strSMSMsg = alertContent.CanceledFlightMsg;
                                break;
                            case FlightRemarks.Delayed:
                                strStatus = item.source == FlightType.Departure ? item.isarabic ? "المغادرة" :
                                    "departure" :
                                    item.isarabic ? "الوصول" : "arrival";

                                if (item.isarabic)
                                    strSMSMsg = alertContent.DelayedFlightMsgAr;
                                else
                                    strSMSMsg = alertContent.DelayedFlightMsg;
                                break;
                            default:
                                strStatus = item.currentstatus;
                                break;
                        }

                        var strFlightDateTime = "";

                        if (item.currentstatus == FlightRemarks.Landed || item.currentstatus == FlightRemarks.Depart)
                            strFlightDateTime =
                                (!string.IsNullOrEmpty(item.currentTime) ? item.currentTime : item.scheduledTime) +
                                " (" + (!string.IsNullOrEmpty(item.currentDate)
                                    ? item.currentDate
                                    : item.scheduledDate) + ")";
                        else
                            strFlightDateTime =
                                (!string.IsNullOrEmpty(item.estimatedTime) ? item.estimatedTime : item.scheduledTime) +
                                " (" + (!string.IsNullOrEmpty(item.estimatedDate)
                                    ? item.estimatedDate
                                    : item.scheduledDate) + ")";

                        strSMSMsg = strSMSMsg
                            .Replace("[#DATETIME#]", strFlightDateTime)
                            .Replace("[#STATUS#]", strStatus)
                            .Replace("[#USERNAME#]", "")
                            .Replace("[#FLIGHTNO#]", item.flightnumber);

                        blnSMSSend = AmazonSNS.SendSMSNotification(item.value, strSMSMsg);
                        if (blnSMSSend)
                        {
                            var intLastInsertedId =
                                objNotificationService.InsertFlightNotificationHistory(item.flightnotificationid,
                                    item.currentstatus, true);
                            lstResultData.Where(x => x.flightnotificationid == item.flightnotificationid).ToList()
                                .ForEach(i => { i.flightnotificationhistoryid = intLastInsertedId; });
                        }
                    }
                }

                #endregion
            }

            objResponse = new CommonResponse<List<Alert>> {success = true, message = "", data = lstResultData};
        }

        public void bindAlertContent()
        {
            var ENGsettings = objEmailService.GetSystemSettings();
            var Emailsettings = objEmailService.GetEmailTemplateByCode("FLIRA");
            var ArEmailsettings = objEmailService.GetEmailTemplateByCode("FLIRA", true);

            alertContent = new AlertContent
            {
                DelayedFlightMsg = ENGsettings.Where(w => w.key == "FLDED" && w.isarabic == false).Select(a => a.value)
                    .FirstOrDefault(),
                DelayedFlightMsgAr = ENGsettings.Where(w => w.key == "FLDED" && w.isarabic).Select(a => a.value)
                    .FirstOrDefault(),
                CanceledFlightMsg = ENGsettings.Where(w => w.key == "FLICD" && w.isarabic == false).Select(a => a.value)
                    .FirstOrDefault(),
                CanceledFlightMsgAr = ENGsettings.Where(w => w.key == "FLICD" && w.isarabic).Select(a => a.value)
                    .FirstOrDefault(),
                EarlyFlightMsg = ENGsettings.Where(w => w.key == "FLIOT" && w.isarabic == false).Select(a => a.value)
                    .FirstOrDefault(),
                EarlyFlightMsgAr = ENGsettings.Where(w => w.key == "FLIOT" && w.isarabic).Select(a => a.value)
                    .FirstOrDefault(),
                DepartedFlightMsg = ENGsettings.Where(w => w.key == "FLIDT" && w.isarabic == false).Select(a => a.value)
                    .FirstOrDefault(),
                DepartedFlightMsgAr = ENGsettings.Where(w => w.key == "FLIDT" && w.isarabic).Select(a => a.value)
                    .FirstOrDefault(),
                LandedFlightMsg = ENGsettings.Where(w => w.key == "FLILD" && w.isarabic == false).Select(a => a.value)
                    .FirstOrDefault(),
                LandedFlightMsgAr = ENGsettings.Where(w => w.key == "FLILD" && w.isarabic).Select(a => a.value)
                    .FirstOrDefault(),
                NewRegisteredEmailMsg = ENGsettings.Where(w => w.key == "NEWRE" && w.isarabic == false)
                    .Select(a => a.value).FirstOrDefault(),
                NewRegisteredEmailMsgAr = ENGsettings.Where(w => w.key == "NEWRE" && w.isarabic).Select(a => a.value)
                    .FirstOrDefault(),
                NewRegisteredTweetMsg = ENGsettings.Where(w => w.key == "NEWRT" && w.isarabic == false)
                    .Select(a => a.value).FirstOrDefault(),
                NewRegisteredTweetMsgAr = ENGsettings.Where(w => w.key == "NEWRT" && w.isarabic).Select(a => a.value)
                    .FirstOrDefault(),
                AlertEmailBody = Emailsettings != null
                    ? BindCompanyDetails(Emailsettings.Select(a => a.emailcontent).FirstOrDefault())
                    : "",
                AlertEmailSubject = Emailsettings != null ? Emailsettings.Select(a => a.subject).FirstOrDefault() : "",
                ArAlertEmailBody = ArEmailsettings != null
                    ? BindCompanyDetails(ArEmailsettings.Select(a => a.emailcontent).FirstOrDefault())
                    : "",
                ArAlertEmailSubject = ArEmailsettings != null
                    ? ArEmailsettings.Select(a => a.subject).FirstOrDefault()
                    : ""
            };
        }

        public string BindCompanyDetails(string strContent)
        {
            if (string.IsNullOrEmpty(strContent)) return "";
            strContent = strContent
                .Replace("#ROOTURL#", Convert.ToString(ConfigurationManager.AppSettings["websiteURL"]).Trim('/'))
                .Replace("data-logosrc", "src");

            return strContent;
        }
    }
}