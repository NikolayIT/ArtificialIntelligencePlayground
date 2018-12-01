namespace mltest
{
    using Microsoft.ML.Runtime.Api;
    using Microsoft.ML.Runtime.Data;

    public class JokeModelPrediction
    {
        [ColumnName(DefaultColumnNames.PredictedLabel)]
        public string Category { get; set; }
    }
}
