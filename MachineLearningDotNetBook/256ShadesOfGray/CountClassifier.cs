namespace _256ShadesOfGray
{
    using System.Collections.Generic;
    using System.Linq;

    public class CountClassifier : BasicClassifier
    {
        private readonly double epsilon;

        public CountClassifier(IDistance distance, double epsilon)
            : base(distance)
        {
            this.epsilon = epsilon;
        }

        public override string Predict(int[] pixels)
        {
            var classified = new Dictionary<string, int>();

            foreach (var obs in this.Data)
            {
                var dist = this.Distance.Between(obs.Pixels, pixels);
                if (dist <= this.epsilon)
                {
                    if (!classified.ContainsKey(obs.Label))
                    {
                        classified.Add(obs.Label, 1);
                    }
                    else
                    {
                        classified[obs.Label]++;
                    }
                }
            }

            var item = classified.OrderByDescending(r => r.Value).FirstOrDefault().Key;
            return item;
        }
    }
}

// epsilon -> percentage
// 15000 -> 73.40%
// 18500 -> 81.20%
// 19000 -> 80.40%
// 20000 -> 78.80%