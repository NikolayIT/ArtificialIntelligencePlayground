namespace SofiaPropertiesPricePrediction
{
    using Microsoft.ML.Data;

    public class ModelInput
    {
        [LoadColumn(0)]
        public float Size { get; set; }

        [LoadColumn(1)]
        public float Floor { get; set; }

        [LoadColumn(2)]
        public float TotalFloors { get; set; }

        [LoadColumn(3)]
        public string District { get; set; }

        [LoadColumn(4)]
        public float Year { get; set; }

        [LoadColumn(5)]
        public string Type { get; set; }

        [LoadColumn(6)]
        public string BuildingType { get; set; }

        [LoadColumn(7)]
        public float Price { get; set; }
    }
}
