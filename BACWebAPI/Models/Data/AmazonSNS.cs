using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Web;
using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using BACWebAPI.Models.Common;

namespace BACWebAPI.Models.Data
{
    public class AmazonSNS
    {
        private clsCommonFunction objCommonFunction = new clsCommonFunction();


        public static bool SendSMSNotification(string strPhoneNumber, string message)
        {
            var accessKey =
                Convert.ToString(ConfigurationManager.AppSettings["SMSAccessKey"]); 
            var secretKey =
                Convert.ToString(
                    ConfigurationManager.AppSettings["SMSSecretKey"]);
            var client = new AmazonSimpleNotificationServiceClient(accessKey, secretKey, RegionEndpoint.EUWest1);
            var messageAttributes = new Dictionary<string, MessageAttributeValue>();
            var blnSendSMS = false;
            try
            {
                
                var smsType = new MessageAttributeValue
                {
                    DataType = "String",
                    StringValue = "Transactional"
                };

                messageAttributes.Add("AWS.SNS.SMS.SMSType", smsType);

                var request = new PublishRequest
                {
                    Message = message,
                    PhoneNumber = strPhoneNumber,
                    MessageAttributes = messageAttributes
                };

                var pubResponse = client.Publish(request);

                if (pubResponse.HttpStatusCode == HttpStatusCode.OK) blnSendSMS = true;
            }
            catch (Exception ex)
            {
                HttpContext.Current.Trace.Warn("SMS ERROR MESSAGE ::" + ex.Message);
            }

            return blnSendSMS;
        }
    }
}