using Newtonsoft.Json;
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
using WebApiRedirector.Models;

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
        public async Task<ActionResult<IEnumerable<DataSheetSamsungSource>>> AddSheet([FromBody] IEnumerable<DataSheetSamsungSource> sheet)
        {
            // var companiesReplacedWithId = await AddMissingCompanies(sheet);
            var sheetCompaniesWithId = await AddMissingCompanies(sheet);
            var sheetProductsWithId = await AddMissingProducts(sheetCompaniesWithId);
            var sheetLinkWithId = await AddMissingLink(sheetProductsWithId);
            var sheetAddressesId = await AddMissingLocations(sheetLinkWithId);


            return Ok(sheetAddressesId);
        }

        private async Task<IEnumerable<DataSheetSamsungSource>> AddMissingLocations(IEnumerable<DataSheetSamsungSource> sheet)
        {
            //Lokacje które są z requesta
            var receivedLocations = sheet.GroupBy(item => new { Location = item.Location, Siec = item.Siec })
                                              .Select(group => new LocationDTO()
                                              {
                                                  IdCompany = Int32.Parse(group.Key.Siec),
                                                  Address = group.Key.Location
                                              });
            //  
            Console.WriteLine("###### Otrzymane lokacje ###########");
            Console.WriteLine(JsonConvert.SerializeObject(receivedLocations));
            //Lokacje które są już w bazie
            var fromDBLocations = _context.Location.Select(item => new LocationDTO
            {
                Address = item.Address,
                IdCompany = item.IdCompany
            }).ToList();

            Console.WriteLine("********** Lokalizacje z bazy ********");
            Console.WriteLine(JsonConvert.SerializeObject(fromDBLocations));


            var locationExpectThoseInDB = receivedLocations.Except(fromDBLocations, new LocationDTOComparer());

            Console.WriteLine("********** Except lokacje ********");
            Console.WriteLine(JsonConvert.SerializeObject(locationExpectThoseInDB));


            var locToAddv3 = receivedLocations.Where(recLoc => (fromDBLocations.Contains(recLoc)))
                                              .Select(loc => new Location()
                                              {
                                                  IdCompany = loc.IdCompany,
                                                  Address = loc.Address
                                              });


            var locToAddv4 = locationExpectThoseInDB.Select(loc => new Location() { Address = loc.Address, IdCompany = loc.IdCompany });
            Console.WriteLine("!!!!!!!!!!!! Lokalizacje do zapisania! !!!!!!!!!!!!!!!");
            Console.WriteLine(JsonConvert.SerializeObject(locToAddv4));

            // Dodawanie brakujących do bazy.
            await _context.Location.AddRangeAsync(locToAddv4);
            await _context.SaveChangesAsync();

            //Lista wszystkich  po zrobieniu updatu.
            var fromDBLocationsAfterUpdate = _context.Location.Select(item => item)
                                                  .OrderBy(item => item.Address);

            var sheetWithIds = sheet.Select(row => new DataSheetSamsungSource
            {
                Location = fromDBLocationsAfterUpdate.Where(c => c.Address == row.Location)
                                                 .Select(c => c.Id)
                                                 .FirstOrDefault()
                                                 .ToString(),
                Product = row.Product,
                Link = row.Link,
                Siec = row.Siec
            });

            return sheetWithIds;
        }


        private async Task<IEnumerable<DataSheetSamsungSource>> AddMissingProducts(IEnumerable<DataSheetSamsungSource> sheet)
        {
            //Produkty które są z requesta
            var receivedProductsNames = sheet.GroupBy(item => item.Product)
                                              .Select(group => group.FirstOrDefault().Product)
                                              .OrderBy(item => item);

            //Produkty które są już w bazie
            var fromDBProducts = _context.Product.Select(item => item)
                                                  .OrderBy(item => item.Name);

            //Ten obiekt powinien zawierać te nazwy których nie ma w bazie
            var productsToAdd = receivedProductsNames.Where(rcomp => !(fromDBProducts.Select(dbcomp => dbcomp.Name).Contains(rcomp)))
                                                       .Select(newComp => new Product { Name = newComp });

            //Dodawanie brakujących do bazy.
            await _context.Product.AddRangeAsync(productsToAdd);
            await _context.SaveChangesAsync();

            //Lista wszystkich  po zrobieniu updatu.
            var fromDBProductsAfterUpdate = _context.Product.Select(item => item)
                                                  .OrderBy(item => item.Name);

            var sheetWithIds = sheet.Select(row => new DataSheetSamsungSource
            {
                Product = fromDBProductsAfterUpdate.Where(c => c.Name == row.Product)
                                                 .Select(c => c.Id)
                                                 .FirstOrDefault()
                                                 .ToString(),
                Location = row.Location,
                Link = row.Link,
                Siec = row.Siec
            });
            return sheetWithIds;
        }

        private async Task<IEnumerable<DataSheetSamsungSource>> AddMissingLink(IEnumerable<DataSheetSamsungSource> sheet)
        {
            //Produkty które są z requesta
            var receivedLinksNames = sheet.GroupBy(item => item.Link)
                                              .Select(group => group.FirstOrDefault().Link)
                                              .OrderBy(item => item);

            //Produkty które są już w bazie
            var fromDBLinks = _context.Link.Select(item => item)
                                                  .OrderBy(item => item.Url);

            //Ten obiekt powinien zawierać te nazwy których nie ma w bazie
            var linksToAdd = receivedLinksNames.Where(rcomp => !(fromDBLinks.Select(dbcomp => dbcomp.Url).Contains(rcomp)))
                                                       .Select(newComp => new Link { Url = newComp });

            //Dodawanie brakujących do bazy.
            await _context.Link.AddRangeAsync(linksToAdd);
            await _context.SaveChangesAsync();

            //Lista wszystkich  po zrobieniu updatu.
            var fromDBLinksAfterUpdate = _context.Link.Select(item => item)
                                                  .OrderBy(item => item.Url);

            var sheetWithIds = sheet.Select(row => new DataSheetSamsungSource
            {
                Link = fromDBLinksAfterUpdate.Where(c => c.Url == row.Link)
                                                 .Select(c => c.Id)
                                                 .FirstOrDefault()
                                                 .ToString(),
                Location = row.Location,
                Product = row.Product,
                Siec = row.Siec
            });
            return sheetWithIds;
        }



        private async Task<IEnumerable<DataSheetSamsungSource>> AddMissingCompanies(IEnumerable<DataSheetSamsungSource> sheet)
        {
            //Firmy które są z requesta
            var receivedCompaniesNames = sheet.GroupBy(item => item.Siec)
                                              .Select(group => group.FirstOrDefault().Siec)
                                              .OrderBy(item => item);

            //Firmy które są już w bazie
            var fromDBCompanies = _context.Company.Select(item => item)
                                                  .OrderBy(item => item.Name);

            //Ten obiekt powinien zawierać te nazwy których nie ma w bazie
            var companiesToAdd = receivedCompaniesNames.Where(rcomp => !(fromDBCompanies.Select(dbcomp => dbcomp.Name).Contains(rcomp)))
                                                       .Select(newComp => new Company { Name = newComp });

            //Dodawanie brakujących do bazy.
            await _context.Company.AddRangeAsync(companiesToAdd);
            await _context.SaveChangesAsync();

            //Lista wszystkich firm po zrobieniu updatu.
            var fromDBCompaniesAfterUpdate = _context.Company.Select(item => item)
                                                  .OrderBy(item => item.Name);

            var sheetWithIds = sheet.Select(row => new DataSheetSamsungSource
            {
                Siec = fromDBCompaniesAfterUpdate.Where(c => c.Name == row.Siec)
                                                 .Select(c => c.Id)
                                                 .FirstOrDefault()
                                                 .ToString(),
                Location = row.Location,
                Link = row.Link,
                Product = row.Product
            });
            return sheetWithIds;
        }

        // private async Task<IEnumerable<DataSheetSamsungSource>> AddMissing<TEntity>(IEnumerable<DataSheetSamsungSource> sheet) where TEntity : class
        // {
        //     var table = _context.Set<TEntity>();
        //     //Produkty które są z requesta

        //     var receivedTEntityNames = sheet.GroupBy(item => item.GetType()
        //                                                          .GetProperty(nameof(TEntity))
        //                                                          .GetValue(this, null))
        //                                       .Select(group => group.FirstOrDefault()
        //                                                             .GetType()
        //                                                             .GetProperty(nameof(TEntity))
        //                                                             .GetValue(this, null))
        //                                       .OrderBy(item => item);
        //     //Produkty które są już w bazie
        //     var fromDBTEntities = table.Select(item => item)
        //                                .OrderBy(item => item.GetType()
        //                                                     .GetProperty("Name")
        //                                                     .GetValue(this, null));

        //     //Ten obiekt powinien zawierać te nazwy których nie ma w bazie
        //     var TEntityToAdd = receivedTEntityNames.Where(rEnt => !(fromDBTEntities.Select(dbEnt => dbEnt.GetType()
        //                                                                                                  .GetProperty("Name")
        //                                                                                                  .GetValue(this))
        //                                                                                                  .Contains(rEnt)))
        //                                                .Select(newEnt => new { Name = newEnt });

        //     //Dodawanie brakujących do bazy.
        //     Console.WriteLine(TEntityToAdd);

        //     if (!(TEntityToAdd is null))
        //     {
        //         await table.AddRangeAsync((TEntity)TEntityToAdd);
        //         await _context.SaveChangesAsync();
        //     }


        //     //Lista wszystkich firm po zrobieniu updatu.
        //     var fromDBTEntitiesAfterUpdate = table.Select(item => item)
        //                                           .OrderBy(item => item.GetType()
        //                                                                .GetProperty("Name")
        //                                                                .GetValue(this, null));
        //     var sheetWithIds = (nameof(TEntity)) switch
        //     {
        //         "Location" => sheet.Select(row => new DataSheetSamsungSource
        //         {
        //             Location = fromDBTEntitiesAfterUpdate.Where(c => c.GetType()
        //                                                           .GetProperty("Name")
        //                                                           .GetValue(this, null) == row.GetType()
        //                                                                                       .GetProperty(nameof(TEntity))
        //                                                                                       .GetValue(this, null))
        //                                           .Select(c => c.GetType()
        //                                                         .GetProperty("Id")
        //                                                         .GetValue(this, null))
        //                                           .FirstOrDefault()
        //                                           .ToString(),
        //             Link = row.Link,
        //             Product = row.Product,
        //             Siec = row.Siec
        //         }),
        //         "Product" => sheet.Select(row => new DataSheetSamsungSource
        //         {
        //             Product = fromDBTEntitiesAfterUpdate.Where(c => c.GetType()
        //                                                           .GetProperty("Name")
        //                                                           .GetValue(this, null) == row.GetType()
        //                                                                                       .GetProperty(nameof(TEntity))
        //                                                                                       .GetValue(this, null))
        //                                           .Select(c => c.GetType()
        //                                                         .GetProperty("Id")
        //                                                         .GetValue(this, null))
        //                                           .FirstOrDefault()
        //                                           .ToString(),
        //             Location = row.Location,
        //             Link = row.Link,
        //             Siec = row.Siec
        //         }),
        //         "Siec" => sheet.Select(row => new DataSheetSamsungSource
        //         {
        //             Siec = fromDBTEntitiesAfterUpdate.Where(c => c.GetType()
        //                                                           .GetProperty("Name")
        //                                                           .GetValue(this, null) == row.GetType()
        //                                                                                       .GetProperty(nameof(TEntity))
        //                                                                                       .GetValue(this, null))
        //                                           .Select(c => c.GetType()
        //                                                         .GetProperty("Id")
        //                                                         .GetValue(this, null))
        //                                           .FirstOrDefault()
        //                                           .ToString(),
        //             Location = row.Location,
        //             Link = row.Link,
        //             Product = row.Product
        //         }),
        //         "Link" => sheet.Select(row => new DataSheetSamsungSource
        //         {
        //             Link = fromDBTEntitiesAfterUpdate.Where(c => c.GetType()
        //                                                           .GetProperty("Name")
        //                                                           .GetValue(this, null) == row.GetType()
        //                                                                                       .GetProperty(nameof(TEntity))
        //                                                                                       .GetValue(this, null))
        //                                           .Select(c => c.GetType()
        //                                                         .GetProperty("Id")
        //                                                         .GetValue(this, null))
        //                                           .FirstOrDefault()
        //                                           .ToString(),
        //             Location = row.Location,
        //             Product = row.Product,
        //             Siec = row.Siec
        //         }),
        //         _ => throw new Exception("Ups, cos sie zchrzanilo"),
        //     };

        //     return sheetWithIds;

        // var sheetWithIds = sheet.Select(row => new DataSheetSamsungSource
        // {
        //     TEntity = fromDBTEntitiesAfterUpdate.Where(c => c.GetType()
        //                                                   .GetProperty("Name")
        //                                                   .GetValue(this, null) == row.GetType()
        //                                                                               .GetProperty(nameof(TEntity))
        //                                                                               .GetValue(this, null))
        //                                      .Select(c => c.GetType()
        //                                                    .GetProperty("Id")
        //                                                    .GetValue(this, null))
        //                                      .FirstOrDefault()
        //                                      .ToString(),
        //     Location = row.Location,
        //     Link = row.Link,
        //     Product = row.Product
        // });
    }


    // var companiesToAdd = fromDBCompanies.Where(dbComp => receivedCompaniesNames.Contains(dbComp.Name));


    // var oneWithId = sheet.GroupBy(item => item.Siec)
    //                      .Select(group => group.FirstOrDefault().Siec)
    //                      .OrderBy(item => item);

    // // var result = receivedCompanies.Except(fromDBCompanies);

    // var checkAgainstDB = _context.Company.Where(comp => oneWithId.Contains(comp.Name))
    //                                      .Select(x => new { Id = x.Id, Name = x.Name });

    /*
      { 2,3,4 {{1}} 5, 6, 7 }
      Chce mieć Id istniejących firm.
      var A = new List<int>() { 1,2,3,4 };
              var B = new List<int>() { 1, 5, 6, 7 };
      ([xx(xy)]yy ))
      // */
    // var receivedCompanies = file.GroupBy(item => item.Siec)
    // .Select(group => new Company { Name = group.FirstOrDefault().Siec })
    // .OrderBy(item => item.Name);

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

// }

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

// public class Foo
// {
//     public int Id { get; set; }
//     public string Name { get; set; }
// }

//Savoir!128

//Ten obiekt powinien zawierać te Lokacje których nie ma w bazie
//     var locationsToAdd = receivedLocations.Where(recivedLocation => !(fromDBLocations.Contains(receivedLocations)).Select(newLoc =>
// new LocationDTO
// {
//     IdCompany = newLoc.IdCompany,
//     Address = newLoc.Address
// });

//Ten obiekt powinien zawierać te nazwy których nie ma w bazie
// var productsToAdd = receivedProductsNames.Where(rcomp => !(fromDBProducts.Select(dbcomp => dbcomp.Name).Contains(rcomp)))
//                                            .Select(newComp => new Product { Name = newComp });
