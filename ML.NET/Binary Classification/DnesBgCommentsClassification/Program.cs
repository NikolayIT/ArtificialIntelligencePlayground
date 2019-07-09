namespace DnesBgCommentsClassification
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    using Microsoft.ML;
    using Microsoft.ML.Trainers;

    public class Program
    {
        public static void Main()
        {
            /*
             * Source: https://www.dnes.bg/
             * var comments = new DnesBgDataGatherer().GatherData(415500, 1).GetAwaiter().GetResult();
             * All comments:
             *
             * Positive comments:
             * WHERE [DownVotes] >= 10 AND (1.0 * [UpVotes]) / [DownVotes] <= 0.2   -- down >= 5 * up
             *
             * Negative comments:
             * WHERE [UpVotes] >= 25 AND (1.0 * [DownVotes]) / [UpVotes] <= 0.04   -- up >= 25 * down
             */
            if (!File.Exists("all.csv"))
            {
                Console.Write("Extracting input file... ");
                ZipFile.ExtractToDirectory("all.zip", Environment.CurrentDirectory);
                Console.WriteLine("Done!");
            }

            Console.OutputEncoding = Encoding.UTF8;
            var modelFile = "DnesBgCommentsModel.zip";
            if (!File.Exists(modelFile))
            {
                Console.Write("Training the model... ");
                TrainModel("all.csv", modelFile);
                Console.WriteLine("Done!");
            }

            var testModelData = new List<string>
                                {
                                    // 0 up, 9 down
                                    "а НАТО защо прибутва границата напред, като са толкова миролюбиви? Досега единствената държава, която е прибутвала граници, е Фашиска Просия на Путин. И не само прибутва граници, но милитаризира новозаграбените територии.  Това са фактите.",

                                    // not a real comment
                                    "Русия, САЩ, Путин, Тръмп",

                                    // 5 up, 0 down
                                    "Тия шопи искат и правнуците да осигурят. Я до отидат в северозапада и да видят за какви пари става дума, а всичко е все земя, даже там е далеч по плодородна.",

                                    // 8 up, 0 down
                                    "Колкото и да увеличават часовете за кормуване има доста шофьори, които никога не трябва да бъдат допускани на пътя, включително и много такива, които се мислят за страхотни шофьори.",

                                    // 16 up, 0 down
                                    "Напротив! Кормуват си отлично, със самочувствие и презрение към Вас! Състезатели! Сега, били дрогирани и нямат акъл и правилно възпитание не е проблем. Ще улегнат в някой бил борд. Важно е да се вземат парите!",

                                    // 35 up, 1 down
                                    "Причината за многото жертви на пътя не е в курса за обучение, а е в интелектуалното състояние на българина. Огромната дупка няма как да се запълни с допълнителните часове практика. Комплексите, ниския интелект, липсата на градивни критерии и отговорност убиват психически в ежедневието и за жалост физически на пътя.",

                                    // WRONGLY predicted - 0 up, 9 down
                                    "Oчаквам скоро да го отровят с Полоний 210, подобно на Литвиненко.",
                                };

            TestModel(modelFile, testModelData);
        }

        private static void TrainModel(string inputFile, string modelFile)
        {
            var context = new MLContext(seed: 1);

            // Load Data
            IDataView trainingDataView = context.Data.LoadFromTextFile<ModelInput>(
                inputFile,
                hasHeader: true,
                separatorChar: ',',
                allowQuoting: true);

            // Build training pipeline
            var dataProcessPipeline = context.Transforms.Text.FeaturizeText("Content_tf", nameof(ModelInput.Content))
                .Append(context.Transforms.CopyColumns("Features", "Content_tf"))
                .Append(context.Transforms.NormalizeMinMax("Features", "Features")).AppendCacheCheckpoint(context);

            // Set the training algorithm
            var trainer = context.BinaryClassification.Trainers.LbfgsLogisticRegression(
                new LbfgsLogisticRegressionBinaryTrainer.Options
                {
                    L2Regularization = 0.6925718f,
                    L1Regularization = 0.6819714f,
                    OptimizationTolerance = 0.0001f,
                    HistorySize = 50,
                    MaximumNumberOfIterations = 95499535,
                    InitialWeightsDiameter = 0.9147193f,
                    DenseOptimizer = true,
                    LabelColumnName = nameof(ModelInput.IsPositive),
                    FeatureColumnName = "Features",
                });
            var trainingPipeline = dataProcessPipeline.Append(trainer);

            // Train Model
            ITransformer model = trainingPipeline.Fit(trainingDataView);

            // Save model
            context.Model.Save(model, trainingDataView.Schema, modelFile);
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
                Console.WriteLine($"Is OK? {prediction.Prediction}");
                Console.WriteLine($"Score: {prediction.Score}");
            }
        }
    }
}
