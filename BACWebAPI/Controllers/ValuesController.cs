using System.Web.Http;
using BACWebAPI.Models.JWT;

namespace BACWebAPI.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        [ActionName("values")]
        [HttpGet]
        [AllowAnonymous]
        public string Get()
        {
            if (CheckUser("", "")) return JWTToken.GenerateToken("bacAirport");
            return "";
        }

        public bool CheckUser(string username, string password)
        {
            // should check in the database
            return true;
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}