namespace DnesBgCommentsBinaryClassification
{
    using Microsoft.ML.Data;

    public class SentimentData
    {
        [LoadColumn(0)]
        public string SentimentText;

        [LoadColumn(1), ColumnName("Label")]
        public bool Sentiment;
    }
}
