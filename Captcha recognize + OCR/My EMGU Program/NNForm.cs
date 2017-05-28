using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeuralNetworks;


namespace MainApp
{
    public partial class NNForm : Form
    {
        // Счетчик для настройки нейронов в скрытых слоях
        NumericUpDown[] NLayers;

        // Метки скытых слоев
        Label[] NLayerLabels;

        // Количество скрытых слоев
        int countLayers = 0;

        // Слои
        // int[] layers;

        // Размер
        // int sizeX;

        // cr

        //public event EventHandler<ParamsNN> CreateNNok;


        private void NNForm_Load(object sender, EventArgs e)
        {

        }
        // Конструктор
        public NNForm()
        {
            InitializeComponent();
            comboBoxAF.DataSource = Enum.GetValues(typeof(ActivationFunction));
        }

        // Создать сеть
        private void CreateNN()
        {
            int[] layers = new int[countLayers + 1];

            int sizeX = (int)numericUpDownNumIn.Value;

            for (int i = 0; i < countLayers; i++) { layers[i] = (int)NLayers[i].Value; }

            layers[countLayers] = (int)numericUpDownNumOut.Value;

           // if (CreateNNok != null)
           // {
          //      CreateNNok(this, new ParamsNN(sizeX, layers, (ActivationFunction)comboBoxAF.SelectedItem));
          //  }

            Close();
        }

        // Добавление контролов для задания числа нейронов в слоях
        void CreateNumeric()
        {
            for (int i = 0; i < countLayers; i++)
            {
                this.panelLayers.Controls.Remove(this.NLayers[i]);
                this.panelLayers.Controls.Remove(this.NLayerLabels[i]);
            }

            countLayers = (int)numericUpDownNumLay.Value;

            NLayers = new NumericUpDown[countLayers];
            NLayerLabels = new Label[countLayers];

            for (int i = 0; i < countLayers; i++)
            {
                // Создаем счетчик слоев
                NLayers[i] = new NumericUpDown();

                NLayers[i].Left = 6;
                NLayers[i].Top = 16 + 39 * i;

                NLayers[i].Maximum = 10000;
                NLayers[i].Minimum = 1;

                this.panelLayers.Controls.Add(this.NLayers[i]);

                // Создаем Метку i-го скрытого слоя
                NLayerLabels[i] = new Label();
                NLayerLabels[i].Top = 39 * i;
                NLayerLabels[i].Left = 6;
                NLayerLabels[i].Text = "Нейронов в " + Convert.ToString(i + 1) + " слое: ";
                NLayerLabels[i].AutoSize = true;

                this.panelLayers.Controls.Add(this.NLayerLabels[i]);
            }
        }

        private void buttonCreat_Click_1(object sender, EventArgs e)
        {
            CreateNumeric();
            CreateNN();
        }

        private void numericUpDownNumLay_ValueChanged_1(object sender, EventArgs e)
        {
            if (numericUpDownNumLay.Value > 3)
            {
                numericUpDownNumLay.Value = 3;
                return;
            }
            else if (numericUpDownNumLay.Value < 0)
            {
                numericUpDownNumLay.Value = 0;
                return;
            }

            CreateNumeric();
        }
    }
}
