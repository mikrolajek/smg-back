using System;
using System.Net;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using WebApiRedirector.Models;
using WebApiRedirector.Persistance;
// using WebApiRedirector.Persistance;
// using WebApiRedirector.Models;

namespace WebApiRedirector.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DBConnectTestController : ControllerBase
    {
        private postgresContext _context;
        public DBConnectTestController(postgresContext context)
        {
            _context = context;
        }

        [HttpPost("inserttest")]
        public async Task<IActionResult> Post(string uqCode)
        {

            var qrCode = new Code { Type = "QR", Uid = $"Q{RandomString.Generate(7)}" };
            var nfcTag = new Code { Type = "NFC", Uid = $"N{RandomString.Generate(7)}" };

            using (_context)
            {
                await _context.Code.AddRangeAsync(qrCode, nfcTag);
                await _context.SaveChangesAsync();

            }

            return Content($"QR: {qrCode.Id}, NFC: {nfcTag.Id}");
        }
    }
}
