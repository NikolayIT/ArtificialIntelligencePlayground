namespace mltest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.ML;
    using Microsoft.ML.Core.Data;
    using Microsoft.ML.Runtime.Api;
    using Microsoft.ML.Runtime.Data;

    public static class Program
    {
        public static void Main(string[] args)
        {
            /*
             * -- 24771 jokes in the data file. Source:
             * SELECT j.Id, c.[Name],
             * REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE([Content], CHAR(13), ''), CHAR(10), ' '), '.', ' '), ',', ' '), '-', ' '), '"', ' '), ':', ' '), '?', ' '), '!', ' ') AS [Content]
             * FROM [FunApp].[dbo].[Jokes] j
             * JOIN [Categories] c ON c.Id = j.CategoryId
             * WHERE c.Id != 4 -- Разни
             * 		AND (SELECT COUNT(*) FROM Jokes WHERE CategoryId = c.Id) >= 0 -- at least 20 jokes in the category
             */
            var dataFile = "jokes-train-data.csv";
            var modelFile = "JokesCategoryModel.zip";
            // TrainModel(dataFile, modelFile);

            var testModelData = new List<string>
                                    {
                                        "Обява: Търси се програмист с компютърни познания.",
                                        "Две борчета седят и си мислят.",
                                        "Всичките фенове на Левски и Славия се сбиха в асансьор!",
                                        "Една блондинка пише книга за живота си... след 2 седмици книгата става най-тиражираният сборник за вицове..",
                                        "Зайо, Лиса и Кумчо Вълчо играят карти. По някое време Зайо казва: - Някой лъже здраво тука и само да разбера кой е ще му смачкам рижавата мутра!",
                                        "ГДБОП разкри група за компютърни измами. Специализираната прокуратура конфискува криптовалута за 5 млн. лева.",
                                        "Котка и куче",
                                    };

            TestModel(modelFile, testModelData);
        }

        private static void TestModel(string modelFile, IEnumerable<string> testModelData)
        {
            var mlContext = new MLContext(seed: 0);
            ITransformer trainedModel;
            using (var stream = new FileStream(modelFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                trainedModel = mlContext.Model.Load(stream);
            }

            var predFunction = trainedModel.MakePredictionFunction<JokeModel, JokeModelPrediction>(mlContext);
            foreach (var testData in testModelData)
            {
                var prediction = predFunction.Predict(new JokeModel { Content = testData });
                Console.WriteLine(new string('-', 60));
                Console.WriteLine($"Content: {testData}");
                Console.WriteLine($"Prediction: {prediction.Category}");
            }
        }

        private static void TrainModel(string dataFile, string modelFile)
        {
            // Create MLContext to be shared across the model creation workflow objects 
            var mlContext = new MLContext(seed: 0);

            // STEP 1: Loading the data
            Console.WriteLine($"Step 1: Loading the data ({dataFile})");
            var textLoader = mlContext.Data.TextReader(
                new TextLoader.Arguments
                    {
                        Separator = ",",
                        HasHeader = true,
                        AllowQuoting = true,
                        AllowSparse = true,
                        Column = new[]
                                     {
                                         new TextLoader.Column("Id", DataKind.Text, 0),
                                         new TextLoader.Column("Category", DataKind.Text, 1),
                                         new TextLoader.Column("Content", DataKind.Text, 2),
                                     }
                    });
            var trainingDataView = textLoader.Read(dataFile);

            // STEP 2: Common data process configuration with pipeline data transformations
            Console.WriteLine("Step 2: Map raw input data columns to ML.NET data");
            var dataProcessPipeline = mlContext.Transforms.Categorical.MapValueToKey("Category", DefaultColumnNames.Label)
                .Append(mlContext.Transforms.Text.FeaturizeText("Content", DefaultColumnNames.Features));

            // (OPTIONAL) Peek data (few records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
            // DataViewToConsole<JokeModel>(mlContext, trainingDataView, dataProcessPipeline, 2);

            // STEP 3: Create the selected training algorithm/trainer
            Console.WriteLine("Step 3: Create and configure the selected training algorithm (trainer)");
            IEstimator<ITransformer> trainer = mlContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent();

            // Alternative training
            //// var averagedPerceptionBinaryTrainer = mlContext.BinaryClassification.Trainers.AveragedPerceptron(
            ////     DefaultColumnNames.Label,
            ////     DefaultColumnNames.Features,
            ////     numIterations: 10);
            //// trainer = mlContext.MulticlassClassification.Trainers.OneVersusAll(averagedPerceptronBinaryTrainer);

            // Set the trainer/algorithm and map label to value (original readable state)
            var trainingPipeline = dataProcessPipeline.Append(trainer).Append(
                mlContext.Transforms.Conversion.MapKeyToValue(DefaultColumnNames.PredictedLabel));

            // STEP 4: Cross-Validate with single dataset (since we don't have two datasets, one for training and for evaluate)
            // in order to evaluate and get the model's accuracy metrics
            Console.WriteLine("Step 4: Cross-Validate with single dataset (alternatively we can divide it 80-20)");
            var crossValidationResults = mlContext.MulticlassClassification.CrossValidate(
                trainingDataView,
                trainingPipeline,
                numFolds: 10,
                labelColumn: "Label");
            PrintMulticlassClassificationFoldsAverageMetrics(trainer.ToString(), crossValidationResults);

            // STEP 5: Train the model fitting to the DataSet
            Console.WriteLine("Step 5: Train the model fitting to the DataSet");
            var trainedModel = trainingPipeline.Fit(trainingDataView);

            // STEP 6: Save/persist the trained model to a .ZIP file
            Console.WriteLine($"Step 6: Save the model to a file ({modelFile})");
            using (var fs = new FileStream(modelFile, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                mlContext.Model.Save(trainedModel, fs);
            }
        }

        public static void DataViewToConsole<TObservation>(
            MLContext mlContext,
            IDataView dataView,
            IEstimator<ITransformer> pipeline,
            int numberOfRows = 4)
            where TObservation : class, new()
        {
            // https://github.com/dotnet/machinelearning/blob/master/docs/code/MlNetCookBook.md#how-do-i-look-at-the-intermediate-data
            var transformer = pipeline.Fit(dataView);
            var transformedData = transformer.Transform(dataView);
            var someRows = transformedData.AsEnumerable<TObservation>(mlContext, false).Take(numberOfRows).ToList();
            foreach (var row in someRows)
            {
                var fieldsInRow = row.GetType().GetProperties();
                foreach (var field in fieldsInRow)
                {
                    Console.Write($"{field.Name}: {field.GetValue(row)} | ");
                }

                Console.WriteLine();
            }
        }

        public static void PrintMulticlassClassificationFoldsAverageMetrics(
                                         string algorithmName,
                                         (MultiClassClassifierEvaluator.Result metrics,
                                          ITransformer model,
                                          IDataView scoredTestData)[] crossValResults
                                                                           )
        {
            var metricsInMultipleFolds = crossValResults.Select(r => r.metrics);

            var microAccuracyValues = metricsInMultipleFolds.Select(m => m.AccuracyMicro);
            var microAccuracyAverage = microAccuracyValues.Average();
            var microAccuraciesStdDeviation = CalculateStandardDeviation(microAccuracyValues);
            var microAccuraciesConfidenceInterval95 = CalculateConfidenceInterval95(microAccuracyValues);

            var macroAccuracyValues = metricsInMultipleFolds.Select(m => m.AccuracyMacro);
            var macroAccuracyAverage = macroAccuracyValues.Average();
            var macroAccuraciesStdDeviation = CalculateStandardDeviation(macroAccuracyValues);
            var macroAccuraciesConfidenceInterval95 = CalculateConfidenceInterval95(macroAccuracyValues);

            var logLossValues = metricsInMultipleFolds.Select(m => m.LogLoss);
            var logLossAverage = logLossValues.Average();
            var logLossStdDeviation = CalculateStandardDeviation(logLossValues);
            var logLossConfidenceInterval95 = CalculateConfidenceInterval95(logLossValues);

            var logLossReductionValues = metricsInMultipleFolds.Select(m => m.LogLossReduction);
            var logLossReductionAverage = logLossReductionValues.Average();
            var logLossReductionStdDeviation = CalculateStandardDeviation(logLossReductionValues);
            var logLossReductionConfidenceInterval95 = CalculateConfidenceInterval95(logLossReductionValues);

            Console.WriteLine($"*************************************************************************************************************");
            Console.WriteLine($"*    Metrics for {algorithmName} Multi-class Classification model      ");
            Console.WriteLine($"*------------------------------------------------------------------------------------------------------------");
            Console.WriteLine($"*    Average MicroAccuracy:    {microAccuracyAverage:0.###}  - Standard deviation: ({microAccuraciesStdDeviation:#.###})  - Confidence Interval 95%: ({microAccuraciesConfidenceInterval95:#.###})");
            Console.WriteLine($"*    Average MacroAccuracy:    {macroAccuracyAverage:0.###}  - Standard deviation: ({macroAccuraciesStdDeviation:#.###})  - Confidence Interval 95%: ({macroAccuraciesConfidenceInterval95:#.###})");
            Console.WriteLine($"*    Average LogLoss:          {logLossAverage:#.###}  - Standard deviation: ({logLossStdDeviation:#.###})  - Confidence Interval 95%: ({logLossConfidenceInterval95:#.###})");
            Console.WriteLine($"*    Average LogLossReduction: {logLossReductionAverage:#.###}  - Standard deviation: ({logLossReductionStdDeviation:#.###})  - Confidence Interval 95%: ({logLossReductionConfidenceInterval95:#.###})");
            Console.WriteLine($"*************************************************************************************************************");
        }

        public static double CalculateStandardDeviation(IEnumerable<double> values)
        {
            double average = values.Average();
            double sumOfSquaresOfDifferences = values.Select(val => (val - average) * (val - average)).Sum();
            double standardDeviation = Math.Sqrt(sumOfSquaresOfDifferences / (values.Count() - 1));
            return standardDeviation;
        }

        public static double CalculateConfidenceInterval95(IEnumerable<double> values)
        {
            double confidenceInterval95 = 1.96 * CalculateStandardDeviation(values) / Math.Sqrt((values.Count() - 1));
            return confidenceInterval95;
        }
    }
}
