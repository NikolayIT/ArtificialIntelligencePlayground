namespace SofiaPropertiesPricePrediction
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    using CsvHelper;

    using Microsoft.ML;
    using Microsoft.ML.Trainers.LightGbm;

    using Newtonsoft.Json;

    public static class Program
    {
        public static void Main()
        {
            /*
             * Source: https://www.imot.bg/
             * var properties = new ImotBgDataGatherer().GatherData(1, 1000).GetAwaiter().GetResult();
             *
             * using var csvWriter = new CsvWriter(new StreamWriter(File.OpenWrite($"imot.bg-raw-data-{DateTime.Now:yyyy-MM-dd}.csv"), Encoding.UTF8), CultureInfo.CurrentCulture);
             * csvWriter.WriteRecords(properties);
             *
             * File.WriteAllText(
                $"imot.bg-raw-data-{DateTime.Now:yyyy-MM-dd}.json",
                JsonConvert.SerializeObject(properties));
             *
             * 24228 records in imot.bg-raw-data-2019-07-06.csv
             *
             * WHERE [Price] > 0 AND [Year] > 0 AND [Floor] > 0 AND [TotalFloors] > 0
             *
             * 16083 records in imot.bg-2019-07-06.csv
             */

            Console.OutputEncoding = Encoding.UTF8;
            var modelFile = "SofiaPropertiesModel.zip";
            if (!File.Exists(modelFile))
            {
                TrainModel("imot.bg-2019-07-06.csv", modelFile);
            }

            var testModelData = new List<ModelInput>
                                {
                                    new ModelInput
                                    {
                                        BuildingType = "3-СТАЕН",
                                        District = "град София, Лозенец",
                                        Floor = 6,
                                        TotalFloors = 6,
                                        Size = 100,
                                        Year = 2017,
                                        Type = "Тухла",
                                    },
                                    new ModelInput
                                    {
                                        BuildingType = "3-СТАЕН",
                                        District = "град София, Лозенец",
                                        Floor = 6,
                                        TotalFloors = 6,
                                        Size = 100,
                                        Year = 1980,
                                        Type = "Тухла",
                                    },
                                    new ModelInput
                                    {
                                        BuildingType = "3-СТАЕН",
                                        District = "град София, Овча купел",
                                        Floor = 6,
                                        TotalFloors = 6,
                                        Size = 100,
                                        Year = 2017,
                                        Type = "Тухла",
                                    },
                                    new ModelInput
                                    {
                                        BuildingType = "3-СТАЕН",
                                        District = "град София, Лозенец",
                                        Floor = 1,
                                        TotalFloors = 6,
                                        Size = 100,
                                        Year = 2017,
                                        Type = "Тухла",
                                    },
                                    new ModelInput
                                    {
                                        BuildingType = "3-СТАЕН",
                                        District = "град София, Лозенец",
                                        Floor = 6,
                                        TotalFloors = 6,
                                        Size = 60,
                                        Year = 2017,
                                        Type = "Тухла",
                                    },
                                };
            testModelData.Dump();

            TestModel(modelFile, testModelData);
        }

        private static void TestModel(string modelFile, IEnumerable<ModelInput> testModelData)
        {
            var context = new MLContext();
            var model = context.Model.Load(modelFile, out _);
            var predictionEngine = context.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
            foreach (var testData in testModelData)
            {
                var prediction = predictionEngine.Predict(testData);
                Console.WriteLine(new string('-', 60));
                Console.WriteLine($"Input: {testData.Dump()}");
                Console.WriteLine($"Prediction: {prediction.Score}");
            }
        }

        private static void TrainModel(string dataFile, string modelFile)
        {
            var context = new MLContext();
            var trainingDataView = context.Data.LoadFromTextFile<ModelInput>(
                dataFile,
                hasHeader: true,
                separatorChar: ',',
                allowQuoting: true);

            // Data process configuration with pipeline data transformations
            var dataProcessPipeline = context.Transforms.Categorical
                .OneHotEncoding(
                    new[]
                    {
                        new InputOutputColumnPair(nameof(ModelInput.District), nameof(ModelInput.District)),
                        new InputOutputColumnPair(nameof(ModelInput.Type), nameof(ModelInput.Type)),
                        new InputOutputColumnPair(nameof(ModelInput.BuildingType), nameof(ModelInput.BuildingType)),
                    }).Append(
                    context.Transforms.Concatenate(
                        outputColumnName: "Features",
                        nameof(ModelInput.District),
                        nameof(ModelInput.Type),
                        nameof(ModelInput.BuildingType),
                        nameof(ModelInput.Size),
                        nameof(ModelInput.Floor),
                        nameof(ModelInput.TotalFloors),
                        nameof(ModelInput.Year)));

            // Set the training algorithm (GBM = Gradient Boosting Machine)
            var trainer = context.Regression.Trainers.LightGbm(
                new LightGbmRegressionTrainer.Options
                {
                    NumberOfIterations = 4000,
                    LearningRate = 0.1006953f,
                    NumberOfLeaves = 55,
                    MinimumExampleCountPerLeaf = 20,
                    UseCategoricalSplit = true,
                    HandleMissingValue = false,
                    MinimumExampleCountPerGroup = 200,
                    MaximumCategoricalSplitPointCount = 16,
                    CategoricalSmoothing = 10,
                    L2CategoricalRegularization = 1,
                    Booster = new GradientBooster.Options { L2Regularization = 0.5, L1Regularization = 0 },
                    LabelColumnName = nameof(ModelInput.Price),
                    FeatureColumnName = "Features",
                });
            var trainingPipeline = dataProcessPipeline.Append(trainer);

            //// var crossValidationResults = mlContext.Regression.CrossValidate(
            ////     trainingDataView,
            ////     trainingPipeline,
            ////     numberOfFolds: 5,
            ////     labelColumnName: "Price");

            ITransformer model = trainingPipeline.Fit(trainingDataView);
            context.Model.Save(model, trainingDataView.Schema, modelFile);
        }

        private static string Dump(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None);
        }
    }
}
