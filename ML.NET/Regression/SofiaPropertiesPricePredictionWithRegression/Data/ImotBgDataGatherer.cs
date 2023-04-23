namespace SofiaPropertiesPricePredictionWithRegression.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using AngleSharp.Html.Parser;

    public class ImotBgDataGatherer
    {
        private HtmlParser parser;
        private HttpClient client;
        private Regex floorsRegex = new Regex(@"(?<floor>[0-9]+)[^\d]+(?<all>[0-9]+)", RegexOptions.Compiled);
        private Regex typeAndInfoRegex = new Regex(@"^(?<type>[^,]+)([,\s]+(?<year>[0-9]+))?", RegexOptions.Compiled);

        public ImotBgDataGatherer()
        {
            this.parser = new HtmlParser();
            var handler = new HttpClientHandler { AllowAutoRedirect = false, };
            this.client = new HttpClient(handler);
        }

        public async Task GatherAllDataAndSaveToDatabaseAsync(string connectionString, int fromSize, int toSize)
        {
            var dbContext = new SofiaPropertiesContext(connectionString);
            await dbContext.Database.EnsureCreatedAsync();
            for (var size = fromSize; size <= toSize; size++)
            {
                var properties = await GatherDataAsync(size);
                foreach (var property in properties)
                {
                    dbContext.SofiaProperties.Add(property);
                }

                await dbContext.SaveChangesAsync();
                dbContext.ChangeTracker.Clear();
            }
        }

        public async Task<IEnumerable<Property>> GatherDataAsync(int size)
        {
            Console.Write($"Area {size}: ");
            var properties = new List<Property>();

            var formDataApartments =
                $"act=3&rub=1&rub_pub_save=1&topmenu=2&actions=1&f0=127.0.0.1&f1=1&f2=&f3=&f4=1&f7=1%7E2%7E3%7E4%7E5%7E6%7E8%7E&f28=&f29=&f43=&f44=&f30=EUR&f26={size}&f27={size}&f41=1&f31=&f32=&f38=%E3%F0%E0%E4+%D1%EE%F4%E8%FF&f42=&f39=&f40=&fe3=&fe4=&f45=&f46=&f51=&f52=&f33=&f34=&f35=&f36=&f37=&fe2=1";
            var formDataHouses =
                $"act=3&rub=1&rub_pub_save=1&topmenu=2&actions=1&f0=127.0.0.1&f1=1&f2=&f3=&f4=1&f7=10%7E&f28=&f29=&f43=&f44=&f30=EUR&f26={size}&f27={size}&f41=1&f31=&f32=&f54=&f38=%E3%F0%E0%E4+%D1%EE%F4%E8%FF&f42=&f39=&f40=&fe3=&fe4=&f45=&f46=&f51=&f52=&f33=&f34=&f35=&f36=&f37=&fe2=1";

            var response = await client.PostAsync(
                               "https://www.imot.bg/pcgi/imot.cgi",
                               new StringContent(formDataApartments, Encoding.UTF8, "application/x-www-form-urlencoded"));
            var firstPageUrl = response.Headers.Location;

            for (var page = 1; page <= 26; page++)
            {
                var pageUrl = firstPageUrl.ToString().Replace("&f1=1", $"&f1={page}");
                var pageHtml = await GetHtml(pageUrl);
                var pageDocument = await parser.ParseDocumentAsync(pageHtml);
                var listItems = pageDocument.QuerySelectorAll("a.photoLink").Where(
                    x => x.Attributes["href"]?.Value?.Contains("pcgi/imot.cgi?act=5&adv=") == true).ToList();

                if (!listItems.Any())
                {
                    break;
                }

                foreach (var listItem in listItems)
                {
                    var url = "https:" + listItem.Attributes["href"].Value;
                    var html = await GetHtml(url);
                    var document = await parser.ParseDocumentAsync(html);

                    var districtElement = document.QuerySelector(".location");
                    var districtDetails = districtElement.QuerySelector("span");
                    if (districtDetails != null)
                    {
                        districtDetails.Remove();
                    }
                    
                    var floorInfoString = html.GetStringBetween("<div>Етаж: <strong>", "</strong>").Replace("Партер", "1");
                    var floorMatch = floorsRegex.Match(floorInfoString);
                    
                    var typeAndInfoString = html.GetStringBetween("<div>Строителство: <strong>", "</strong>");
                    var typeAndInfoMatch = typeAndInfoRegex.Match(typeAndInfoString);
                    
                    var yardSizeString = html.GetStringBetween("<div>Двор: <strong>", " m<sup>").Trim();
                    int.TryParse(yardSizeString, out var yardSize);
                    
                    var property = new Property
                    {
                        Url = url,
                        Size = size,
                        YardSize = yardSize,
                        District = districtElement?.TextContent?.Trim().Trim(',').Trim(),
                        Type = html.GetStringBetween( "<h1 style=\"margin: 0; font-size:18px;\">", "</h1>")?.Replace("Продава", string.Empty).Trim(),
                        Floor = floorMatch.Success ? floorMatch.Groups["floor"].Value.ToInteger() : 0,
                        TotalFloors = floorMatch.Success ? floorMatch.Groups["all"].Value.ToInteger() : 0,
                        Price = document.QuerySelector("div#cena")?.TextContent?.Replace(" EUR", string.Empty)?.ToInteger() ?? 0,
                        Year = typeAndInfoMatch.Success && typeAndInfoMatch.Groups["year"].Success
                                    ? typeAndInfoMatch.Groups["year"].Value.ToInteger() : 0,
                        BuildingType = typeAndInfoMatch.Success && typeAndInfoMatch.Groups["type"].Success
                                    ? typeAndInfoMatch.Groups["type"].Value : null,
                    };
                    properties.Add(property);
                }

                Console.Write($"{page}({listItems.Count}), ");
            }

            Console.WriteLine($" => Total: {properties.Count}");

            return properties;

            async Task<string> GetHtml(string pageUrl)
            {
                var pageResponse = await client.GetAsync(pageUrl);
                var byteContent = await pageResponse.Content.ReadAsByteArrayAsync();
                var html = Encoding.GetEncoding("windows-1251").GetString(byteContent);
                return html;
            }
        }
    }
}
