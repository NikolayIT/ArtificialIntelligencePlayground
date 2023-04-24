namespace DnesBgCommentsBinaryClassification
{
    using Microsoft.ML.Data;

    public class ModelOutput : ModelInput
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        public float Probability { get; set; }

        public float Score { get; set; }
    }
}
