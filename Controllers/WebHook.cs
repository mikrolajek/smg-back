

using System;
using Microsoft.AspNetCore.Mvc;

namespace WebApiRedirector.Controllers
{
    [ApiController]
    [Route("webhook")]
    public class WebHookController : ControllerBase
    {
        [HttpPost]
        public IActionResult Post(object obj)
        {
            Console.WriteLine(obj);
            return StatusCode(200);
        }
    }

}