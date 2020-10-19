using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace WebApiRedirector.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HelloWorld : ControllerBase
    {
        [HttpGet("/[controller]")]
        public IActionResult Index()
        {
            var objToPrint = JsonConvert.SerializeObject(Request.Headers.Values);
            var objToPrintv2 = JsonConvert.SerializeObject(Request.Headers.Keys);
            var objToPrintv3 = JsonConvert.SerializeObject(Request.HttpContext.User.Identity);
            return Ok($"helloworld {objToPrintv3} ############# {objToPrintv2}");

        }
    }
}