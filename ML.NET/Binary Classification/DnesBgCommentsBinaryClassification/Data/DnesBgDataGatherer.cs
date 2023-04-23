namespace DnesBgCommentsBinaryClassification.Data
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using AngleSharp.Html.Parser;

    public class DnesBgDataGatherer
    {
        private HtmlParser parser;
        private HttpClient client;

        public DnesBgDataGatherer()
        {
            parser = new HtmlParser();
            client = new HttpClient();
        }

        public async Task GatherAllDataAndSaveToDatabaseAsync(string connectionString, int fromId, int toId)
        {
            var dbContext = new DnesBgCommentsContext(connectionString);
            await dbContext.Database.EnsureCreatedAsync();
            for (var newsId = toId; newsId >= fromId; newsId--)
            {
                var comments = await GatherDataAsync(newsId);
                foreach (var comment in comments)
                {
                    dbContext.DnesBgComments.Add(comment);
                }

                await dbContext.SaveChangesAsync();
                dbContext.ChangeTracker.Clear();
            }
        }

        public async Task<IEnumerable<DnesBgComment>> GatherDataAsync(int newsId)
        {
            var comments = new List<DnesBgComment>();

            Console.Write($"{newsId} => ");
            for (var page = 1; page <= 1000; page++)
            {
                Console.Write('*');
                var url = $"https://www.dnes.bg/category/2010/01/01/news.{newsId},{page}";
                string htmlContent = null;
                for (var i = 0; i < 10; i++)
                {
                    try
                    {
                        var response = await client.GetAsync(url);
                        htmlContent = await response.Content.ReadAsStringAsync();
                        break;
                    }
                    catch
                    {
                        Console.Write('!');
                        Thread.Sleep(500);
                    }
                }

                if (string.IsNullOrWhiteSpace(htmlContent))
                {
                    break;
                }

                var document = await parser.ParseDocumentAsync(htmlContent);

                var commentSection = document.GetElementById("comments-cont");
                if (commentSection == null)
                {
                    Console.Write("Skip. Comments section not found.");
                    break;
                }

                var htmlComments = commentSection.GetElementsByClassName("comment-box");
                if (htmlComments.Length == 0)
                {
                    break;
                }

                foreach (var htmlComment in htmlComments)
                {
                    var contentElement = htmlComment.QuerySelector(".comment_text");
                    if (contentElement == null)
                    {
                        continue;
                    }

                    var divToRemove = contentElement.QuerySelector(".feedback_comment");
                    contentElement.RemoveChild(divToRemove);

                    var positiveVotes = int.Parse(htmlComment.QuerySelector(".comments-grades-up")?.TextContent);
                    var downVotes = int.Parse(htmlComment.QuerySelector(".comments-grades-down")?.TextContent);

                    var comment = new DnesBgComment
                    {
                        Content = contentElement.TextContent.Trim(),
                        UpVotes = positiveVotes,
                        DownVotes = downVotes,
                        NewsId = newsId,
                    };

                    comments.Add(comment);
                }
            }

            Console.WriteLine($"OK. {comments.Count} total comment(s).");
            return comments;
        }
    }
}
