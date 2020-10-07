using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebApiRedirector.Persistance;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System;
using System.Text;
using System.Linq;
using CsvHelper.Configuration;
using WebApiRedirector.Controllers;

namespace WebApiRedirector.Controllers
{
    [ApiController]
    [Route("")]
    public class ApiController : ControllerBase
    {
        private postgresContext _context;
        public ApiController(postgresContext context)
        {
            _context = context;
        }

        // [HttpGet("{type?}/{uqCode?}")]
        [HttpGet("{uqCode?}")]
        public async Task<ActionResult<string>> Get(string uqCode)
        {
            var codeInDb = await _context.Code.FirstOrDefaultAsync(c => c.Uid == uqCode);

            return (codeInDb is null) ?
            "Nie ma takiego zasobu" :
            $"Jest, oraz ma id {codeInDb.Id}";
        }

        [HttpPost("/add/sheet")]
        public ActionResult<IEnumerable<DataSheetSamsungSource>> AddSheet([FromBody] IEnumerable<DataSheetSamsungSource> file)
        {
            var result = file.GroupBy(item => item.Siec).Select(group => group.FirstOrDefault().Siec);
            return Ok(result);
        }
        // private void CsvConverterListReturner(out IEnumerable<DataSheetSamsungSource> records, string decodedString)
        // {
        //     IEnumerable<DataSheetSamsungSource> _records;
        //     byte[] data = Convert.FromBase64String(file.csv);
        //     string decodedString = Encoding.UTF8.GetString(data);

        //     using (var reader = new StringReader(decodedString))
        //     using (var csvReader = new CsvReader(reader, CultureInfo.CurrentCulture))
        //     {
        //         records = csvReader.GetRecords<DataSheetSamsungSource>();
        //     }
        // }
    }

}

// if (uqCode == "google") return Redirect("https://google.com");
// if (uqCode == "samsung") return Redirect("https://samsung.com");
// if (uqCode == "amazon") return Redirect("https://amazon.com");
// if (uqCode == "ebay") return Redirect("https://ebay.com");

// return (StatusCode(404));


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

public class Foo
{
    public int Id { get; set; }
    public string Name { get; set; }
}