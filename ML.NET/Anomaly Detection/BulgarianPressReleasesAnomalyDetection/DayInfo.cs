namespace BulgarianPressReleasesAnomalyDetection
{
    using System;

    using Microsoft.ML.Data;

    public class DayInfo
    {
        [LoadColumn(0)]
        public DateTime Date { get; set; }

        [LoadColumn(1)]
        public float Count { get; set; }
    }
}
