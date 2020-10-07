using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApiRedirector.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HelloWorld : ControllerBase
    {
        [HttpGet("/[controller]")]
        public IActionResult Index()
        {
            return Ok("helloworld, co tam jak tam?");
        }
    }
}