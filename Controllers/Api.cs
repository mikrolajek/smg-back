using System;
using System.Net;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiRedirector.Controllers
{
    [ApiController]
    [Route("")]
    public class ApiController : ControllerBase
    {
        private ApplicationDbContext _context;
        public ApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // [HttpGet("{type?}/{uqCode?}")]
        [HttpGet("{uqCode?}")]
        public IActionResult Get(string uqCode)
        {

            if (uqCode == "google") return Redirect("https://google.com");
            if (uqCode == "samsung") return Redirect("https://samsung.com");
            if (uqCode == "ebay") return Redirect("https://ebay.com");

            return (StatusCode(404));
            // Console.WriteLine(uqCode);

            // Branch ctxBranch;
            // using (_context)
            // {
            //     ctxBranch = _context.Branches.FirstOrDefault(b => b.GuidLink == uqCode);
            // }

            // return Redirect($@"https://www.allegro.pl/");
            // return new ContentResult
            // {

            //     ContentType = "text/html",
            //     StatusCode = (int)HttpStatusCode.OK,
            //     Content = HtmlBuilderUtil.Link(ctxBranch.Url)
            //     // Content = "Git, zapisalo sie!"
            //     // Content = HtmlBuilderUtil.Link(name)
            // };
        }
    }
}

// using (_context)
//             {
//                 _context.Branches.Add(new Branch
//                 {
//                     Url = "savoir",
//                     Location = "Warszawa",
//                     GuidLink = "identyfikator-1"
//                 });

//                 _context.Branches.Add(new Branch
//                 {
//                     Url = "google",
//                     Location = "Kraków",
//                     GuidLink = "identyfikator-2"
//                 });

//                 _context.SaveChanges();
//             }