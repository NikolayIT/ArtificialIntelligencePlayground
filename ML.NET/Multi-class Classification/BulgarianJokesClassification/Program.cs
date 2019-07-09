namespace BulgarianJokesClassification
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Microsoft.ML;

    public static class Program
    {
        public static void Main()
        {
            /*
             * Source: https://fun.dir.bg/
             * var jokes = new DataGatherer().GatherData(1, 47000);
             *
             * Removed category "Разни"
             * Removed categories with less than 20 jokes
             *
             * jokes-train-data.csv: 24771 jokes in 167 categories
             */

            Console.OutputEncoding = Encoding.UTF8;
            var modelFile = "JokesCategoryModel.zip";
            if (!File.Exists(modelFile))
            {
                TrainModel("jokes-train-data.csv", modelFile);
            }

            var testModelData = new List<string>
                                    {
                                        "Обява: Търси се програмист с компютърни познания.",
                                        "Две борчета седят и си мислят.",
                                        "Всички фенове на Левски и Славия се сбиха в асансьор!",
                                        "Една блондинка пише книга за живота си... след 2 седмици книгата става най-тиражираният сборник за вицове..",
                                        "Зайо, Лиса и Кумчо Вълчо играят карти. По някое време Зайо казва: - Някой лъже здраво тука и само да разбера кой е ще му смачкам рижавата мутра!",
                                        "ГДБОП разкри група за компютърни измами. Специализираната прокуратура конфискува криптовалута за 5 млн. лева.",
                                        "Котка и куче",
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
            var trainingDataView = context.Data.LoadFromTextFile<JokeModel>(dataFile, ',', true, true, true);

            // Common data process configuration with pipeline data transformations
            Console.WriteLine("Map raw input data columns to ML.NET data");
            var dataProcessPipeline = context.Transforms.Conversion.MapValueToKey("Label", nameof(JokeModel.Category))
                .Append(context.Transforms.Text.FeaturizeText("Features", nameof(JokeModel.Content)));

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
            var predictionEngine = context.Model.CreatePredictionEngine<JokeModel, JokeModelPrediction>(model);
            foreach (var testData in testModelData)
            {
                var prediction = predictionEngine.Predict(new JokeModel { Content = testData });
                Console.WriteLine(new string('-', 60));
                Console.WriteLine($"Content: {testData}");
                Console.WriteLine($"Prediction: {prediction.Category}");
                Console.WriteLine($"Score: {prediction.Score.Max()}");
            }
        }
    }
}
