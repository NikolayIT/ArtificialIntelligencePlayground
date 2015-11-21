namespace _256ShadesOfGray
{
    using System.Collections.Generic;

    public class BasicClassifier : IClassifier
    {
        protected readonly IDistance Distance;

        protected IEnumerable<Observation> Data;

        public BasicClassifier(IDistance distance)
        {
            this.Distance = distance;
        }

        public void Train(IEnumerable<Observation> trainingSet)
        {
            this.Data = trainingSet;
        }

        public virtual string Predict(int[] pixels)
        {
            Observation currentBest = null;
            var shortest = double.MaxValue;

            foreach (var obs in this.Data)
            {
                var dist = this.Distance.Between(obs.Pixels, pixels);
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
