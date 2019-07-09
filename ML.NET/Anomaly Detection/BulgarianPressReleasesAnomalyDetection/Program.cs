namespace BulgarianPressReleasesAnomalyDetection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.ML;
    using Microsoft.ML.Data;

    using PLplot;

    public class Program
    {
        private const string ModelPath = "PressCentersAnomalyModel.zip";

        public static void Main()
        {
            /*
             * Source: https://presscenters.com/
             */

            var context = new MLContext(seed: 0);

            // load data
            var dataView = context.Data.LoadFromTextFile<DayInfo>(
                "presscenters.com.csv",
                separatorChar: ',',
                hasHeader: true);

            // transform options
            BuildTrainModel(context, dataView);

            var anomalies = DetectAnomalies(context, dataView);

            var days = context.Data.CreateEnumerable<DayInfo>(dataView, reuseRowObject: false).ToArray();
            DrawPlot(days, anomalies.ToList());
        }

        private static void BuildTrainModel(MLContext context, IDataView dataView)
        {
            // Configure the Estimator
            var trainingPipeLine = context.Transforms.DetectSpikeBySsa(
                outputColumnName: nameof(SpikePrediction.Prediction),
                inputColumnName: nameof(DayInfo.Count),
                confidence: 95,
                pvalueHistoryLength: 21,
                trainingWindowSize: 730,
                seasonalityWindowSize: 7);

            ITransformer trainedModel = trainingPipeLine.Fit(dataView);

            context.Model.Save(trainedModel, dataView.Schema, ModelPath);

            Console.WriteLine("The model is saved to {0}", ModelPath);
        }

        private static IEnumerable<DayInfo> DetectAnomalies(MLContext context, IDataView dataView)
        {
            ITransformer trainedModel = context.Model.Load(ModelPath, out _);

            var transformedData = trainedModel.Transform(dataView);

            // Getting the data of the newly created column as an IEnumerable
            IEnumerable<SpikePrediction> predictions =
                context.Data.CreateEnumerable<SpikePrediction>(transformedData, false);

            var columnCount = dataView.GetColumn<float>(nameof(DayInfo.Count)).ToArray();
            var columnDate = dataView.GetColumn<DateTime>(nameof(DayInfo.Date)).ToArray();

            // Output the input data and predictions
            Console.WriteLine("======Displaying anomalies in the PressCenters.com data=========");
            Console.WriteLine("Date                          \tCount\tAlert\tScore\tP-Value");

            var anomalies = new List<DayInfo>();

            int i = 0;
            foreach (var p in predictions)
            {
                if (p.Prediction[0] > 0)
                {
                    anomalies.Add(new DayInfo { Date = columnDate[i], Count = columnCount[i] });
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                }

                // if (p.Prediction[0] > 0)
                {
                    Console.WriteLine(
                        "{0}\t{1:0}\t{2:0.00}\t{3:0.00}\t{4:0.00}",
                        columnDate[i].ToLongDateString().PadRight(25),
                        columnCount[i],
                        p.Prediction[0],
                        p.Prediction[1],
                        p.Prediction[2]);
                    Console.ResetColor();
                }

                i++;
            }

            return anomalies;
        }

        private static void DrawPlot(IList<DayInfo> days, IList<DayInfo> anomalies)
        {
            days = days.Where(x => x.Date >= new DateTime(2017, 9, 1) && x.Date <= new DateTime(2017, 9, 30)).ToList();
            anomalies = anomalies.Where(x => x.Date >= new DateTime(2017, 9, 1) && x.Date <= new DateTime(2017, 9, 30)).ToList();

            using (var plot = new PLStream())
            {
                plot.sdev("pngcairo"); // png rendering
                plot.sfnam("data.png"); // output filename
                plot.spal0("cmap0_alternate.pal"); // alternate color palette
                plot.init();
                plot.env(
                    1, // x-axis range
                    days.Count,
                    0, // y-axis range
                    150,
                    AxesScale.Independent, // scale x and y independently
                    AxisBox.BoxTicksLabelsAxes); // draw box, ticks, and num ticks
                plot.lab(
                    "Date", // x-axis label
                    "Count", // y-axis label
                    "Press releases September 2017"); // plot title
                plot.line(
                    (from x in Enumerable.Range(1, days.Count) select (double)x).ToArray(),
                    (from p in days select (double)p.Count).ToArray());

                // plot the spikes
                plot.col0(2);     // blue color
                plot.schr(3, 3);  // scale characters
                plot.string2(
                    (from s in anomalies select (double)days.ToList().FindIndex(x => x.Date == s.Date) + 1).ToArray(),
                    (from s in anomalies select (double)s.Count + 15).ToArray(),
                    "↓");
                plot.eop();
            }
        }
    }
}
