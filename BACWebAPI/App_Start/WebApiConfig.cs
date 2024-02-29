using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using Microsoft.Owin.Security.OAuth;

namespace BACWebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services    
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // Web API routes
            config.MapHttpAttributeRoutes();

            //config.Filters.Add(new ValidateModelAttribute());

            config.Filters.Add(new AuthorizeAttribute());

            config.Routes.MapHttpRoute(
                "WebApi",
                "api/{controller}/{action}/{id}",
                new {id = RouteParameter.Optional}
            );

            //DEFAULT JSON API RESULT
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            // ADDING FORMATTER FOR XML => ?type=xml
            config.Formatters.XmlFormatter.MediaTypeMappings.Add(
                new QueryStringMapping("type", "xml", new MediaTypeHeaderValue("application/xml")));
        }
    }
}