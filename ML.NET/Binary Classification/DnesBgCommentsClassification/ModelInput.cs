namespace DnesBgCommentsClassification
{
    using Microsoft.ML.Data;

    public class ModelInput
    {
        [LoadColumn(0)]
        public string Content { get; set; }

        [LoadColumn(1)]
        public bool IsPositive { get; set; }
    }
}
