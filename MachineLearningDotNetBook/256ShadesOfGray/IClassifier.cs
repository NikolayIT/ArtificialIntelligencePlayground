namespace _256ShadesOfGray
{
    using System.Collections.Generic;

    using CSharp;

    public interface IClassifier
    {
        void Train(IEnumerable<Observation> trainingSet);
        string Predict(int[] pixels);
    }
}