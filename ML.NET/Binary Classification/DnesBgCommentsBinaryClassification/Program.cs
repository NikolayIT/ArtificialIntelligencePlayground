namespace DnesBgCommentsBinaryClassification
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using DnesBgCommentsBinaryClassification.Data;

    using Microsoft.ML;

    /*
     * Source: https://dnes.bg/
     * // Id [32-567341] - all news from 2005-01-01 to 2023-04-21, inclusive.
     * await new DnesBgDataGatherer().GatherAllDataAndSaveToDatabaseAsync("Server=.;Database=MlNetDataComments;Integrated Security=True;TrustServerCertificate=True", 32, 567341);
     *
     * Negative comments:
     * WHERE [DownVotes] >= 15 AND (1.0 * [UpVotes]) / [DownVotes] <= 0.2   -- down >= 5 * up
     *
     * Positive comments:
     * WHERE [UpVotes] >= 30 AND (1.0 * [DownVotes]) / [UpVotes] <= 0.05   -- up >= 20 * down
     */
    public static class Program
    {
        public static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            var dataFile = @"Data\dnes.bg-2023-04-23.csv";
            var modelFile = "DnesBgCommentsModel.zip";
            if (!File.Exists(modelFile))
            {
                Console.Write("Training the model... ");
                TrainModel(dataFile, modelFile);
                Console.WriteLine("Done!");
            }

            var testModelData = new List<string>
            {
                // 18 up, 2 down
                "Я как хубаво върви угояването на депутатите, то и затова са решили за пореден път да си вдигнат заплатите... да си угаждат повече.",
                // 0 up, 13 down
                "Победа!",
                // Not real comments:
                "Русия, САЩ, Путин, Байдън",
                "Програмистите играят игри по цял ден и взимат големи заплати...",
                "Комунистите прецакаха държавата",
            };

            TestModel(modelFile, testModelData);
        }


        public static void TrainModel(string dataFile, string modelFile)
        {
            var context = new MLContext();
            IDataView dataView = context.Data.LoadFromTextFile<ModelInput>(
                dataFile,
                hasHeader: true,
                separatorChar: ',',
                allowQuoting: true);
            var estimator = context.Transforms.Text.FeaturizeText(outputColumnName: "Features", inputColumnName: nameof(ModelInput.SentimentText))
                .Append(context.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features"));
            var model = estimator.Fit(dataView);
            context.Model.Save(model, dataView.Schema, modelFile);
        }

        private static void TestModel(string modelFile, IEnumerable<string> testModelData)
        {
            var context = new MLContext();
            var model = context.Model.Load(modelFile, out _);
            var predictionEngine = context.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
            foreach (var testData in testModelData)
            {
                var prediction = predictionEngine.Predict(new ModelInput { SentimentText = testData });
                Console.WriteLine(new string('-', 60));
                Console.WriteLine($"Content: {testData}");
                Console.WriteLine($"Is positive? {prediction.Prediction}");
                Console.WriteLine($"Score: {prediction.Score}");
            }
        }
    }
}

/*
SELECT Content AS SentimentText,
	CAST(CASE
		WHEN [UpVotes] >= 30 AND (1.0 * [DownVotes]) / [UpVotes] <= 0.05 THEN 1
		WHEN [DownVotes] >= 15 AND (1.0 * [UpVotes]) / [DownVotes] <= 0.2 THEN 0
        ELSE -1000
    END AS int) as Sentiment
FROM [MlNetData].[dbo].[DnesBgComments]
WHERE ([UpVotes] >= 30 AND (1.0 * [DownVotes]) / NULLIF([UpVotes], 0) <= 0.05) 
	OR ([DownVotes] >= 15 AND (1.0 * [UpVotes]) / NULLIF([DownVotes], 0) <= 0.2)
*/