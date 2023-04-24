namespace BulgarianJokesMultiClassClassification
{
    using Microsoft.ML.Data;

    public class ModelInput
    {
        [LoadColumn(0)]
        public string Category { get; set; }

        [LoadColumn(1)]
        public string Content { get; set; }
    }
}
