using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetworks
{
    [Serializable]
    public class LayerNW
    {
     

        //Веса
        private double[,] Weights;
        private int cX;
        private int cY;



        //Свойство для сX
        public int countX
        {
            get { return cX; }
        }

        //Свойство для сY
        public int countY
        {
            get { return cY; }
        }

        // Индексатор
        public double this[int row, int col]
        {
            get { return Weights[row, col]; }
            set { Weights[row, col] = value; }
        }




        //Конструктор с параметрами. передается количество входных и выходных нейронов
        public LayerNW(int countX, int countY)
        {
            cX = countX;
            cY = countY;
            GiveMemory();
        }

        //Заполняем веса случайными числами
        public void GenerateWeights()
        {
            Random rnd = new Random();

            for (int i = 0; i < cX; i++)
            {
                for (int j = 0; j < cY; j++) { Weights[i, j] = rnd.NextDouble() - 0.5; }
            }
        }

        //Выделяется память под веса
        protected void GiveMemory()
        {
            Weights = new double[cX, cY];
        }

    }
}
