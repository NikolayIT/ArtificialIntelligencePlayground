namespace BulgarianJokesMultiClassClassification.Data
{
    using Microsoft.ML.Data;

    public class Joke
    {
        public int Id { get; set; }

        public string Category { get; set; }

        public string Content { get; set; }

        public int ExternalId { get; set; }
    }
}
