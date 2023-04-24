namespace BulgarianJokesMultiClassClassification
{
    using Microsoft.ML.Data;

    public class ModelOutput
    {
        [ColumnName("PredictedLabel")]
        public string Category { get; set; }

        public float[] Score { get; set; }
    }
}
