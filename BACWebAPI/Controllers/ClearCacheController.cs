using System.Web.Http;

namespace BACWebAPI.Controllers
{
    public class ClearCacheController : ApiController
    {
        // POST: api/ClearCache
        [AllowAnonymous]
        public void Post([FromBody] string value)
        {
        }
    }
}