namespace DnesBgCommentsClassification
{
    using Microsoft.ML.Data;

    public class ModelOutput
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        public float Score { get; set; }
    }
}
