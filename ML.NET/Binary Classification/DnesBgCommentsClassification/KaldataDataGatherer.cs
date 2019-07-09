namespace DnesBgCommentsClassification
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using AngleSharp.Html.Parser;

    public class KaldataDataGatherer
    {
        public async Task<IEnumerable<RawComment>> GatherData(int fromNewsId, int toNewsId)
        {
            var comments = new List<RawComment>();
            var parser = new HtmlParser();
            var client = new HttpClient();

            for (var newsId = toNewsId; newsId >= fromNewsId; newsId--)
            {
                Console.Write($"{newsId} => ");
                var url = $"https://www.kaldata.com/category/news-{newsId}.html";

                string htmlContent = null;
                for (var i = 0; i < 10; i++)
                {
                    try
                    {
                        var response = await client.GetAsync(url);
                        if (!response.Content.Headers.GetValues("Content-Type").Contains("text/html; charset=UTF-8"))
                        {
                            Console.WriteLine("Skip. Not HTML.");
                            break;
                        }

                        htmlContent = await response.Content.ReadAsStringAsync();
                        break;
                    }
                    catch
                    {
                        Console.Write("!");
                        Thread.Sleep(500);
                    }
                }

                if (htmlContent == null)
                {
                    continue;
                }

                var document = await parser.ParseDocumentAsync(htmlContent);

                var commentSection = document.GetElementById("wcThreadWrapper");
                if (commentSection == null)
                {
                    Console.WriteLine("Skip. Comments section not found.");
                    continue;
                }

                var htmlComments = commentSection.GetElementsByClassName("wc-comment-right");
                foreach (var htmlComment in htmlComments)
                {
                    var content = htmlComment.QuerySelector(".wc-comment-text")?.TextContent;
                    var positiveVotes = int.Parse(htmlComment.QuerySelector(".wc-vote-result-like")?.TextContent);
                    var downVotes = -int.Parse(htmlComment.QuerySelector(".wc-vote-result-dislike")?.TextContent);
                    var createdOn = DateTime.ParseExact(
                        htmlComment.QuerySelector(".wc-comment-date")?.TextContent,
                        "dd.MM.yyyy, H:mm",
                        CultureInfo.InvariantCulture);

                    var comment = new RawComment
                    {
                        Content = content,
                        UpVotes = positiveVotes,
                        DownVotes = downVotes,
                        NewsId = newsId,
                        CreatedOn = createdOn,
                    };

                    comments.Add(comment);
                }

                Console.WriteLine($"OK. {htmlComments.Length} comment(s).");
            }

            return comments;
        }

        public class RawComment
        {
            public int NewsId { get; set; }

            public string Content { get; set; }

            public int UpVotes { get; set; }

            public int DownVotes { get; set; }

            public DateTime CreatedOn { get; set; }
        }
    }
}
