using System.Collections.Generic;
using System.Text;
using System;
using Microsoft.ML.Trainers;
using Microsoft.ML;
using CsvHelper;
using System.IO;
using System.Globalization;
using System.Linq;

namespace SoftUniNextCoursesRecommendation
{
    /*
     * Source: SoftUni
     *
     * softuni-users-2023-04-24.csv
     * UserId,CourseId
     *
     * softuni-courses-2023-04-24.csv
     * CourseId,CourseName
     */
    public static class Program
    {
        public static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            // Train model
            var modelFile = "SoftUniCoursesModel.zip";
            TrainModel(@"Data\softuni-users-2023-04-24.csv", modelFile);
            Console.WriteLine("Model ready.");

            // Test model
            var courses = new Dictionary<int, string>();
            using (var reader = new StreamReader(@"Data\softuni-courses-2023-04-24.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                courses = csv.GetRecords<Course>().ToDictionary(x => x.CourseId, x => x.CourseName);
            }

            Console.WriteLine();
            Console.WriteLine("User with courses \"Python Advanced\", \"Python OOP\" and \"Python Web Basics\":");
            TestModel(modelFile, new List<ModelInput>
                             {
                                 new ModelInput { UserId = 50000, CourseId = 465 }, // Python Web Framework
                                 new ModelInput { UserId = 50000, CourseId = 35 }, // Photoshop
                                 new ModelInput { UserId = 50000, CourseId = 450 }, // JS Advanced
                             }, courses);
            Console.WriteLine();
            Console.WriteLine("User with courses \"C# Advanced\", \"C# OOP\", \"MS SQL\" and \"EF Core\"");
            TestModel(modelFile, new List<ModelInput>
                             {
                                 new ModelInput { UserId = 100000, CourseId = 538 }, // ASP.NET Core
                                 new ModelInput { UserId = 100000, CourseId = 518 }, // QA Automation
                                 new ModelInput { UserId = 100000, CourseId = 67 }, // Практични умения за общуване
                             }, courses);
        }

        private static void TrainModel(string inputFile, string modelFile)
        {
            // Create MLContext to be shared across the model creation workflow objects
            var context = new MLContext();

            // Load data
            IDataView trainingDataView = context.Data.LoadFromTextFile<ModelInput>(
                inputFile,
                hasHeader: true,
                separatorChar: ',');

            // Build & train model
            IEstimator<ITransformer> estimator = context.Transforms.Conversion
                .MapValueToKey(outputColumnName: "userIdEncoded", inputColumnName: nameof(ModelInput.UserId)).Append(
                    context.Transforms.Conversion.MapValueToKey(outputColumnName: "courseIdEncoded", inputColumnName: nameof(ModelInput.CourseId)));
            var options = new MatrixFactorizationTrainer.Options
            {
                LossFunction = MatrixFactorizationTrainer.LossFunctionType.SquareLossOneClass,
                MatrixColumnIndexColumnName = "userIdEncoded",
                MatrixRowIndexColumnName = "courseIdEncoded",
                LabelColumnName = nameof(ModelInput.Label),
                Alpha = 0.1,
                Lambda = 0.5,
                NumberOfIterations = 100,
                Quiet = true,
            };

            var trainerEstimator = estimator.Append(context.Recommendation().Trainers.MatrixFactorization(options));
            ITransformer model = trainerEstimator.Fit(trainingDataView);

            // Save model
            context.Model.Save(model, trainingDataView.Schema, modelFile);
        }

        private static void TestModel(string modelFile, IEnumerable<ModelInput> testModelData, Dictionary<int, string> course)
        {
            var context = new MLContext();
            var model = context.Model.Load(modelFile, out _);
            var predictionEngine = context.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
            foreach (var testInput in testModelData)
            {
                var prediction = predictionEngine.Predict(testInput);
                Console.WriteLine($"User: {testInput.UserId}, Course: {course[testInput.CourseId]}, Score: {prediction.Score}");
            }
        }
    }
}