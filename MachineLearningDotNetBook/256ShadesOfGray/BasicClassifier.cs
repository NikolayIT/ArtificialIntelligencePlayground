namespace _256ShadesOfGray
{
    using System.Collections.Generic;

    public class BasicClassifier : IClassifier
    {
        private readonly IDistance distance;

        private IEnumerable<Observation> data;

        public BasicClassifier(IDistance distance)
        {
            this.distance = distance;
        }

        public void Train(IEnumerable<Observation> trainingSet)
        {
            this.data = trainingSet;
        }

        public string Predict(int[] pixels)
        {
            Observation currentBest = null;
            var shortest = double.MaxValue;

            foreach (var obs in this.data)
            {
                var dist = this.distance.Between(obs.Pixels, pixels);
                if (dist < shortest)
                {
                    shortest = dist;
                    currentBest = obs;
                }
            }

            return currentBest?.Label;
        }
    }
}
