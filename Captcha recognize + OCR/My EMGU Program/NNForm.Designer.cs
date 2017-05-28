namespace MainApp
{
    partial class NNForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.numericUpDownNumIn = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownNumOut = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownNumLay = new System.Windows.Forms.NumericUpDown();
            this.comboBoxAF = new System.Windows.Forms.ComboBox();
            this.panelLayers = new System.Windows.Forms.Panel();
            this.buttonCreat = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumIn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumOut)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumLay)).BeginInit();
            this.SuspendLayout();
            // 
            // numericUpDownNumIn
            // 
            this.numericUpDownNumIn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numericUpDownNumIn.Location = new System.Drawing.Point(348, 28);
            this.numericUpDownNumIn.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDownNumIn.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownNumIn.Name = "numericUpDownNumIn";
            this.numericUpDownNumIn.Size = new System.Drawing.Size(189, 26);
            this.numericUpDownNumIn.TabIndex = 1;
            this.numericUpDownNumIn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // numericUpDownNumOut
            // 
            this.numericUpDownNumOut.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numericUpDownNumOut.Location = new System.Drawing.Point(348, 62);
            this.numericUpDownNumOut.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDownNumOut.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownNumOut.Name = "numericUpDownNumOut";
            this.numericUpDownNumOut.Size = new System.Drawing.Size(189, 26);
            this.numericUpDownNumOut.TabIndex = 3;
            this.numericUpDownNumOut.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // numericUpDownNumLay
            // 
            this.numericUpDownNumLay.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numericUpDownNumLay.Location = new System.Drawing.Point(348, 96);
            this.numericUpDownNumLay.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDownNumLay.Maximum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numericUpDownNumLay.Name = "numericUpDownNumLay";
            this.numericUpDownNumLay.Size = new System.Drawing.Size(189, 26);
            this.numericUpDownNumLay.TabIndex = 5;
            this.numericUpDownNumLay.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDownNumLay.ValueChanged += new System.EventHandler(this.numericUpDownNumLay_ValueChanged_1);
            // 
            // comboBoxAF
            // 
            this.comboBoxAF.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAF.FormattingEnabled = true;
            this.comboBoxAF.Location = new System.Drawing.Point(348, 130);
            this.comboBoxAF.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxAF.Name = "comboBoxAF";
            this.comboBoxAF.Size = new System.Drawing.Size(188, 24);
            this.comboBoxAF.TabIndex = 8;
            // 
            // panelLayers
            // 
            this.panelLayers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelLayers.AutoScroll = true;
            this.panelLayers.Location = new System.Drawing.Point(348, 162);
            this.panelLayers.Margin = new System.Windows.Forms.Padding(4);
            this.panelLayers.Name = "panelLayers";
            this.panelLayers.Size = new System.Drawing.Size(189, 180);
            this.panelLayers.TabIndex = 10;
            // 
            // buttonCreat
            // 
            this.buttonCreat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCreat.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.buttonCreat.Location = new System.Drawing.Point(52, 355);
            this.buttonCreat.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCreat.Name = "buttonCreat";
            this.buttonCreat.Size = new System.Drawing.Size(179, 28);
            this.buttonCreat.TabIndex = 11;
            this.buttonCreat.Text = "Создать";
            this.buttonCreat.UseVisualStyleBackColor = true;
            this.buttonCreat.Click += new System.EventHandler(this.buttonCreat_Click_1);
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(12, 138);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(275, 25);
            this.label4.TabIndex = 15;
            this.label4.Text = "Функция активации";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(13, 102);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(275, 25);
            this.label3.TabIndex = 14;
            this.label3.Text = "Число скрытых слоёв";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(13, 66);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(275, 25);
            this.label2.TabIndex = 13;
            this.label2.Text = "Число выходов";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(13, 29);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(275, 25);
            this.label1.TabIndex = 12;
            this.label1.Text = "Число входов";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // NNForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(615, 396);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonCreat);
            this.Controls.Add(this.panelLayers);
            this.Controls.Add(this.comboBoxAF);
            this.Controls.Add(this.numericUpDownNumLay);
            this.Controls.Add(this.numericUpDownNumOut);
            this.Controls.Add(this.numericUpDownNumIn);
            this.Name = "NNForm";
            this.Text = "NNForm";
            this.Load += new System.EventHandler(this.NNForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumIn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumOut)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumLay)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NumericUpDown numericUpDownNumIn;
        private System.Windows.Forms.NumericUpDown numericUpDownNumOut;
        private System.Windows.Forms.NumericUpDown numericUpDownNumLay;
        private System.Windows.Forms.ComboBox comboBoxAF;
        private System.Windows.Forms.Panel panelLayers;
        private System.Windows.Forms.Button buttonCreat;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}