namespace BulgarianJokesClassification
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.Threading;

    using AngleSharp.Html.Parser;

    public class DataGatherer
    {
        public IEnumerable<JokeModel> GatherData(int fromId, int toId)
        {
            var jokes = new List<JokeModel>();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var parser = new HtmlParser();
            var webClient = new WebClient { Encoding = Encoding.GetEncoding("windows-1251") };

            for (var i = fromId; i <= toId; i++)
            {
                var url = "http://fun.dir.bg/vic_open.php?id=" + i;
                string html = null;
                for (var j = 0; j < 10; j++)
                {
                    try
                    {
                        html = webClient.DownloadString(url);
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(1000);
                    }
                }

                if (string.IsNullOrWhiteSpace(html))
                {
                    continue;
                }

                var document = parser.ParseDocument(html);
                var jokeContent = document.QuerySelector("#newsbody")?.TextContent?.Trim();
                var categoryName = document.QuerySelector(".tag-links-left a")?.TextContent?.Trim();

                if (!string.IsNullOrWhiteSpace(jokeContent) &&
                    !string.IsNullOrWhiteSpace(categoryName))
                {
                    var jokeModel = new JokeModel { Category = categoryName, Content = jokeContent };
                    jokes.Add(jokeModel);
                }

                Console.WriteLine($"{i} => {categoryName}");
            }

            return jokes;
        }
    }
}
