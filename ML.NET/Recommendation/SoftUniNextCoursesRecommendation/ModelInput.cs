namespace SoftUniNextCoursesRecommendation
{
    using Microsoft.ML.Data;

    public class ModelInput
    {
        [LoadColumn(0)]
        public int UserId { get; set; }

        [LoadColumn(1)]
        public int CourseId { get; set; }

        [LoadColumn(2)]
        public float Label { get; set; }
    }
}
