namespace BulgarianPressReleasesAnomalyDetection
{
    using Microsoft.ML.Data;

    public class SpikePrediction
    {
        /// <summary>
        /// Gets or sets the vector containing 3 elements:
        /// - alert (non-zero value means a spike)
        /// - raw score
        /// - p-value
        /// </summary>
        [VectorType(3)]
        public double[] Prediction { get; set; }
    }
}
