using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NeuralNetworks
{
    [Serializable]
    public class NeuralNW
    {

        private LayerNW[] Layers;
        private int countLayers = 0,
            countX, countY;
        private double[][] NETOUT;  // NETOUT[countLayers + 1][]
        private double[][] DELTA;   // NETOUT[countLayers    ][]


        //Получение числа входов и выходов сети
        public int GetX { get { return countX; } }
        public int GetY { get { return countY; } }
        public int CountLayers { get { return countLayers; } }
        public ActivationFunction AFtype { private set; get; }

        //Для хранения образа
        public Dictionary<int, string> IdVal { set; get; }

        /// <summary>
        /// Создает полносвязанную сеть из 1 слоя
        /// </summary>
        public NeuralNW(ActivationFunction af, int sizeX, int sizeY)
        {
            AFtype = af;
            countLayers = 1;
            Layers = new LayerNW[countLayers];
            Layers[0] = new LayerNW(sizeX, sizeY);
            Layers[0].GenerateWeights();
        }

        /// <summary>
        /// Создает полносвязанную сеть из нескольких слоев
        /// </summary>
        public NeuralNW(ActivationFunction af, int sizeX, params int[] layers)
        {
            AFtype = af;
            countLayers = layers.Length;
            countX = sizeX;
            countY = layers[layers.Length - 1];
            //Размерность выходов нейронов и Дельты
            NETOUT = new double[countLayers + 1][];
            NETOUT[0] = new double[sizeX];
            DELTA = new double[countLayers][];
            this.Layers = new LayerNW[countLayers];
            int countY1, countX1 = sizeX;
            //Задаём размерность и генерируем случайные веса
            for (int i = 0; i < countLayers; i++)
            {
                countY1 = layers[i];
                NETOUT[i + 1] = new double[countY1];
                DELTA[i] = new double[countY1];
                this.Layers[i] = new LayerNW(countX1, countY1);
                this.Layers[i].GenerateWeights();
                countX1 = countY1;
            }
        }

        /// <summary>
        /// Возвращает значение заданного слоя НС
        /// </summary>
        public void NetOUT(double[] inX, out double[] outY, int jLayer)
        {
            GetOUT(inX, jLayer);
            int N = NETOUT[jLayer].Length;
            outY = new double[N];
            for (int i = 0; i < N; i++)
                outY[i] = NETOUT[jLayer][i];
        }

        /// <summary>
        /// Возвращает значение НС
        /// </summary>
        public void NetOUT(double[] inX, out double[] outY)
        {
            int j = countLayers;
            NetOUT(inX, out outY, j);
        }

        /// <summary>
        /// Возвращает ошибку (метод наименьших квадратов)
        /// </summary>
        public double CalcError(double[] X, double[] Y)
        {
            double kErr = 0;
            // Считаем производные
            for (int i = 0; i < Y.Length; i++)
            {
                if (AFtype == ActivationFunction.Sigmoid)
                {
                    kErr += Math.Pow(Y[i] - NETOUT[countLayers][i], 2);
                }
                else if (AFtype == ActivationFunction.HyperbolicTangent)
                {
                    kErr += 1.0 / Math.Pow(Math.Cosh(NETOUT[countLayers][i]), 2);
                }
                else if (AFtype == ActivationFunction.Arctangent)
                {
                    kErr += 1.0 / (1.0 + Math.Pow(NETOUT[countLayers][i], 2));
                }
            }
            return 0.5 * kErr;
        }

        /// <summary>
        /// Обучает сеть, изменяя ее весовые коэффициэнты
        /// </summary>
        public double LernNW(double[] X, double[] Y, double kLern)
        {
            double O;  // Вход нейрона
            double s;
            // Вычисляем выход сети
            GetOUT(X);
            // Заполняем последний слой
            for (int j = 0; j < Layers[countLayers - 1].countY; j++)
            {
                O = NETOUT[countLayers][j];
                DELTA[countLayers - 1][j] = (Y[j] - O) * O * (1 - O);
            }
            // Перебираем все слои начиная споследнего, изменяя веса и вычисляя дельта для скрытого слоя
            for (int k = countLayers - 1; k >= 0; k--)
            {
                // Изменяем веса выходного слоя
                for (int j = 0; j < Layers[k].countY; j++)
                {
                    for (int i = 0; i < Layers[k].countX; i++)
                    {
                        Layers[k][i, j] += kLern * DELTA[k][j] * NETOUT[k][i];
                    }
                }
                if (k > 0)
                {
                    // Вычисляем дельта слоя к-1
                    for (int j = 0; j < Layers[k - 1].countY; j++)
                    {
                        s = 0;
                        for (int i = 0; i < Layers[k].countY; i++)
                            s += Layers[k][j, i] * DELTA[k][i];
                        DELTA[k - 1][j] = NETOUT[k][j] * (1 - NETOUT[k][j]) * s;
                    }
                }
            }
            return CalcError(X, Y);
        }

        /// <summary>
        /// Возвращает num-й слой НС
        /// </summary>
        public LayerNW Layer(int num)
        {
            return Layers[num];
        }

        // Возвращает все значения нейронов до lastLayer слоя
        void GetOUT(double[] inX, int lastLayer)
        {
            double s;
            for (int j = 0; j < Layers[0].countX; j++) { NETOUT[0][j] = inX[j]; }
            for (int i = 0; i < lastLayer; i++)
            {
                // размерность столбца проходящего через i-й слой
                for (int j = 0; j < Layers[i].countY; j++)
                {
                    s = 0;
                    for (int k = 0; k < Layers[i].countX; k++)
                        s += Layers[i][k, j] * NETOUT[i][k];
                    // Вычисляем значение активационной функции
                    s = SetAF(AFtype, s);
                    NETOUT[i + 1][j] = s;
                }
            }
        }

        // Возвращает все значения нейронов всех слоев
        private void GetOUT(double[] inX)
        {
            GetOUT(inX, countLayers);
        }

        ///Функции активации
        //Сигмоид
        private double AFsigmoid(double s)
        {
            return 1.0 / (1.0 + Math.Exp(-s));
        }
        //Гиперболический тангенс
        private double AFhyperbolicTangent(double s)
        {
            return Math.Tanh(s);
        }
        // Арктангенс
        private double AFarctangent(double s)
        {
            return Math.Atan(s);
        }

        // Получить тип функции активации активации
        private double SetAF(ActivationFunction taf, double x)
        {
            double res = 0.0;
            if (taf == ActivationFunction.Sigmoid) { res = AFsigmoid(x); }
            else if (taf == ActivationFunction.HyperbolicTangent) { res = AFhyperbolicTangent(x); }
            else if (taf == ActivationFunction.Arctangent) { res = AFarctangent(x); }
            return res;
        }
    }
}
