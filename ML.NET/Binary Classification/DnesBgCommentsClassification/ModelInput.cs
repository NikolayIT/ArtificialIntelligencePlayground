namespace DnesBgCommentsClassification
{
    using Microsoft.ML.Data;

    public class ModelInput
    {
        [ColumnName("Content"), LoadColumn(0)]
        public string Content { get; set; }

        [ColumnName("IsPositive"), LoadColumn(1)]
        public bool IsPositive { get; set; }
    }
}
