namespace SofiaPropertiesPricePrediction
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
        public async Task<IEnumerable<RawProperty>> GatherData(int fromSize, int toSize)
        {
            var properties = new List<RawProperty>();

            var parser = new HtmlParser();
            var handler = new HttpClientHandler { AllowAutoRedirect = false, };
            var client = new HttpClient(handler);
            var floorsRegex = new Regex(@"(?<floor>[0-9]+)[^\d]+(?<all>[0-9]+)", RegexOptions.Compiled);
            var typeAndInfoRegex = new Regex(@"^(?<type>[^,]+)([,\s]+(?<year>[0-9]+))?", RegexOptions.Compiled);

            // 10 => 1000
            for (var size = fromSize; size <= toSize; size++)
            {
                Console.Write($"Area {size}: ");

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
                        var district = html.GetStringBetween(
                            "<span style=\"font-size:14px; margin:8px 0; display:inline-block\">",
                            "</span>")?.Trim();
                        if (district?.Contains("<br>") == true)
                        {
                            var indexOfBr = district.IndexOf("<br>", StringComparison.InvariantCulture);
                            district = district.Substring(0, indexOfBr).Trim();
                        }

                        var floorInfoString = html.GetStringBetween(
                            "<li>Етаж:</li><li>",
                            "</li>").Replace("Партер", "1");
                        var floorMatch = floorsRegex.Match(floorInfoString);
                        var typeAndInfoString = html.GetStringBetween(
                            "<li>Строителство:</li><li>",
                            "</li>");
                        var typeAndInfoMatch = typeAndInfoRegex.Match(typeAndInfoString);
                        var yardSizeString = html.GetStringBetween(
                            "<li>Двор:</li><li>",
                            "</li>").Replace(" кв.м", string.Empty).Trim();
                        int.TryParse(yardSizeString, out var yardSize);
                        var property = new RawProperty
                                       {
                                           Url = url,
                                           Size = size,
                                           YardSize = yardSize,
                                           District = district,
                                           Type =
                                               html.GetStringBetween(
                                                   "<h1 style=\"margin: 0; font-size:18px;\">",
                                                   "</h1>")?.Replace("Продава", string.Empty).Trim(),
                                           Floor =
                                               floorMatch.Success ? floorMatch.Groups["floor"].Value.ToInteger() : 0,
                                           TotalFloors =
                                               floorMatch.Success ? floorMatch.Groups["all"].Value.ToInteger() : 0,
                                           Price =
                                               document.QuerySelector("span#cena")?.TextContent
                                                   ?.Replace(" EUR", string.Empty)?.ToInteger() ?? 0,
                                           Year = typeAndInfoMatch.Success && typeAndInfoMatch.Groups["year"].Success
                                                      ? typeAndInfoMatch.Groups["year"].Value.ToInteger()
                                                      : 0,
                                           BuildingType =
                                               typeAndInfoMatch.Success && typeAndInfoMatch.Groups["type"].Success
                                                   ? typeAndInfoMatch.Groups["type"].Value
                                                   : null,
                                       };
                        properties.Add(property);
                    }

                    Console.Write($"{page}({listItems.Count}), ");
                }

                Console.WriteLine($" => Total: {properties.Count}");
            }

            return properties;

            async Task<string> GetHtml(string pageUrl)
            {
                var pageResponse = await client.GetAsync(pageUrl);
                var byteContent = await pageResponse.Content.ReadAsByteArrayAsync();
                var html = Encoding.GetEncoding("windows-1251").GetString(byteContent);
                return html;
            }
        }

        public class RawProperty
        {
            public string Url { get; set; }

            public int Size { get; set; }

            public int YardSize { get; set; }

            public int Floor { get; set; }

            public int TotalFloors { get; set; }

            public string District { get; set; }

            public int Year { get; set; }

            public string Type { get; set; }

            public string BuildingType { get; set; }

            public int Price { get; set; }
        }
    }
}
