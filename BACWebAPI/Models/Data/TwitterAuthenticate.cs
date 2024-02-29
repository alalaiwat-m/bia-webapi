using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;

namespace BACWebAPI.Models.Data
{
    public class TwitterAuthenticate
    {
        public string query = "umerpasha";

        //    public string url = "https://api.twitter.com/1.1/users/search.json" ;
        public string url = "https://api.twitter.com/1.1/statuses/user_timeline.json";


        public void findUserTwitter(string resource_url, string q)
        {
            try
            {
                // oauth application keys
                var oauth_token = ConfigurationManager.AppSettings.Get("accessToken");
                var oauth_token_secret = ConfigurationManager.AppSettings.Get("accessTokenSecret");
                var oauth_consumer_key = ConfigurationManager.AppSettings.Get("consumerKey");
                var oauth_consumer_secret = ConfigurationManager.AppSettings.Get("consumerKeySecret");

                // oauth implementation details
                var oauth_version = "1.0";
                var oauth_signature_method = "HMAC-SHA1";

                // unique request details
                var oauth_nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
                var timeSpan = DateTime.UtcNow
                               - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                var oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();


                // create oauth signature
                var baseFormat = "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" +
                                 "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}&q={6}";

                var baseString = string.Format(baseFormat,
                    oauth_consumer_key,
                    oauth_nonce,
                    oauth_signature_method,
                    oauth_timestamp,
                    oauth_token,
                    oauth_version,
                    Uri.EscapeDataString(q)
                );

                baseString = string.Concat("GET&", Uri.EscapeDataString(resource_url), "&",
                    Uri.EscapeDataString(baseString));

                var compositeKey = string.Concat(Uri.EscapeDataString(oauth_consumer_secret),
                    "&", Uri.EscapeDataString(oauth_token_secret));

                string oauth_signature;
                using (var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(compositeKey)))
                {
                    oauth_signature = Convert.ToBase64String(
                        hasher.ComputeHash(Encoding.ASCII.GetBytes(baseString)));
                }

                // create the request header
                var headerFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
                                   "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
                                   "oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
                                   "oauth_version=\"{6}\"";

                var authHeader = string.Format(headerFormat,
                    Uri.EscapeDataString(oauth_nonce),
                    Uri.EscapeDataString(oauth_signature_method),
                    Uri.EscapeDataString(oauth_timestamp),
                    Uri.EscapeDataString(oauth_consumer_key),
                    Uri.EscapeDataString(oauth_token),
                    Uri.EscapeDataString(oauth_signature),
                    Uri.EscapeDataString(oauth_version)
                );


                ServicePointManager.Expect100Continue = false;

                // make the request
                var postBody = "screen_name=" + Uri.EscapeDataString(q); //
                resource_url += "?" + postBody;
                var request = (HttpWebRequest) WebRequest.Create(resource_url);
                request.Headers.Add("Authorization", authHeader);
                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded";
                var response = (HttpWebResponse) request.GetResponse();
                var reader = new StreamReader(response.GetResponseStream());
                var objText = reader.ReadToEnd();
                //myDiv.InnerHtml = objText;/**/
                var html = "";

                var jsonDat = JArray.Parse(objText);
                HttpContext.Current.Trace.Warn("jsonDat Twitter ::" + jsonDat);
                for (var x = 0; x < jsonDat.Count(); x++)
                {
                    //html += jsonDat[x]["id"].ToString() + "<br/>";
                    html += jsonDat[x]["text"] + "<br/>";
                    // html += jsonDat[x]["name"].ToString() + "<br/>";
                    html += jsonDat[x]["created_at"] + "<br/>";
                }

                HttpContext.Current.Trace.Warn("Html Twitter ::" + html);
                //myDiv.InnerHtml = html;
            }
            catch (Exception twit_error)
            {
                HttpContext.Current.Trace.Warn("Exception Mesage ::" + twit_error.Message);
                // myDiv.InnerHtml = html + twit_error.ToString();
            }
        }
    }
}