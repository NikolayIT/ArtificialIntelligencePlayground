namespace _256ShadesOfGray
{
    using System;
    using System.Linq;

    public static class Program
    {
        private const string AllDataPath = @"..\..\..\Data\digits_train.csv";

        public static void Main()
        {
            IDistance distance = new EuclideanDistance();
            IClassifier classifier = new BasicClassifier(distance);

            var allData = DataReader.ReadObservations(AllDataPath);
            var training = allData.Take((int)(allData.Length * 0.9)).ToArray();
            var validation = allData.Skip((int)(allData.Length * 0.9)).ToArray();
            
            classifier.Train(training);
            
            var correct = Evaluator.Correct(validation, classifier);
            Console.WriteLine("Correctly classified: {0:P2}", correct);

            Console.ReadLine();
        }
    }
}
