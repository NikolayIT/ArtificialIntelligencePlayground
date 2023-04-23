namespace BulgarianJokesMultiClassClassification.Data
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using AngleSharp.Html.Parser;

    public class FunDirBgDataGatherer
    {
        private HtmlParser parser;
        private HttpClient httpClient;

        public FunDirBgDataGatherer()
        {
            this.parser = new HtmlParser();
            this.httpClient = new HttpClient();
        }

        public async Task GatherAllDataAndSaveToDatabaseAsync(string connectionString, int fromId, int toId)
        {
            var dbContext = new BulgarianJokesContext(connectionString);
            await dbContext.Database.EnsureCreatedAsync();
            for (var jokeId = toId; jokeId >= fromId; jokeId--)
            {
                var joke = await GatherDataAsync(jokeId);
                if (joke == null)
                {
                    continue;
                }

                dbContext.BulgarianJokes.Add(joke);
                await dbContext.SaveChangesAsync();
                dbContext.ChangeTracker.Clear();
                Console.WriteLine($"{joke.ExternalId} => {joke.Category}");
            }
        }

        public async Task<Joke> GatherDataAsync(int id)
        {
            var url = $"https://fun.dir.bg/fun/{id}/razni";
            string html = await this.ReadAsStringAsync(url);
            if (string.IsNullOrWhiteSpace(html) || html.Contains("Страницата, която търсите, не съществува"))
            {
                return null;
            }

            var document = parser.ParseDocument(html);
            var jokeContent = document.QuerySelector(".single_joke .joke_text")?.TextContent?.Trim();
            var categoryName = document.QuerySelector("#joke_heading h2 a")?.TextContent?.Trim();

            if (!string.IsNullOrWhiteSpace(jokeContent) &&
                !string.IsNullOrWhiteSpace(categoryName))
            {
                var joke = new Joke { Category = categoryName, Content = jokeContent, ExternalId = id };
                return joke;
            }

            return null;
        }

        private async Task<string> ReadAsStringAsync(string url)
        {
            for (var j = 0; j < 10; j++)
            {
                try
                {
                    HttpResponseMessage response = await this.httpClient.GetAsync(url);
                    return await response.Content.ReadAsStringAsync();
                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                }
            }

            return null;
        }
    }
}
