namespace BulgarianJokesClassification
{
    using Microsoft.ML.Data;

    public class JokeModel
    {
        [LoadColumn(1)]
        public string Category { get; set; }

        [LoadColumn(2)]
        public string Content { get; set; }
    }
}
