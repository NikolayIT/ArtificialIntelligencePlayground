namespace BulgarianJokesMultiClassClassification
{
    using System.Text;
    using System;
    using System.Threading.Tasks;

    using BulgarianJokesMultiClassClassification.Data;

    /*
     * Source: https://fun.dir.bg/
     * await new FunDirBgDataGatherer().GatherAllDataAndSaveToDatabaseAsync("Server=.;Database=MlNetData;Integrated Security=True;TrustServerCertificate=True", 1, 49000);
     *
     * Removed category "Разни"
     * Removed categories with less than 20 jokes
     *
     * jokes-train-data.csv: ???? jokes in ?? categories
     */
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            await new FunDirBgDataGatherer().GatherAllDataAndSaveToDatabaseAsync("Server=.;Database=MlNetDataJokes;Integrated Security=True;TrustServerCertificate=True", 1, 49000);
        }
    }
}
