namespace DelaBgCourtDocumentsByDateAnomalyDetection
{
    using Microsoft.ML.Data;

    public class PredictionResult
    {
        /// <summary>
        /// Gets or sets a vector containing 4 elements:
        /// - IsAnomaly: A binary value (0 or 1) indicating whether the data point is considered an anomaly or not.
        /// - AnomalyScore: Higher values generally indicate more severe anomalies.
        /// - Magnitude: A numerical value that represents the magnitude or size of the detected spike.
        /// - ExpectedValue: The expected or predicted value of the data point based on the model or algorithm used for spike detection.
        /// </summary>
        [VectorType(4)]
        public double[] Prediction { get; set; }
    }
}
