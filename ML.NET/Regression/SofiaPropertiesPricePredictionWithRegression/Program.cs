namespace SofiaPropertiesPricePredictionWithRegression
{
    using System.Threading.Tasks;

    using SofiaPropertiesPricePredictionWithRegression.Data;

    public static class Program
    {
        /*
         * Source: https://www.imot.bg/
         * await new ImotBgDataGatherer().GatherAllDataAndSaveToDatabaseAsync("Server=.;Database=MlNetDataProperties;Integrated Security=True;TrustServerCertificate=True", 10, 1000);
         *
         * 24228 records in imot.bg-raw-data-2019-07-06.csv
         *
         * WHERE [Price] > 0 AND [Year] > 0 AND [Floor] > 0 AND [TotalFloors] > 0
         *
         * 16083 records in imot.bg-2019-07-06.csv
         */
        public static async Task Main()
        {
            await new ImotBgDataGatherer().GatherAllDataAndSaveToDatabaseAsync("Server=.;Database=MlNetDataProperties;Integrated Security=True;TrustServerCertificate=True", 10, 1000);
        }
    }
}
