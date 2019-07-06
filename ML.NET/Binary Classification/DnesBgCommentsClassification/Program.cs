using System;

namespace DnesBgCommentsClassification
{
    class Program
    {
        static void Main(string[] args)
        {
            /* Source: 
             * var comments = new DnesBgDataGatherer().GatherData(415500, 1).GetAwaiter().GetResult();
             *
             * Positive comments:
             * WHERE [DownVotes] >= 10 AND (1.0 * [UpVotes]) / [DownVotes] <= 0.2   -- down >= 5 * up
             *
             * Negative comments:
             * WHERE [UpVotes] >= 25 AND (1.0 * [DownVotes]) / [UpVotes] <= 0.04   -- up >= 25 * down
             */
            Console.WriteLine("Hello World!");
        }
    }
}
