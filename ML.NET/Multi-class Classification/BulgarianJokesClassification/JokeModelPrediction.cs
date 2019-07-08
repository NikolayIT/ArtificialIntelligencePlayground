namespace BulgarianJokesClassification
{
    using Microsoft.ML.Data;

    public class JokeModelPrediction
    {
        [ColumnName("PredictedLabel")]
        public string Category { get; set; }

        public float[] Score { get; set; }
    }
}
