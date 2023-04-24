namespace SofiaPropertiesPricePredictionWithRegression
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.Json;

    using SofiaPropertiesPricePredictionWithRegression.Data;

    using Microsoft.ML;
    using System.Text.Encodings.Web;

    public static class Program
    {
        /*
         * Source: https://www.imot.bg/
         * await new ImotBgDataGatherer().GatherAllDataAndSaveToDatabaseAsync("Server=.;Database=MlNetDataProperties;Integrated Security=True;TrustServerCertificate=True", 10, 1000);
         *
         * 23018 records in imot.bg-raw-data-2023-04-23.csv
         *
         * 18775 records in imot.bg-2023-04-23.csv
         * (WHERE [Price] > 0 AND [Year] > 0 AND [Floor] > 0 AND [TotalFloors] > 0 AND Size < 500)
         */
        public static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            var modelFile = "SofiaPropertiesModel.zip";
            if (!File.Exists(modelFile))
            {
                TrainModel("Data/imot.bg-2023-04-23.csv", modelFile);
                Console.WriteLine("Model training complete.");
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
                                        Year = 2020,
                                        Type = "Тухла",
                                    },
                                    new ModelInput
                                    {
                                        BuildingType = "3-СТАЕН",
                                        District = "град София, Люлин 4",
                                        Floor = 6,
                                        TotalFloors = 6,
                                        Size = 100,
                                        Year = 2020,
                                        Type = "Тухла",
                                    },
                                    new ModelInput
                                    {
                                        BuildingType = "3-СТАЕН",
                                        District = "град София, Лозенец",
                                        Floor = 1,
                                        TotalFloors = 6,
                                        Size = 100,
                                        Year = 2020,
                                        Type = "Тухла",
                                    },
                                    new ModelInput
                                    {
                                        BuildingType = "2-СТАЕН",
                                        District = "град София, Лозенец",
                                        Floor = 6,
                                        TotalFloors = 6,
                                        Size = 60,
                                        Year = 2020,
                                        Type = "Тухла",
                                    },
                                };
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
            MLContext mlContext = new MLContext(seed: 0);
            IDataView dataView = mlContext.Data.LoadFromTextFile<ModelInput>(
                dataFile,
                hasHeader: true,
                separatorChar: ',',
                allowQuoting: true);

            // Set the training algorithm
            var trainer = mlContext.Regression.Trainers.FastTree(numberOfTrees: 100, numberOfLeaves: 25);

            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(ModelInput.Price))
                    .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DistrictEncoded", inputColumnName: nameof(ModelInput.District)))
                    .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "TypeEncoded", inputColumnName: nameof(ModelInput.Type)))
                    .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "BuildingTypeEncoded", inputColumnName: nameof(ModelInput.BuildingType)))
                    .Append(mlContext.Transforms.Concatenate(
                        outputColumnName: "Features",
                        "DistrictEncoded",
                        "TypeEncoded",
                        "BuildingTypeEncoded",
                        nameof(ModelInput.Size),
                        nameof(ModelInput.Floor),
                        nameof(ModelInput.TotalFloors),
                        nameof(ModelInput.Year)))
                    .Append(trainer);

            var model = pipeline.Fit(dataView);

            mlContext.Model.Save(model, dataView.Schema, modelFile);
        }

        private static string Dump(this object obj)
        {
            JsonSerializerOptions jso = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true,
            };
            return JsonSerializer.Serialize(obj, jso);
        }
    }
}
