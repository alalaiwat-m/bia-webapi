using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace BACWebAPI.Models.Data
{
    public class TwitterAPI
    {
        private const string TwitterApiBaseUrl = "https://api.twitter.com/1.1/";
        private readonly string consumerKey, consumerKeySecret, accessToken, accessTokenSecret;
        private readonly DateTime epochUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly HMACSHA1 sigHasher;

        /// <summary>
        ///     Creates an object for sending tweets to Twitter using Single-user OAuth.
        ///     Get your access keys by creating an app at apps.twitter.com then visiting the
        ///     "Keys and Access Tokens" section for your app. They can be found under the
        ///     "Your Access Token" heading.
        /// </summary>
        public TwitterAPI(string consumerKey, string consumerKeySecret, string accessToken, string accessTokenSecret)
        {
            this.consumerKey = consumerKey;
            this.consumerKeySecret = consumerKeySecret;
            this.accessToken = accessToken;
            this.accessTokenSecret = accessTokenSecret;

            sigHasher = new HMACSHA1(
                new ASCIIEncoding().GetBytes(string.Format("{0}&{1}", consumerKeySecret, accessTokenSecret)));
        }

        public TwitterAPI()
        {
            consumerKey = ConfigurationManager.AppSettings.Get("consumerKey");
            consumerKeySecret = ConfigurationManager.AppSettings.Get("consumerKeySecret");
            accessToken = ConfigurationManager.AppSettings.Get("accessToken");
            accessTokenSecret = ConfigurationManager.AppSettings.Get("accessTokenSecret");

            sigHasher = new HMACSHA1(
                new ASCIIEncoding().GetBytes(string.Format("{0}&{1}", consumerKeySecret, accessTokenSecret)));
        }

        /// <summary>
        ///     Sends a tweet with the supplied text and returns the response from the Twitter API.
        /// </summary>
        public Task<string> Tweet(string text, string strUsername)
        {
            var data = new Dictionary<string, string>
            {
                {"status", text},
                {"trim_user", "1"}
            };
            var Response = SendRequest("statuses/update.json", data, strUsername);
            return Response;
        }

        private Task<string> SendRequest(string url, Dictionary<string, string> data, string strUsername)
        {
            var fullUrl = TwitterApiBaseUrl + url;

            // Timestamps are in seconds since 1/1/1970.
            var timestamp = (int) (DateTime.UtcNow - epochUtc).TotalSeconds;

            // Add all the OAuth headers we'll need to use when constructing the hash.
            data.Add("oauth_consumer_key", consumerKey);
            data.Add("oauth_signature_method", "HMAC-SHA1");
            data.Add("oauth_timestamp", timestamp.ToString());
            data.Add("oauth_nonce", "a"); // Required, but Twitter doesn't appear to use it, so "a" will do.
            data.Add("oauth_token", accessToken);
            data.Add("oauth_version", "1.0");

            // Generate the OAuth signature and add it to our payload.
            data.Add("oauth_signature", GenerateSignature(fullUrl, data));

            // Build the OAuth HTTP Header from the data.
            var oAuthHeader = GenerateOAuthHeader(data);

            // Build the form data (exclude OAuth stuff that's already in the header).
            var formData = new FormUrlEncodedContent(data.Where(kvp => !kvp.Key.StartsWith("oauth_")));

            return SendRequest(fullUrl, oAuthHeader, formData);
        }

        /// <summary>
        ///     Generate an OAuth signature from OAuth header values.
        /// </summary>
        private string GenerateSignature(string url, Dictionary<string, string> data)
        {
            var sigString = string.Join(
                "&",
                data
                    .Union(data)
                    .Select(kvp =>
                        string.Format("{0}={1}", Uri.EscapeDataString(kvp.Key), Uri.EscapeDataString(kvp.Value)))
                    .OrderBy(s => s)
            );

            var fullSigData = string.Format(
                "{0}&{1}&{2}",
                "POST",
                Uri.EscapeDataString(url),
                Uri.EscapeDataString(sigString)
            );

            return Convert.ToBase64String(sigHasher.ComputeHash(new ASCIIEncoding().GetBytes(fullSigData)));
        }

        /// <summary>
        ///     Generate the raw OAuth HTML header from the values (including signature).
        /// </summary>
        private string GenerateOAuthHeader(Dictionary<string, string> data)
        {
            return "OAuth " + string.Join(
                ", ",
                data
                    .Where(kvp => kvp.Key.StartsWith("oauth_"))
                    .Select(kvp =>
                        string.Format("{0}=\"{1}\"", Uri.EscapeDataString(kvp.Key), Uri.EscapeDataString(kvp.Value)))
                    .OrderBy(s => s)
            );
        }

        /// <summary>
        ///     Send HTTP Request and return the response.
        /// </summary>
        private async Task<string> SendRequest(string fullUrl, string oAuthHeader, FormUrlEncodedContent formData)
        {
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("Authorization", oAuthHeader);

                var httpResp = http.PostAsync(fullUrl, formData);
                if (httpResp.Result.IsSuccessStatusCode)
                    HttpContext.Current.Trace.Warn("Twitter API Status Ok:: " + httpResp.Result.IsSuccessStatusCode);
                else
                    HttpContext.Current.Trace.Warn("Twitter API Status Not Ok:: " +
                                                   httpResp.Result.IsSuccessStatusCode);
                var respBody = await httpResp.Result.Content.ReadAsStringAsync();
                return respBody;
            }
        }

        public static TwitterDto TwitterLogin(string oauth_token, string oauth_token_secret, string oauth_consumer_key,
            string oauth_consumer_secret, string strUsername, string oAuthHeader)
        {
            var ValidateResonse = new TwitterDto();
            try
            {
                // oauth implementation details
                var oauth_version = "1.0";
                var oauth_signature_method = "HMAC-SHA1";

                // unique request details
                var oauth_nonce = "a";
                var timeSpan = DateTime.UtcNow
                               - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                var oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();

                var resource_url =
                    "https://api.twitter.com/labs/2/users/by?usernames=" + strUsername ;

                // create oauth signature
                var baseFormat = "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" +
                                 "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}";

                var baseString = string.Format(baseFormat,
                    oauth_consumer_key,
                    oauth_nonce,
                    oauth_signature_method,
                    oauth_timestamp,
                    oauth_token,
                    oauth_version
                );

                baseString = string.Concat("GET&", Uri.EscapeDataString(resource_url), "%26",
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
                var headerFormat =
                    "OAuth oauth_consumer_key=\"{0}\", oauth_nonce=\"{1}\", oauth_signature=\"{2}\", oauth_signature_method=\"{3}\", oauth_timestamp=\"{4}\", oauth_token=\"{5}\", oauth_version=\"{6}\"";

                var authHeader = string.Format(headerFormat,
                    Uri.EscapeDataString(oauth_consumer_key),
                    Uri.EscapeDataString(oauth_nonce),
                    Uri.EscapeDataString(oauth_signature),
                    Uri.EscapeDataString(oauth_signature_method),
                    Uri.EscapeDataString(oauth_timestamp),
                    Uri.EscapeDataString(oauth_token),
                    Uri.EscapeDataString(oauth_version)
                );


                // make the request
                ServicePointManager.Expect100Continue = false;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 |
                                                       SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
                //resource_url += "?include_email=false";
                var request = (HttpWebRequest) WebRequest.Create(resource_url);
                request.Headers.Add("Authorization", oAuthHeader);
                request.Method = "GET";
                HttpContext.Current.Trace.Warn("Header Authorization ::" + oAuthHeader);
                HttpContext.Current.Trace.Warn("Header authHeader Authorization ::" + authHeader);

                var response = request.GetResponse();
                HttpContext.Current.Trace.Warn("Twitter Validdate Resposne ::" + response);
                ValidateResonse =
                    JsonConvert.DeserializeObject<TwitterDto>(
                        new StreamReader(response.GetResponseStream()).ReadToEnd());
            }
            catch (Exception ex)
            {
                HttpContext.Current.Trace.Warn("Error Message For Twiiter ::" + ex.Message);
                HttpContext.Current.Trace.Warn("Error Message For Twiiter ::" + ex.InnerException);
            }

            HttpContext.Current.Trace.Warn("Validdate after Resposne ::" + ValidateResonse);
            return ValidateResonse;
        }
    }

    public class TwitterDto
    {
        public string name { get; set; }
        public string email { get; set; }
    }
}