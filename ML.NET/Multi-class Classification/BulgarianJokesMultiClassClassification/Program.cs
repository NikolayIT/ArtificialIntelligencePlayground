namespace BulgarianJokesMultiClassClassification
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using BulgarianJokesMultiClassClassification.Data;

    using Microsoft.ML;

    /*
     * Source: https://fun.dir.bg/
     * await new FunDirBgDataGatherer().GatherAllDataAndSaveToDatabaseAsync("Server=.;Database=MlNetDataJokes;Integrated Security=True;TrustServerCertificate=True", 1, 49000);
     *
     * Total jokes: 32753
     * Removed category "Разни"
     * Removed categories with less than 30 jokes
     * jokes-train-data.csv: 24756 jokes in 83 categories
     */
    public static class Program
    {
        public static async Task Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            var modelFile = "JokesCategoryModel.zip";
            if (!File.Exists(modelFile))
            {
                TrainModel(@"Data\fun.dir.bg-2023-04-23.csv", modelFile);
            }

            var testModelData = new List<string>
                                    {
                                        "Обява: Търси се програмист с компютърни познания.",
                                        "Две борчета седят и си мислят.",
                                        "Една блондинка пише книга за живота си... след 2 седмици книгата става най-тиражираният сборник за вицове..",
                                        "Зайо Байо, Кума Лиса и Кумчо Вълчо играят карти. По някое време Зайо казва: - Някой лъже здраво тука и само да разбера кой е ще му смачкам рижавата мутра!",
                                        "Котка и куче седят и си мислят. Кучето: - Този човек ме храни, гали, играе си с мен... Сигурно е бог. Котето: - Този човек ме храни, гали, играе си с мен... Сигурно съм бог.",
                                        "Няма такова друго животно, което да изпие толкова бира, като пържената цаца!",
                                    };

            TestModel(modelFile, testModelData);
        }

        private static void TrainModel(string dataFile, string modelFile)
        {
            // Create MLContext to be shared across the model creation workflow objects
            var context = new MLContext(seed: 0);

            // Loading the data
            Console.WriteLine($"Loading the data ({dataFile})");
            var trainingDataView = context.Data.LoadFromTextFile<ModelInput>(dataFile, ',', true, true, true);

            // Common data process configuration with pipeline data transformations
            Console.WriteLine("Map raw input data columns to ML.NET data");
            var dataProcessPipeline = context.Transforms.Conversion.MapValueToKey("Label", nameof(ModelInput.Category))
                .Append(context.Transforms.Text.FeaturizeText("Features", nameof(ModelInput.Content)));

            // Create the selected training algorithm/trainer
            Console.WriteLine("Create and configure the selected training algorithm (trainer)");
            var trainer = context.MulticlassClassification.Trainers.SdcaMaximumEntropy(); // SDCA = Stochastic Dual Coordinate Ascent
            //// Alternative: LightGbm (GBM = Gradient Boosting Machine)

            // Set the trainer/algorithm and map label to value (original readable state)
            var trainingPipeline = dataProcessPipeline.Append(trainer).Append(
                context.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            // Train the model fitting to the DataSet
            Console.WriteLine("Train the model fitting to the DataSet");
            var trainedModel = trainingPipeline.Fit(trainingDataView);

            // Save/persist the trained model to a .ZIP file
            Console.WriteLine($"Save the model to a file ({modelFile})");
            context.Model.Save(trainedModel, trainingDataView.Schema, modelFile);
        }

        private static void TestModel(string modelFile, IEnumerable<string> testModelData)
        {
            var context = new MLContext();
            var model = context.Model.Load(modelFile, out _);
            var predictionEngine = context.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
            foreach (var testData in testModelData)
            {
                var prediction = predictionEngine.Predict(new ModelInput { Content = testData });
                Console.WriteLine(new string('-', 60));
                Console.WriteLine($"Content: {testData}");
                Console.WriteLine($"Prediction: {prediction.Category}");
                Console.WriteLine($"Score: {prediction.Score.Max()}");
            }
        }
    }
}

/*
  SELECT [Category]
      ,REPLACE(REPLACE([Content], CHAR(13), ''), CHAR(10), ' ') AS Content
  FROM [MlNetDataJokes].[dbo].[BulgarianJokes] j
  WHERE Category != N'Разни' AND (SELECT COUNT(*) FROM [BulgarianJokes] j2 WHERE j.[Category] = j2.[Category]) > 30
  ORDER BY [Id] DESC
*/