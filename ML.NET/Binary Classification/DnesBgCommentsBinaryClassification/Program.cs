namespace DnesBgCommentsBinaryClassification
{
    using System.Text;
    using System;
    using System.Threading.Tasks;

    using DnesBgCommentsBinaryClassification.Data;

    public static class Program
    {
        /*
         * Source: https://dnes.bg/
         * // Id [32-567341] means all news from 2005-01-01 to 2023-04-21, inclusive.
         * await new DnesBgDataGatherer().GatherAllDataAndSaveToDatabaseAsync("Server=.;Database=MlNetData;Integrated Security=True;TrustServerCertificate=True", 32, 567341);
         *
         * Negative comments:
         * WHERE [DownVotes] >= 15 AND (1.0 * [UpVotes]) / [DownVotes] <= 0.2   -- down >= 5 * up
         *
         * Positive comments:
         * WHERE [UpVotes] >= 30 AND (1.0 * [DownVotes]) / [UpVotes] <= 0.04   -- up >= 25 * down
         */
        public static async Task Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            // Id [32-567341] means all news from 2005-01-01 to 2023-04-21, inclusive.
            await new DnesBgDataGatherer().GatherAllDataAndSaveToDatabaseAsync("Server=.;Database=MlNetData;Integrated Security=True;TrustServerCertificate=True", 32, 567341);
        }
    }
}
