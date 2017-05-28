using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetworks
{
    [Serializable]
    public class Types
    {
    }
    public enum ActivationFunction { Sigmoid, HyperbolicTangent, Arctangent }

    [Serializable]
    public struct ParamsNN
    {
        public ParamsNN(int s, int[] l, ActivationFunction af)
        {
            Size = s;
            Layers = l;
            AFtype = af;
        }
        public int Size;
        public int[] Layers;
        public ActivationFunction AFtype;
    }
}
