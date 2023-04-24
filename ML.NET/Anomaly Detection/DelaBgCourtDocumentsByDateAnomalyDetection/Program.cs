namespace DelaBgCourtDocumentsByDateAnomalyDetection
{
    using System;
    using System.Linq;

    using Microsoft.ML;
    using Microsoft.ML.Data;
    using Microsoft.ML.TimeSeries;

    /*
     * Source: https://dela.bg/
     */
    public static class Program
    {
        public static void Main()
        {
            // Create MLContext to be shared across the model creation workflow objects
            MLContext mlContext = new MLContext();

            // Load the data
            IDataView dataView = mlContext.Data.LoadFromTextFile<ModelInput>(
                path: "dela.bg documents count.csv",
                hasHeader: true,
                separatorChar: ',');

            // Detect period on the given series.
            int period = mlContext.AnomalyDetection.DetectSeasonality
                (dataView,
                nameof(ModelInput.Count));
            Console.WriteLine("Period of the series is: {0}.", period);

            // Setup the parameters
            var options = new SrCnnEntireAnomalyDetectorOptions()
            {
                Threshold = 0.19,
                Sensitivity = 64.0,
                DetectMode = SrCnnDetectMode.AnomalyAndExpectedValue,
                Period = period,
            };

            // Invoke SrCnn algorithm to detect anomaly on the entire series.
            var outputDataView = mlContext.AnomalyDetection.DetectEntireAnomalyBySrCnn(
                dataView,
                nameof(ModelOutput.Prediction),
                nameof(ModelInput.Count),
                options);

            // Get the detection results as an IEnumerable
            var predictions = mlContext.Data.CreateEnumerable<ModelOutput>(
                outputDataView, reuseRowObject: false);

            // Print out the detection results
            int index = 0;
            var columnCount = dataView.GetColumn<double>(nameof(ModelInput.Count)).ToArray();
            var columnDate = dataView.GetColumn<DateTime>(nameof(ModelInput.DocumentDate)).ToArray();
            foreach (var prediction in predictions)
            {
                if (prediction.Prediction[0] == 1)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{columnDate[index]:yyyy-MM-dd},\t{columnCount[index]},\t{prediction.Prediction[1]:0.00},\t{prediction.Prediction[2]:0.00},\t{prediction.Prediction[3]:0} <-- anomaly detected");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"{columnDate[index]:yyyy-MM-dd},\t{columnCount[index]},\t{prediction.Prediction[1]:0.00},\t{prediction.Prediction[2]:0.00},\t{prediction.Prediction[3]:0}");
                }

                index++;
            }
        }
    }
}
