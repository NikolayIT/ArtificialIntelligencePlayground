namespace OpenALPR
{
    using System;

    using openalprnet;

    public static class Program
    {
        public static void Main()
        {
            var alpr = new AlprNet(
                "eu",
                Environment.CurrentDirectory + @"\openalpr.conf",
                Environment.CurrentDirectory + @"\runtime_data");
            if (!alpr.IsLoaded())
            {
                Console.WriteLine("OpenAlpr failed to load!");
                return;
            }

            Console.WriteLine($"Version: {AlprNet.GetVersion()}");
            var results = alpr.Recognize(Environment.CurrentDirectory + @"\samples\niki_ivo.jpg");

            for (int index = 0; index < results.Plates.Count; index++)
            {
                var result = results.Plates[index];
                Console.WriteLine($"Plate {index}: {result.TopNPlates.Count} result(s)");
                Console.WriteLine($"  Processing Time: {result.ProcessingTimeMs} msec(s)");
                foreach (var plate in result.TopNPlates)
                {
                    Console.WriteLine(
                        $"  - {plate.Characters}\t Confidence: {plate.OverallConfidence}\tMatches Template: {plate.MatchesTemplate}");
                }
            }
        }
    }
}
