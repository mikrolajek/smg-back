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
using Microsoft.AspNetCore.Http;

namespace WebApiRedirector.Controllers
{
    [ApiController]
    [Route("")]
    public class ApiController : ControllerBase
    {
        private readonly postgresContext _context;
        private readonly IHttpContextAccessor _accessor;

        public ApiController(postgresContext context, IHttpContextAccessor accessor)
        {
            _context = context;
            _accessor = accessor;
        }

        // [HttpGet("{type?}/{uqCode?}")]
        [HttpGet("{uqCode?}")]
        public async Task<ActionResult> Get(string uqCode)
        {
            string phoneType;
            string acceptLanguage;

            var codeInDb = await _context.Code.FirstOrDefaultAsync(c => c.Uid == uqCode);
            var groupWithCode = _context.Group.Where(gr => gr.IdCodeNavigation.Uid == uqCode);
            var codeInDbGroup = await groupWithCode.Select(gr => gr.IdLinkNavigation.Url).FirstOrDefaultAsync();
            if (!(codeInDbGroup is null))
            {
                var ip = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
                var headers = _accessor.HttpContext.Request.Headers;
                var userAgent = _accessor.HttpContext.Request.Headers["User-Agent"];
                var userAgentString = JsonConvert.SerializeObject(headers);
                acceptLanguage = _accessor.HttpContext.Request.Headers["Accept-language"].ToString();
                var isIphone = userAgentString.ToLower().Contains("iphone");
                var isAndroid = userAgentString.ToLower().Contains("android");

                phoneType = isIphone ? "iphone" : isAndroid ? "android" : "unknown";

                var logDto = new LogDto()
                {
                    DeviceType = phoneType,
                    Ip = ip,
                    UserAgent = userAgent,
                    AcceptLanguage = acceptLanguage,
                    IdGroup = groupWithCode.Select(gr => gr.Id).FirstOrDefault()
                };

                var logToSave = new Log()
                {
                    Ip = logDto.Ip,
                    DeviceType = logDto.DeviceType,
                    UserAgent = logDto.UserAgent,
                    AcceptLanguage = logDto.AcceptLanguage,
                    IdGroup = logDto.IdGroup
                };

                _context.Log.Add(logToSave);
                await _context.SaveChangesAsync();
                // groupWithCode.Select(g => g.)
                return Redirect(codeInDbGroup);
            }

            // return Content(phoneType);
            return Content($"Nie ma takiego zasobu :ACCESSOR {_accessor.HttpContext.Connection.RemoteIpAddress}");
        }

        [HttpPost("/add/sheet/{haslo}")]
        public async Task<ActionResult<IEnumerable<DataSheetSamsungSource>>> AddSheet([FromBody] IEnumerable<DataSheetSamsungSource> sheet, string haslo)
        {
            if (haslo != "saVGRouPPasS563") return (Unauthorized());
            // var companiesReplacedWithId = await AddMissingCompanies(sheet);
            var sheetCompaniesWithId = await AddMissingCompanies(sheet);
            var sheetProductsWithId = await AddMissingProducts(sheetCompaniesWithId);
            var sheetLinkWithId = await AddMissingLink(sheetProductsWithId);
            var sheetAddressesId = await AddMissingLocations(sheetLinkWithId);
            var groupWithAllTHingsAdded = await AddMissingGroup(sheetAddressesId);
            return Ok(groupWithAllTHingsAdded.Select(item => new
            {
                Id = item.Id,
                IdCode = item.IdCode,
                IdLocation = item.IdLocation,
                IdProduct = item.IdProduct,
                IdLink = item.IdLink,
            }).ToList());
        }

        private async Task<IEnumerable<Group>> AddMissingGroup(IEnumerable<DataSheetSamsungSource> sheet)
        {
            var receivedGroups = sheet.Select(group => new GroupDTO()
            {
                Link = Int32.Parse(group.Link),
                Location = Int32.Parse(group.Location),
                Product = Int32.Parse(group.Product)
            });

            //Grupy które są już w bazie bez timestampów i przypisanym ich kodom
            var fromDBGroupsDTO = _context.Group.Select(item => new GroupDTO
            {
                Link = item.IdLink,
                Location = item.IdLocation,
                Product = item.IdProduct
            }).ToList();

            Console.WriteLine("********** Grupy z bazy ********");
            Console.WriteLine(JsonConvert.SerializeObject(fromDBGroupsDTO));


            var groupsExpectThoseInDB = receivedGroups.Except(fromDBGroupsDTO, new GroupDTOComparer());

            Console.WriteLine("********** Except Grupy ********");
            Console.WriteLine(JsonConvert.SerializeObject(groupsExpectThoseInDB));

            // var qrCode = (Codetype.QR);
            // var nfcCode = GenerateCode();
            var DateTimeddd = DateTime.Now;
            var codeToAddv1 = groupsExpectThoseInDB.SelectMany(gr =>
            {
                var qrCode = GenerateCode(Codetype.QR).Id;
                var nfcCode = GenerateCode(Codetype.NFC).Id;
                var pair = $"QR: {qrCode}; NFC: {nfcCode}";
                var pairId = AddPairTracker(qrCode, nfcCode);
                return new Group[] {
                    new Group()
                    {
                        IdCode = qrCode,
                        IdLocation = gr.Location,
                        IdLink = gr.Link,
                        IdProduct = gr.Product,
                        FromDate = DateTime.Now,
                        IdPairTracker = pairId
                    },
                    new Group()
                    {
                        IdCode = nfcCode,
                        IdLocation = gr.Location,
                        IdLink = gr.Link,
                        IdProduct = gr.Product,
                        FromDate = DateTime.Now,
                        IdPairTracker = pairId
                    }
                    };

            }).ToList();

            Console.WriteLine("!!!!!!!!!!!! Grupy do zapisania! !!!!!!!!!!!!!!!");
            Console.WriteLine(JsonConvert.SerializeObject(codeToAddv1.Select(item => new
            {
                Id = item.Id,
                IdCode = item.IdCode,
                IdLocation = item.IdLocation,
                IdProduct = item.IdProduct,
                IdLink = item.IdLink,
            }).ToList()));

            // Dodawanie brakujących do bazy.
            await _context.Group.AddRangeAsync(codeToAddv1);
            await _context.SaveChangesAsync();

            //Lista wszystkich  po zrobieniu updatu.
            var fromDBGroupsAfterUpdate = _context.Group.Select(item => item);

            return fromDBGroupsAfterUpdate;

            // var sheetWithIds = sheet.Select(row => new DataSheetSamsungSource
            // {
            //     Location = fromDBGroupsAfterUpdate.Where(c => c.Address == row.Location)
            //                                      .Select(c => c.Id)
            //                                      .FirstOrDefault()
            //                                      .ToString(),
            //     Product = row.Product,
            //     Link = row.Link,
            //     Company = row.Company
            // });
        }

        private int AddPairTracker(int qrid, int nfcid)
        {

            var pairToAdd = new PairTracker()
            {
                IdCode1 = qrid,
                IdCode2 = nfcid
            };
            _context.PairTracker.Add(pairToAdd);
            _context.SaveChanges();
            return pairToAdd.Id;

        }
        private Code GenerateCode(string type)
        {
            if (type == Codetype.QR | type == Codetype.NFC)
            {
                var uid = $"{type.Substring(0, 1)}{RandomString.Generate(7)}";
                var codeToAdd = new Code()
                {
                    Taken = true,
                    Type = type,
                    Uid = uid
                };
                _context.Code.Add(codeToAdd);
                _context.SaveChanges();
                return codeToAdd;
            }
            throw new Exception("cos nie dzial z dodawaniem kodu, najprawdopodobniej zly input");
        }


        private async Task<IEnumerable<DataSheetSamsungSource>> AddMissingLocations(IEnumerable<DataSheetSamsungSource> sheet)
        {
            //Lokacje które są z requesta
            var receivedLocations = sheet.GroupBy(item => new { Location = item.Location, Company = item.Company })
                                              .Select(group => new LocationDTO()
                                              {
                                                  IdCompany = Int32.Parse(group.Key.Company),
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
                Company = row.Company
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
                Company = row.Company
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
                Company = row.Company
            });
            return sheetWithIds;
        }



        private async Task<IEnumerable<DataSheetSamsungSource>> AddMissingCompanies(IEnumerable<DataSheetSamsungSource> sheet)
        {
            //Firmy które są z requesta
            var receivedCompaniesNames = sheet.GroupBy(item => item.Company)
                                              .Select(group => group.FirstOrDefault().Company)
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
                Company = fromDBCompaniesAfterUpdate.Where(c => c.Name == row.Company)
                                                 .Select(c => c.Id)
                                                 .FirstOrDefault()
                                                 .ToString(),
                Location = row.Location,
                Link = row.Link,
                Product = row.Product
            });
            return sheetWithIds;
        }
    }
}

/*
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
//             Company = row.Company
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
//             Company = row.Company
//         }),
//         "Company" => sheet.Select(row => new DataSheetSamsungSource
//         {
//             Company = fromDBTEntitiesAfterUpdate.Where(c => c.GetType()
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
//             Company = row.Company
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



// var companiesToAdd = fromDBCompanies.Where(dbComp => receivedCompaniesNames.Contains(dbComp.Name));


// var oneWithId = sheet.GroupBy(item => item.Company)
//                      .Select(group => group.FirstOrDefault().Company)
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
// var receivedCompanies = file.GroupBy(item => item.Company)
// .Select(group => new Company { Name = group.FirstOrDefault().Company })
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
// _context
//     public string Name { get; set; }
// }

//Savoir!128

//Ten obiekt powinien zawierać te Lokacje których nie ma w bazie
//     var locationsToAdd = receivedGroups.Where(recivedLocation => !(fromDBLocations.Contains(receivedGroups)).Select(newLoc =>
// new LocationDTO
// {
//     IdCompany = newLoc.IdCompany,
//     Address = newLoc.Address
// });

//Ten obiekt powinien zawierać te nazwy których nie ma w bazie
// var productsToAdd = receivedProductsNames.Where(rcomp => !(fromDBProducts.Select(dbcomp => dbcomp.Name).Contains(rcomp)))
//                                            .Select(newComp => new Product { Name = newComp });
