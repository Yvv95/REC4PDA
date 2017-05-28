using System;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using Aspose.OCR;
using Aspose.OCR.Filters;
using AForge;
//using AForge.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Web;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AForge.Math;
using AForge.Imaging;
using AForge.Imaging.Filters;
using NeuralNetworks;
using Tesseract;


namespace MainApp
{
    public partial class MainForm : Form
    {
        const string format = ".gif";
        public Dictionary<int, string> numberDictionary =
            new Dictionary<int, string>();
        public static List<string> syms = new List<string>();
        Size char_size = new Size(20, 20);
        public static string recognizedCaptcha = "";
        private char[] ch_arr = { 'Т', 'ч', 'Ш', 'в', 'Д', 'О', 'П', 'С', 'В', 'т', 'Ч' };
        private char[] _convert = { 'Т', 'ч', 'Ш', 'в', 'Д', 'О', 'П', 'С', 'В', 'т', 'Ч' };

        public int Width, Height, RowCount;
        bool _grayInUse = false;
        private string path = "";
        private bool run = false;
        public NeuralNW NET = null;
        bool Training = false;
        // Тестовая картинка
        private Bitmap TestBm;
        // Для хранения ID образа
        private Dictionary<int, string> IdVal;
        string train_dir = System.IO.Path.Combine(Application.StartupPath, @"train");

        public MainForm()
        {
            InitializeComponent();
            //Добавляются слипшиеся символы
            syms.Add("СЯ");
            syms.Add("СЕ");
            syms.Add("ДЕ");
            syms.Add("РЕ");
            button4.Visible = false;
            button5.Visible = false;
            button7.Visible = false;
            groupBox2.Visible = false;
            generateCaptcha.Visible = false;
            makeTest.Visible = false;
            MessageBox.Show("Для работы с НС сначала необходимо выбрать тенировочный файл");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //считывание словаря
            numberDictionary.Clear();
            StreamReader rdr =
            new StreamReader(System.IO.Path.Combine(Application.StartupPath, @"test.txt"));
            for (int i = 1000; i <= 9999; i++)
            {
                if (i < 2000)
                    numberDictionary.Add(i, Regex.Replace(rdr.ReadLine(), " ", "").ToLower().Remove(0, 4));//чтобы удалить слово "одна": одна тысяча,...
                else
                    numberDictionary.Add(i, Regex.Replace(rdr.ReadLine(), " ", "").ToLower());
                difference.Add(50); //50 - все символы отличаются
            }
            rdr.Close();
        }


        //тренировать на тестовых данных
        private void makeTest_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(train_dir))
            {
                MessageBox.Show("Проверьте путь до тренировочных данных");
                return;
            }
            timeStartLabel.Text = DateTime.Now.ToString();
            NNForm cnn = new NNForm();
            cnn.ShowDialog();
            List<string> rec = new List<string>();
            //MessageBox.Show("recognize:"+ Convert.ToString(Recognize(temp.ToBitmap())));
        }


        /// <summary>
        /// Сколько пикселей отличаются
        /// </summary>
        private int diffPictures(Bitmap pic1, Bitmap pic2)
        {
            int difference = 0;
            if ((pic1.Width == pic2.Width) && (pic1.Height == pic2.Height))
            {
                for (int i = 0; i < pic1.Width; i++)
                    for (int j = 0; j < pic2.Height; j++)
                        if (pic1.GetPixel(i, j) != pic2.GetPixel(i, j))
                            difference++;
            }
            else
                return Int32.MaxValue;
            return difference;
        }

        /// <summary>
        /// Простое сравнение
        /// </summary>
        private char simpleCompare(Bitmap image)
        {
            int minDiff = Int32.MaxValue;
            char rec = ch_arr[0];
            int count = 0;
            //string dirPath = Path.GetDirectoryName(train_dir);

            List<string> dirs =
                new List<string>(Directory.EnumerateDirectories(train_dir).OrderBy(s => byte.Parse(Path.GetFileName(s))));
            if (dirs.Count != ch_arr.Length)
                throw new Exception("error");

            foreach (var dir in dirs)
            //cnt_files += Directory.GetFiles(dir, @"*.gif", SearchOption.TopDirectoryOnly).Length;
            {
                var file_arr = Directory.GetFiles(dir, "*.gif").OrderBy(fn => fn);
                foreach (string fn in file_arr)
                {
                    // inputdata seperated by space
                    Bitmap bmp = new Bitmap(fn);
                    int dif = diffPictures(bmp, image);
                    if (dif < minDiff)
                    {
                        minDiff = dif;
                        rec = ch_arr[count];
                    }
                }
                count++;
            }
            return rec;
        }

        /// <summary>
        /// Выбор символа по максимальной "схожести" из НС
        /// </summary>
        private char GetCharByMax(double[] output)
        {
            if (ch_arr.Length != output.Length)
                throw new Exception("неправильный размер массива");
            double m = output.Max();
            int cnt = output.Count(v => v == m);
            //int idx = (cnt == 1 ? output.ToList().IndexOf(m) : -1);
            int idx = output.ToList().IndexOf(m);
            char ch_err = 'a';
            return (idx >= 0 ? ch_arr[idx] : ch_err);
        }


        private void CreateTrainFile(string train_file)
        {
            string dirPath = Path.GetDirectoryName(train_file);
            List<string> dirs =
                new List<string>(Directory.EnumerateDirectories(dirPath).OrderBy(s => byte.Parse(Path.GetFileName(s))));

            if (dirs.Count != ch_arr.Length)
                throw new Exception("error CreateTrainFile");

            int cnt_files = 0;
            foreach (var dir in dirs)
                cnt_files += Directory.GetFiles(dir, @"*.gif", SearchOption.TopDirectoryOnly).Length;
            // первая строка Train.tr : num_train_data num_input num_output
            string res = cnt_files.ToString() + " " + (char_size.Width * char_size.Height).ToString() + " " +
                         ch_arr.Length.ToString();
            for (int i = 0; i < dirs.Count; i++)
                res += (res.Length > 0 ? "\n" : "") + GetDataArr(i, dirs[i]);
            File.WriteAllText(train_file, res);
        }

        private string GetDataArr(int i, string dir)
        {
            string s = "";

            var file_arr = Directory.GetFiles(dir, "*.gif").OrderBy(fn => fn);
            foreach (string fn in file_arr)
            {
                // inputdata seperated by space
                string inputdata = "";
                Bitmap bmp = new Bitmap(fn);
                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                        inputdata += (inputdata.Length > 0 ? " " : "") +
                                     (bmp.GetPixel(x, y).ToArgb() == Color.Black.ToArgb() ? "1" : "0");
                string outputdata = "";
                for (int x = 0; x < ch_arr.Length; x++)
                    outputdata += (outputdata.Length > 0 ? " " : "") + (i == x ? "1" : "0");
                s += (s.Length > 0 ? "\n" : "") + inputdata + "\n" + outputdata;
            }
            return s;
        }

        public struct Contur
        {
            public int X, Y, W, H;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openfile = new OpenFileDialog();
            if (openfile.ShowDialog() == DialogResult.OK)
            {
                Image<Bgr, Byte> myImage = new Image<Bgr, byte>(openfile.FileName);
                Bitmap image = null;
                pictureBox1.Image = myImage.ToBitmap();
                pictureBox2.Image = pictureBox1.Image;
                image = myImage.ToBitmap();
                int value = Convert.ToInt32(textBox1.Text);
                //градиентный метод для контуров. на вход - битмап и значение порога
                EdgeDetection(image, value);
                pictureBox2.Image = image;
                Bitmap image1 = image;
                textBox2.Text = "";
                TesseractEngine ocr = new TesseractEngine(AppDomain.CurrentDomain.BaseDirectory, "rus");
                ocr.SetVariable("tessedit_char_whitelist", "АБВГДЕЖЗИКЛМНОПРСТУФХЦШЩЭЮЯ");
                var page = ocr.Process(image1);
                ocr.DefaultPageSegMode = PageSegMode.Auto;
                textBox2.Text += page.GetText().Trim();
                ocr.Dispose();
                Sobel();
                TesseractEngine ocr2 = new TesseractEngine(AppDomain.CurrentDomain.BaseDirectory, "rus");
                ocr2.SetVariable("tessedit_char_whitelist", "АБВГДЕЖЗИКЛМНОПРСТУФХЦШЩЭЮЯ");
                var page2 = ocr2.Process(new Bitmap(pictureBox3.Image));
                textBox2.Text += "|" + page2.GetText().Trim();
                ocr2.Dispose();
                TesseractEngine ocr3 = new TesseractEngine(AppDomain.CurrentDomain.BaseDirectory, "rus");
                ocr3.SetVariable("tessedit_char_whitelist", "АБВГДЕЖЗИКЛМНОПРСТУФХЦШЩЭЮЯабвгдежзиклмнопрстуфхцшщэюя");
                var page3 = ocr3.Process(new Bitmap(pictureBox1.Image));
                textBox2.Text += "|" + page3.GetText().Trim();
                ocr3.Dispose();
            }
        }

        /// <summary>
        /// Градиентный метод выделения контуров. Не используется
        /// </summary>
        public static void EdgeDetection(Bitmap b, float threshold)
        {
            Bitmap bSrc = (Bitmap)b.Clone();

            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb);
            BitmapData bmSrc = bSrc.LockBits(new Rectangle(0, 0, bSrc.Width, bSrc.Height), ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            unsafe
            {
                byte* p = (byte*)(void*)bmData.Scan0;
                byte* pSrc = (byte*)(void*)bmSrc.Scan0;

                int nOffset = stride - b.Width * 3;
                int nWidth = b.Width - 1;
                int nHeight = b.Height - 1;

                for (int y = 0; y < nHeight; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {
                        //  | p0 |  p1  |
                        //  |    |  p2  |
                        var p0 = ToGray(pSrc);
                        var p1 = ToGray(pSrc + 3);
                        var p2 = ToGray(pSrc + 3 + stride);

                        if (Math.Abs(p1 - p2) + Math.Abs(p1 - p0) > threshold)
                            p[0] = p[1] = p[2] = 255;
                        else
                            p[0] = p[1] = p[2] = 0;

                        p += 3;
                        pSrc += 3;
                    }
                    p += nOffset;
                    pSrc += nOffset;
                }
            }

            b.UnlockBits(bmData);
            bSrc.UnlockBits(bmSrc);
        }

        static unsafe float ToGray(byte* bgr)
        {
            return bgr[2] * 0.3f + bgr[1] * 0.59f + bgr[0] * 0.11f;
        }

        /// <summary>
        /// Изменение размера изображения
        /// </summary>
        public static Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (var graph = Graphics.FromImage(destImage))
            {
                graph.CompositingMode = CompositingMode.SourceCopy;
                graph.CompositingQuality = CompositingQuality.HighQuality;
                graph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graph.SmoothingMode = SmoothingMode.HighQuality;
                graph.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graph.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        /// <summary>
        /// Разрезание капчи на символы с последующим сохранением изображений в папку "temp" и выводом координат в текстбокс
        /// </summary>
        private void cutCaptcha()
        {
            Bitmap colorimage = null;
            OpenFileDialog _openfile = new OpenFileDialog();
            _openfile.InitialDirectory = System.IO.Path.Combine(Application.StartupPath, @"test");
            if (_openfile.ShowDialog() == DialogResult.OK)
            {
                Image<Bgr, Byte> myImage = new Image<Bgr, byte>(_openfile.FileName);
                colorimage = myImage.ToBitmap();
            }
            pictureBox5.Image = colorimage;
            FindCountours processor = new FindCountours();
            Bitmap color;
            Bitmap gray;
            int value = Convert.ToInt32(textBox1.Text);
            List<int> x = new List<int>();
            List<int> y = new List<int>();
            List<int> w = new List<int>();
            List<int> h = new List<int>();
            //метод из EmguCV. На выходе получаем координаты (X, Y) всех контуров и их высоты
            //3й параметр отвечает за инвертирование
            processor.IdentifyContours(colorimage, value, 5, 15, true, out gray, out color, out x, out y, out w, out h);
            pictureBox2.Image = color;
            pictureBox1.Image = gray;
            textBox3.Text = "";
            textBox4.Text = "";
            int count = 0;
            for (int i = 0; i < x.Count; i++)
            {
                textBox3.Text += i.ToString() + ": (" + x[i].ToString() + " " + y[i].ToString() + ") Ширина: " +
                                 w[i].ToString() + "; Высота: " + h[i].ToString() + Environment.NewLine;
                if ((w[i] > 6) && (h[i] > 10))
                    count++;
            }
            int line = 0, miny = 0;
            for (int i = 0; i < x.Count; i++)
                for (int j = i + 1; j < x.Count; j++)
                {
                    if ((w[i] > 6) && (h[i] > 9) && (w[j] > 6) && (h[j] > 9) && ((y[i] + h[i]) > h[j]) && (miny < y[i]))
                    {
                        line++;
                        miny = y[i] + h[i] + 5;
                        textBox3.Text += i.ToString() + "; ";
                    }
                }

            if (count < 6)
                textBox4.Text += "Мало символов/много нахлёстов" + Environment.NewLine;
            else
                textBox4.Text += "Много отдельно стоящих символов" + Environment.NewLine;
            //textBox4.Text += "Кол-во строк: " + line + Environment.NewLine;

            #region определение логотипа яндекс
            //bool lable = false;
            //for (int i = 0; i < x.Count; i++)
            //    if ((x[i] > 200) && (y[i] < 40) && (h[i] > 15))
            //    {
            //        lable = true;
            //        break;
            //    }
            //if (lable)
            //    textBox4.Text += "Справа вверху логотип";
            //else
            //    textBox4.Text += "Логотипа нет";
            #endregion

            //подсчет незанятой площади
            int pixelTaken = 0;
            for (int i = 0; i < x.Count; i++)
                pixelTaken += w[i] * h[i];
            textBox5.Text = (colorimage.Height * colorimage.Width - pixelTaken).ToString();
            List<SymRows> rows = new List<SymRows>();
            Height = 100;
            Width = 240;
            RowCount = 3;
            for (int i = 1; i <= RowCount; i++)
            {
                SymRows _row = new SymRows();
                int minY = Height * (i - 1) / RowCount;
                int maxY = Height * (i) / RowCount;
                for (int j = 0; j < x.Count; j++)
                {
                    if ((y[j] >= minY) && ((y[j] + h[j]) <= maxY))
                    {
                        _row.AddSym(x[j], y[j], w[j], h[j]);
                    }
                }
                RowSortByHeight(_row.X, _row.Y, _row.W, _row.H); //сортируем строку по убыванию высоты контура
                rows.Add(_row);
            }

            //упорядочивание элементов в строках, по возрастанию координаты Х
            for (int i = 1; i <= RowCount; i++)
                RowSortByX(rows[i - 1].X, rows[i - 1].Y, rows[i - 1].W, rows[i - 1].H);


            //очистка папки "temp" с символами
            DirectoryInfo dirInfo = new DirectoryInfo(System.IO.Path.Combine(Application.StartupPath, @"temp"));
            foreach (FileInfo file in dirInfo.GetFiles())
            {
                file.Delete();
            }
            //
            for (int i = 1; i <= RowCount; i++)
            {
                for (int q = 0; q < rows[i - 1].X.Count; q++)
                {
                    //TesseractEngine ocr = new TesseractEngine(AppDomain.CurrentDomain.BaseDirectory, "rus");
                    // ocr.SetVariable("tessedit_char_whitelist", "тдчпшсвд"); 
                    Bitmap tmp = CopyBitmap(colorimage,
                        new Rectangle(rows[i - 1].X[q], rows[i - 1].Y[q], rows[i - 1].W[q], rows[i - 1].H[q]));
                    tmp = ResizeImage(tmp, 20, 20);
                    string tmpPath = System.IO.Path.Combine(Application.StartupPath, @"temp\" + i +
                                                                                     "_" + q +
                                                                                     ".gif");
                    tmp.Save(tmpPath);
                    tmp.Dispose();
                    //  ocr.DefaultPageSegMode = PageSegMode.Auto;
                    //  textBox3.Text += i + "_" + q + ":" + page.GetText().Trim() + Environment.NewLine;
                    //  ocr.Dispose();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            cutCaptcha();
        }

        /// <summary>
        /// Сортировка контуров
        /// </summary>
        /// <param name="x">Х</param>
        /// <param name="y">Y</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        private void RowSortByHeight(List<int> x, List<int> y, List<int> w, List<int> h)
        {
            for (int i = 0; i < x.Count; i++)
            {
                for (int j = 0; j < x.Count - 1; j++)
                {
                    if (h[j] < h[j + 1]) //сортировка по убыванию высоты контуров
                    {
                        int z = y[j];
                        y[j] = y[j + 1];
                        y[j + 1] = z;

                        z = x[j];
                        x[j] = x[j + 1];
                        x[j + 1] = z;

                        z = w[j];
                        w[j] = w[j + 1];
                        w[j + 1] = z;

                        z = h[j];
                        h[j] = h[j + 1];
                        h[j + 1] = z;
                    }
                }
            }
        }

        /// <summary>
        /// Упорядочивание по координате Х, т.к. текст идёт слева - направо
        /// </summary>
        private void RowSortByX(List<int> x, List<int> y, List<int> w, List<int> h)
        {
            for (int i = 0; i < x.Count; i++)
            {
                for (int j = 0; j < x.Count - 1; j++)
                {
                    if (x[j] > x[j + 1]) //сортировка по возрастанию координаты X
                    {
                        int z = y[j];
                        y[j] = y[j + 1];
                        y[j + 1] = z;

                        z = x[j];
                        x[j] = x[j + 1];
                        x[j + 1] = z;

                        z = w[j];
                        w[j] = w[j + 1];
                        w[j + 1] = z;

                        z = h[j];
                        h[j] = h[j + 1];
                        h[j + 1] = z;
                    }
                }
            }
        }

        /// <summary>
        /// Вспомогательный метод для вырезки части изображения
        /// </summary>
        /// <param name="source"></param>
        /// <param name="part"></param>
        /// <returns></returns>
        protected Bitmap CopyBitmap(Bitmap source, Rectangle part)
        {
            Bitmap bmp = new Bitmap(part.Width, part.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(source, 0, 0, part, GraphicsUnit.Pixel);
            g.Dispose();
            return bmp;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            button2_Click(sender, e);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            Sobel();
        }

        /// <summary>
        /// Собель
        /// </summary>
        private void Sobel()
        {
            Bitmap b = new Bitmap(pictureBox1.Image);
            Bitmap bb = new Bitmap(pictureBox1.Image);
            int width = b.Width;
            int height = b.Height;
            int[,] gx = new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] gy = new int[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };

            int[,] allPixR = new int[width, height];
            int[,] allPixG = new int[width, height];
            int[,] allPixB = new int[width, height];

            int limit = 128 * 128;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    allPixR[i, j] = b.GetPixel(i, j).R;
                    allPixG[i, j] = b.GetPixel(i, j).G;
                    allPixB[i, j] = b.GetPixel(i, j).B;
                }
            }

            int new_rx = 0, new_ry = 0;
            int new_gx = 0, new_gy = 0;
            int new_bx = 0, new_by = 0;
            int rc, gc, bc;
            for (int i = 1; i < b.Width - 1; i++)
            {
                for (int j = 1; j < b.Height - 1; j++)
                {

                    new_rx = 0;
                    new_ry = 0;
                    new_gx = 0;
                    new_gy = 0;
                    new_bx = 0;
                    new_by = 0;
                    rc = 0;
                    gc = 0;
                    bc = 0;

                    for (int wi = -1; wi < 2; wi++)
                    {
                        for (int hw = -1; hw < 2; hw++)
                        {
                            rc = allPixR[i + hw, j + wi];
                            new_rx += gx[wi + 1, hw + 1] * rc;
                            new_ry += gy[wi + 1, hw + 1] * rc;

                            gc = allPixG[i + hw, j + wi];
                            new_gx += gx[wi + 1, hw + 1] * gc;
                            new_gy += gy[wi + 1, hw + 1] * gc;

                            bc = allPixB[i + hw, j + wi];
                            new_bx += gx[wi + 1, hw + 1] * bc;
                            new_by += gy[wi + 1, hw + 1] * bc;
                        }
                    }
                    if (new_rx * new_rx + new_ry * new_ry > limit || new_gx * new_gx + new_gy * new_gy > limit ||
                        new_bx * new_bx + new_by * new_by > limit)
                        bb.SetPixel(i, j, Color.Black);
                    else
                        bb.SetPixel(i, j, Color.White);
                }
            }
            pictureBox3.Image = bb;
        }

        /// <summary>
        /// Кэнни
        /// </summary>
        private void Canny()
        {
            Bitmap b = new Bitmap(pictureBox1.Image);
            int width = b.Width;
            int height = b.Height;

            Bitmap n = new Bitmap(width, height);

            int[,] allPixR = new int[width, height];
            int[,] allPixG = new int[width, height];
            int[,] allPixB = new int[width, height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    allPixR[i, j] = b.GetPixel(i, j).R;
                    allPixG[i, j] = b.GetPixel(i, j).G;
                    allPixB[i, j] = b.GetPixel(i, j).B;
                }
            }
            for (int i = 2; i < b.Width - 2; i++)
            {
                for (int j = 2; j < b.Height - 2; j++)
                {
                    int red = (
                        ((allPixR[i - 2, j - 2]) * 1 + (allPixR[i - 1, j - 2]) * 4 + (allPixR[i, j - 2]) * 7 +
                         (allPixR[i + 1, j - 2]) * 4 + (allPixR[i + 2, j - 2])
                         + (allPixR[i - 2, j - 1]) * 4 + (allPixR[i - 1, j - 1]) * 16 + (allPixR[i, j - 1]) * 26 +
                         (allPixR[i + 1, j - 1]) * 16 + (allPixR[i + 2, j - 1]) * 4
                         + (allPixR[i - 2, j]) * 7 + (allPixR[i - 1, j]) * 26 + (allPixR[i, j]) * 41 + (allPixR[i + 1, j]) * 26 +
                         (allPixR[i + 2, j]) * 7
                         + (allPixR[i - 2, j + 1]) * 4 + (allPixR[i - 1, j + 1]) * 16 + (allPixR[i, j + 1]) * 26 +
                         (allPixR[i + 1, j + 1]) * 16 + (allPixR[i + 2, j + 1]) * 4
                         + (allPixR[i - 2, j + 2]) * 1 + (allPixR[i - 1, j + 2]) * 4 + (allPixR[i, j + 2]) * 7 +
                         (allPixR[i + 1, j + 2]) * 4 + (allPixR[i + 2, j + 2]) * 1) / 273
                    );

                    int green = (
                        ((allPixG[i - 2, j - 2]) * 1 + (allPixG[i - 1, j - 2]) * 4 + (allPixG[i, j - 2]) * 7 +
                         (allPixG[i + 1, j - 2]) * 4 + (allPixG[i + 2, j - 2])
                         + (allPixG[i - 2, j - 1]) * 4 + (allPixG[i - 1, j - 1]) * 16 + (allPixG[i, j - 1]) * 26 +
                         (allPixG[i + 1, j - 1]) * 16 + (allPixG[i + 2, j - 1]) * 4
                         + (allPixG[i - 2, j]) * 7 + (allPixG[i - 1, j]) * 26 + (allPixG[i, j]) * 41 + (allPixG[i + 1, j]) * 26 +
                         (allPixG[i + 2, j]) * 7
                         + (allPixG[i - 2, j + 1]) * 4 + (allPixG[i - 1, j + 1]) * 16 + (allPixG[i, j + 1]) * 26 +
                         (allPixG[i + 1, j + 1]) * 16 + (allPixG[i + 2, j + 1]) * 4
                         + (allPixG[i - 2, j + 2]) * 1 + (allPixG[i - 1, j + 2]) * 4 + (allPixG[i, j + 2]) * 7 +
                         (allPixG[i + 1, j + 2]) * 4 + (allPixG[i + 2, j + 2]) * 1) / 273
                    );

                    int blue = (
                        ((allPixB[i - 2, j - 2]) * 1 + (allPixB[i - 1, j - 2]) * 4 + (allPixB[i, j - 2]) * 7 +
                         (allPixB[i + 1, j - 2]) * 4 + (allPixB[i + 2, j - 2])
                         + (allPixB[i - 2, j - 1]) * 4 + (allPixB[i - 1, j - 1]) * 16 + (allPixB[i, j - 1]) * 26 +
                         (allPixB[i + 1, j - 1]) * 16 + (allPixB[i + 2, j - 1]) * 4
                         + (allPixB[i - 2, j]) * 7 + (allPixB[i - 1, j]) * 26 + (allPixB[i, j]) * 41 + (allPixB[i + 1, j]) * 26 +
                         (allPixB[i + 2, j]) * 7
                         + (allPixB[i - 2, j + 1]) * 4 + (allPixB[i - 1, j + 1]) * 16 + (allPixB[i, j + 1]) * 26 +
                         (allPixB[i + 1, j + 1]) * 16 + (allPixB[i + 2, j + 1]) * 4
                         + (allPixB[i - 2, j + 2]) * 1 + (allPixB[i - 1, j + 2]) * 4 + (allPixB[i, j + 2]) * 7 +
                         (allPixB[i + 1, j + 2]) * 4 + (allPixB[i + 2, j + 2]) * 1) / 273
                    );
                    n.SetPixel(i, j, Color.FromArgb(red, green, blue));
                }
            }
            //pictureBox2.Image = n;//////////////////////////////////////////////////////here onward use n///////////////////////////////////////////////
            int[,] allPixRn = new int[width, height];
            int[,] allPixGn = new int[width, height];
            int[,] allPixBn = new int[width, height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    allPixRn[i, j] = n.GetPixel(i, j).R;
                    allPixGn[i, j] = n.GetPixel(i, j).G;
                    allPixBn[i, j] = n.GetPixel(i, j).B;
                }
            }


            int[,] gx = new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] gy = new int[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };
            int new_rx = 0, new_ry = 0;
            int new_gx = 0, new_gy = 0;
            int new_bx = 0, new_by = 0;
            int rc, gc, bc;
            int gradR, gradG, gradB;

            int[,] graidientR = new int[width, height];
            int[,] graidientG = new int[width, height];
            int[,] graidientB = new int[width, height];

            int atanR, atanG, atanB;

            int[,] tanR = new int[width, height];
            int[,] tanG = new int[width, height];
            int[,] tanB = new int[width, height];

            //int limit = 128 * 128;
            //Bitmap bb = new Bitmap (pictureBox1.Image);

            for (int i = 1; i < b.Width - 1; i++)
            {
                for (int j = 1; j < b.Height - 1; j++)
                {

                    new_rx = 0;
                    new_ry = 0;
                    new_gx = 0;
                    new_gy = 0;
                    new_bx = 0;
                    new_by = 0;
                    rc = 0;
                    gc = 0;
                    bc = 0;

                    for (int wi = -1; wi < 2; wi++)
                    {
                        for (int hw = -1; hw < 2; hw++)
                        {
                            rc = allPixRn[i + hw, j + wi];
                            new_rx += gx[wi + 1, hw + 1] * rc;
                            new_ry += gy[wi + 1, hw + 1] * rc;

                            gc = allPixGn[i + hw, j + wi];
                            new_gx += gx[wi + 1, hw + 1] * gc;
                            new_gy += gy[wi + 1, hw + 1] * gc;

                            bc = allPixBn[i + hw, j + wi];
                            new_bx += gx[wi + 1, hw + 1] * bc;
                            new_by += gy[wi + 1, hw + 1] * bc;
                        }
                    }

                    //find gradieant
                    gradR = (int)Math.Sqrt((new_rx * new_rx) + (new_ry * new_ry));
                    graidientR[i, j] = gradR;

                    gradG = (int)Math.Sqrt((new_gx * new_gx) + (new_gy * new_gy));
                    graidientG[i, j] = gradG;

                    gradB = (int)Math.Sqrt((new_bx * new_gx) + (new_by * new_by));
                    graidientB[i, j] = gradB;
                    //
                    //find tans
                    ////////////////tan red//////////////////////////////////
                    atanR = (int)((Math.Atan((double)new_ry / new_rx)) * (180 / Math.PI));
                    if ((atanR > 0 && atanR < 22.5) || (atanR > 157.5 && atanR < 180))
                    {
                        atanR = 0;
                    }
                    else if (atanR > 22.5 && atanR < 67.5)
                    {
                        atanR = 45;
                    }
                    else if (atanR > 67.5 && atanR < 112.5)
                    {
                        atanR = 90;
                    }
                    else if (atanR > 112.5 && atanR < 157.5)
                    {
                        atanR = 135;
                    }

                    if (atanR == 0)
                    {
                        tanR[i, j] = 0;
                    }
                    else if (atanR == 45)
                    {
                        tanR[i, j] = 1;
                    }
                    else if (atanR == 90)
                    {
                        tanR[i, j] = 2;
                    }
                    else if (atanR == 135)
                    {
                        tanR[i, j] = 3;
                    }
                    ////////////////tan red end//////////////////////////////////

                    ////////////////tan green//////////////////////////////////
                    atanG = (int)((Math.Atan((double)new_gy / new_gx)) * (180 / Math.PI));
                    if ((atanG > 0 && atanG < 22.5) || (atanG > 157.5 && atanG < 180))
                    {
                        atanG = 0;
                    }
                    else if (atanG > 22.5 && atanG < 67.5)
                    {
                        atanG = 45;
                    }
                    else if (atanG > 67.5 && atanG < 112.5)
                    {
                        atanG = 90;
                    }
                    else if (atanG > 112.5 && atanG < 157.5)
                    {
                        atanG = 135;
                    }


                    if (atanG == 0)
                    {
                        tanG[i, j] = 0;
                    }
                    else if (atanG == 45)
                    {
                        tanG[i, j] = 1;
                    }
                    else if (atanG == 90)
                    {
                        tanG[i, j] = 2;
                    }
                    else if (atanG == 135)
                    {
                        tanG[i, j] = 3;
                    }
                    ////////////////tan green end//////////////////////////////////


                    ////////////////tan blue//////////////////////////////////
                    atanB = (int)((Math.Atan((double)new_by / new_bx)) * (180 / Math.PI));
                    if ((atanB > 0 && atanB < 22.5) || (atanB > 157.5 && atanB < 180))
                    {
                        atanB = 0;
                    }
                    else if (atanB > 22.5 && atanB < 67.5)
                    {
                        atanB = 45;
                    }
                    else if (atanB > 67.5 && atanB < 112.5)
                    {
                        atanB = 90;
                    }
                    else if (atanB > 112.5 && atanB < 157.5)
                    {
                        atanB = 135;
                    }

                    if (atanB == 0)
                    {
                        tanB[i, j] = 0;
                    }
                    else if (atanB == 45)
                    {
                        tanB[i, j] = 1;
                    }
                    else if (atanB == 90)
                    {
                        tanB[i, j] = 2;
                    }
                    else if (atanB == 135)
                    {
                        tanB[i, j] = 3;
                    }
                    ////////////////tan blue end//////////////////////////////////
                }
            }

            int[,] allPixRs = new int[width, height];
            int[,] allPixGs = new int[width, height];
            int[,] allPixBs = new int[width, height];

            for (int i = 2; i < width - 2; i++)
            {
                for (int j = 2; j < height - 2; j++)
                {

                    ////red
                    if (tanR[i, j] == 0)
                    {
                        if (graidientR[i - 1, j] < graidientR[i, j] && graidientR[i + 1, j] < graidientR[i, j])
                        {
                            allPixRs[i, j] = graidientR[i, j];
                        }
                        else
                        {
                            allPixRs[i, j] = 0;
                        }
                    }
                    if (tanR[i, j] == 1)
                    {
                        if (graidientR[i - 1, j + 1] < graidientR[i, j] && graidientR[i + 1, j - 1] < graidientR[i, j])
                        {
                            allPixRs[i, j] = graidientR[i, j];
                        }
                        else
                        {
                            allPixRs[i, j] = 0;
                        }
                    }
                    if (tanR[i, j] == 2)
                    {
                        if (graidientR[i, j - 1] < graidientR[i, j] && graidientR[i, j + 1] < graidientR[i, j])
                        {
                            allPixRs[i, j] = graidientR[i, j];
                        }
                        else
                        {
                            allPixRs[i, j] = 0;
                        }
                    }
                    if (tanR[i, j] == 3)
                    {
                        if (graidientR[i - 1, j - 1] < graidientR[i, j] && graidientR[i + 1, j + 1] < graidientR[i, j])
                        {
                            allPixRs[i, j] = graidientR[i, j];
                        }
                        else
                        {
                            allPixRs[i, j] = 0;
                        }
                    }

                    //green
                    if (tanG[i, j] == 0)
                    {
                        if (graidientG[i - 1, j] < graidientG[i, j] && graidientG[i + 1, j] < graidientG[i, j])
                        {
                            allPixGs[i, j] = graidientG[i, j];
                        }
                        else
                        {
                            allPixGs[i, j] = 0;
                        }
                    }
                    if (tanG[i, j] == 1)
                    {
                        if (graidientG[i - 1, j + 1] < graidientG[i, j] && graidientG[i + 1, j - 1] < graidientG[i, j])
                        {
                            allPixGs[i, j] = graidientG[i, j];
                        }
                        else
                        {
                            allPixGs[i, j] = 0;
                        }
                    }
                    if (tanG[i, j] == 2)
                    {
                        if (graidientG[i, j - 1] < graidientG[i, j] && graidientG[i, j + 1] < graidientG[i, j])
                        {
                            allPixGs[i, j] = graidientG[i, j];
                        }
                        else
                        {
                            allPixGs[i, j] = 0;
                        }
                    }
                    if (tanG[i, j] == 3)
                    {
                        if (graidientG[i - 1, j - 1] < graidientG[i, j] && graidientG[i + 1, j + 1] < graidientG[i, j])
                        {
                            allPixGs[i, j] = graidientG[i, j];
                        }
                        else
                        {
                            allPixGs[i, j] = 0;
                        }
                    }

                    //blue
                    if (tanB[i, j] == 0)
                    {
                        if (graidientB[i - 1, j] < graidientB[i, j] && graidientB[i + 1, j] < graidientB[i, j])
                        {
                            allPixBs[i, j] = graidientB[i, j];
                        }
                        else
                        {
                            allPixBs[i, j] = 0;
                        }
                    }
                    if (tanB[i, j] == 1)
                    {
                        if (graidientB[i - 1, j + 1] < graidientB[i, j] && graidientB[i + 1, j - 1] < graidientB[i, j])
                        {
                            allPixBs[i, j] = graidientB[i, j];
                        }
                        else
                        {
                            allPixBs[i, j] = 0;
                        }
                    }
                    if (tanB[i, j] == 2)
                    {
                        if (graidientB[i, j - 1] < graidientB[i, j] && graidientB[i, j + 1] < graidientB[i, j])
                        {
                            allPixBs[i, j] = graidientB[i, j];
                        }
                        else
                        {
                            allPixBs[i, j] = 0;
                        }
                    }
                    if (tanB[i, j] == 3)
                    {
                        if (graidientB[i - 1, j - 1] < graidientB[i, j] && graidientB[i + 1, j + 1] < graidientB[i, j])
                        {
                            allPixBs[i, j] = graidientB[i, j];
                        }
                        else
                        {
                            allPixBs[i, j] = 0;
                        }
                    }
                }
            }

            int threshold = Convert.ToInt16(textBox1.Text);
            int[,] allPixRf = new int[width, height];
            int[,] allPixGf = new int[width, height];
            int[,] allPixBf = new int[width, height];

            // Bitmap bb = new Bitmap (pictureBox1.Image);
            Bitmap bb = new Bitmap(width, height);

            for (int i = 2; i < width - 2; i++)
            {
                for (int j = 2; j < height - 2; j++)
                {
                    if (allPixRs[i, j] > threshold)
                    {
                        allPixRf[i, j] = 1;
                    }
                    else
                    {
                        allPixRf[i, j] = 0;
                    }

                    if (allPixGs[i, j] > threshold)
                    {
                        allPixGf[i, j] = 1;
                    }
                    else
                    {
                        allPixGf[i, j] = 0;
                    }

                    if (allPixBs[i, j] > threshold)
                    {
                        allPixBf[i, j] = 1;
                    }
                    else
                    {
                        allPixBf[i, j] = 0;
                    }



                    if (allPixRf[i, j] == 1 || allPixGf[i, j] == 1 || allPixBf[i, j] == 1)
                    {
                        bb.SetPixel(i, j, Color.Black);
                    }
                    else
                        bb.SetPixel(i, j, Color.White);
                }
            }
            pictureBox4.Image = bb;



        }

        private void button2_Click_2(object sender, EventArgs e)
        {
            Canny();
        }

        /// <summary>
        /// Размытие фильтром Гаусса
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openfile = new OpenFileDialog();
            if (openfile.ShowDialog() == DialogResult.OK)
            {
                Image<Bgr, Byte> myImage = new Image<Bgr, byte>(openfile.FileName);
                Bitmap image = null;
                pictureBox2.Image = myImage.ToBitmap();
                image = myImage.ToBitmap();
                GaussianBlur filter = new GaussianBlur(2, 10);
                filter.ApplyInPlace(image);
                pictureBox1.Image = image;
            }
        }

        /// <summary>
        /// Не используется. Метод для удаления логотипа Яндекс - капчи
        /// </summary>
        private void button4_Click(object sender, EventArgs e) //для яндекс
        {
            OpenFileDialog openfile = new OpenFileDialog();
            if (openfile.ShowDialog() == DialogResult.OK)
            {

                Bitmap b = new Bitmap(pictureBox2.Image);
                Bitmap backGround = new Bitmap(openfile.FileName);
                for (int i = 1; i < b.Width - 1; i++)
                {
                    for (int j = 1; j < b.Height - 1; j++)
                        if ((b.GetPixel(i, j).ToArgb() - backGround.GetPixel(i, j).ToArgb()) < 100)
                        {
                            b.SetPixel(i, j, Color.White);
                        }
                }
                pictureBox1.Image = b;
            }
        }

        /// <summary>
        /// Удаление пикселей, яркость которых ниже заданной
        /// </summary>
        private void button5_Click(object sender, EventArgs e) //для 4pda
        {
            OpenFileDialog openfile = new OpenFileDialog();
            if (openfile.ShowDialog() == DialogResult.OK)
            {

                Bitmap b = new Bitmap(pictureBox2.Image);
                Bitmap backGround = new Bitmap(openfile.FileName);
                for (int i = 1; i < b.Width - 1; i++)
                {
                    for (int j = 1; j < b.Height - 1; j++)
                        if ((b.GetPixel(i, j).ToArgb() - backGround.GetPixel(i, j).ToArgb()) < 100)
                        {
                            b.SetPixel(i, j, Color.White);
                        }
                }
                pictureBox1.Image = b;
            }
        }


        List<int> difference = new List<int>();

        /// <summary>
        /// Распознавание с Tesseract. Изображение разрезается на 3 строки.
        /// </summary>
        private void button6_Click(object sender, EventArgs e) //4pda
        {
            
            System.IO.DirectoryInfo dirInfo2 = new System.IO.DirectoryInfo(System.IO.Path.Combine(Application.StartupPath, @"test"));
            FileInfo[] files = dirInfo2.GetFiles("*" + format);
            int rowsCount = 3;
            OpenFileDialog openfile = new OpenFileDialog();
            if (openfile.ShowDialog() == DialogResult.OK)
            {
                Image<Bgr, Byte> myImage = new Image<Bgr, byte>(openfile.FileName);
                System.Drawing.Image img2 = myImage.ToBitmap();
                Bitmap rows = new Bitmap(img2);
                Bitmap outBitmap = new Bitmap(240 * rowsCount,
                    33); //новая картинка
                List<System.Drawing.Image> images = new List<System.Drawing.Image>();
                for (int i = 0; i <= rowsCount - 1; i++)
                {
                    images.Add(rows.Clone(new Rectangle(0, i * 33, 240, 33), rows.PixelFormat));
                }
                // применяем фильтр -бинаризация
                for (int j = 0; j < rowsCount; j++)
                {
                    try
                    {
                        FiltersSequence SEQLOC = new FiltersSequence();
                        SEQLOC.Add(Grayscale.CommonAlgorithms.BT709);
                        SEQLOC.Add(new OtsuThreshold());
                        images[j] = SEQLOC.Apply(new Bitmap(images[j]));
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(exc.Message);
                    }
                }
                firstRowPicture.Image = images[0];
                secondRowPicture.Image = images[1];
                thirdRowPicture.Image = images[2];
                //распознавание с Тессерактом
                try
                {
                    List<string> text = new List<string>();
                    TesseractEngine ocr = new TesseractEngine(AppDomain.CurrentDomain.BaseDirectory, "rus");
                    ocr.SetVariable("tessedit_char_whitelist", "авеинорстмычьцчя"); //путает т и м, ы, ч 
                    var page = ocr.Process(new Bitmap(images[0]));
                    ocr.DefaultPageSegMode = PageSegMode.Auto;
                    text.Add(textFixer(page.GetText().Trim()));
                    firstRowBox.Text = text[text.Count - 1];
                    ocr = new TesseractEngine(AppDomain.CurrentDomain.BaseDirectory, "rus");
                    ocr.SetVariable("tessedit_char_whitelist", "авеинорстмычьцчя"); //убран один символ. +убрана п, и
                    page = ocr.Process(new Bitmap(images[1]));
                    ocr.DefaultPageSegMode = PageSegMode.Auto;
                    text.Add(textFixer(page.GetText().Trim()));
                    secondRowBox.Text = text[text.Count - 1];

                    ocr = new TesseractEngine(AppDomain.CurrentDomain.BaseDirectory, "rus");
                    ocr.SetVariable("tessedit_char_whitelist", "авеинорстмычьцчя");
                    page = ocr.Process(new Bitmap(images[2]));
                    ocr.DefaultPageSegMode = PageSegMode.Auto;
                    text.Add(textFixer(page.GetText().Trim()));
                    thirdRowBox.Text = text[text.Count - 1];


                    textBox8.Text = EditDistance(text[0] + text[1] + text[2],
                            numberDictionary[Convert.ToInt32(Path.GetFileNameWithoutExtension(openfile.FileName))])
                        .ToString();
                    int answer = 1000;
                    int minDiff = 50;
                    for (int i = 1000; i <= 5899; i++)
                    {
                        if (EditDistance(text[0] + text[1] + text[2], numberDictionary[i]) < minDiff)
                        {
                            minDiff = EditDistance(text[0] + text[1] + text[2], i.ToString());
                            answer = i;
                            textBox8.Text = answer.ToString();
                        }

                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.ToString());
                }
                #region //прогон целой картинки, без разделения

                //TesseractEngine ocr2 = new TesseractEngine(AppDomain.CurrentDomain.BaseDirectory, "rus");
                //ocr2.SetVariable("tessedit_char_whitelist", "авдеинопрстьцчыя");//путает т и м, 
                //var page2 = ocr2.Process(rows);
                //ocr2.DefaultPageSegMode = PageSegMode.Auto;
                //string text2 = page2.GetText().Trim();
                //text2 = textFixer(text2);
                //textBox8.Text = text2;

                //foreach (tessnet2.Word word in result)
                //{
                //    firstRowBox.Text += String.Format("{0} : {1}", word.Confidence, word.Text);
                //}

                #endregion
            }

        }

        private string textFixer(string inText) //исправление возможных недостатков распознавания. Применялось для Тессеракта
        {
            inText = Regex.Replace(inText, "тт", "п");
            inText = Regex.Replace(inText, " ", "");
            return inText;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = System.IO.Path.Combine(Application.StartupPath, @"temp");
            if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            { return; }
            Image<Bgr, Byte> temp =
                new Image<Bgr, byte>(ofd.FileName);
            MessageBox.Show(simpleCompare(temp.ToBitmap()).ToString());
        }

        /// <summary>
        /// Редакционное расстояние Левенштейна
        /// </summary>
        public static int EditDistance(string s, string t)//s-полученное значение, t-значение из словаря
        {
            int edit = 100;
            string startString = t;
            int m = s.Length, n = t.Length;
            int[,] ed = new int[m, n];
            for (int i = 0; i < m; ++i)
                ed[i, 0] = i + 1;
            for (int j = 0; j < n; ++j)
                ed[0, j] = j + 1;
            for (int j = 1; j < n; ++j)
            {
                for (int i = 1; i < m; ++i)
                {
                    if (s[i] == t[j])
                    {
                        // Операция не требуется
                        ed[i, j] = ed[i - 1, j - 1];
                    }
                    else
                    {
                        // Минимум между удалением, вставкой и заменой
                        ed[i, j] = Math.Min(ed[i - 1, j] + 1,
                            Math.Min(ed[i, j - 1] + 1, ed[i - 1, j - 1] + 1));
                    }
                }
            }
            if (ed[m - 1, n - 1] < edit)//??? <=
                edit = ed[m - 1, n - 1];
            ///////
            foreach (string tmp in syms)
            {
                t = startString;
                t.Replace(tmp, tmp[0].ToString());
                m = s.Length; n = t.Length;
                ed = new int[m, n];
                for (int i = 0; i < m; ++i)
                    ed[i, 0] = i + 1;
                for (int j = 0; j < n; ++j)
                    ed[0, j] = j + 1;
                for (int j = 1; j < n; ++j)
                {
                    for (int i = 1; i < m; ++i)
                    {
                        if (s[i] == t[j])
                        {
                            // Операция не требуется
                            ed[i, j] = ed[i - 1, j - 1];
                        }
                        else
                        {
                            // Минимум между удалением, вставкой и заменой
                            ed[i, j] = Math.Min(ed[i - 1, j] + 1,
                                Math.Min(ed[i, j - 1] + 1, ed[i - 1, j - 1] + 1));
                        }
                    }
                }
                if (ed[m - 1, n - 1] < edit)//??? <=
                    edit = ed[m - 1, n - 1];
            }
            ////
            return edit;
        }

        /// <summary>
        /// Создание НС
        /// </summary>
        void cnn_CreateNNok(object sender, ParamsNN e)
        {
            ActivationFunction af = e.AFtype;
            int size = e.Size;
            int[] layers = e.Layers;

            CreateNW(af, size, layers);
        }

        public void CreateNW(ActivationFunction af, int SizeX, int[] Layers)
        {
            IdVal = new Dictionary<int, string>();
            NET = new NeuralNW(af, SizeX, Layers);
            //for (int i = 0; i < Layers.Count() - 1; i++)
            //{
            //    richTextBoxLogs.AppendText("Нейронов в " + (i + 1).ToString() + " скрытом слое: " + (Layers[i]).ToString() + "\r\n");
            //}
        }

        /// <summary>
        /// Отображение информации о НС
        /// </summary>
        private void ShowInfoNET()
        {
            MessageBox.Show("Функция:" + NET.AFtype.ToString() + Environment.NewLine + "Кол-во слоев:" + (NET.CountLayers - 1).ToString()
                + Environment.NewLine + "Входов:" + NET.GetX.ToString() + Environment.NewLine + "Выходов:" + NET.GetY.ToString() + Environment.NewLine);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ShowInfoNET();
        }

        private bool trainLoaded = false;

        /// <summary>
        /// Открытие тренировочных данных
        /// </summary>
        private void buttonOpenTrain_Click(object sender, EventArgs e)
        {
            if (trainLoaded)
                TestFromFile();
            else
            {
                OpenNET();
                trainLoaded = true;
                buttonOpenTrain.Text = "Распознать символ";
            }
        }
        /////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Распознавание с помощью НС
        /// </summary>
        private void TestCaptcha()
        {
            textBox4.Text = "";
            textBox2.Text = "";
            string dir = System.IO.Path.Combine(Application.StartupPath, @"temp");
            var file_arr = Directory.GetFiles(dir, "*.gif").OrderBy(fn => fn);
            foreach (string fn in file_arr)
            {
                // inputdata seperated by space
                Bitmap bmp = new Bitmap(fn);
                var res = TestImgDo(bmp);
                bmp.Dispose();
                double[] X = new double[NET.GetX];
                double[] Y;
                for (int i = 0; i < NET.GetX; i++)
                {
                    X[i] = res[i];
                }
                NET.NetOUT(X, out Y);
                NNtextBox.Text += GetCharByMax(Y).ToString();
            }
            string _rec = NNtextBox.Text;
            int answer = 1000;
            int minDiff = 50;
            switch (_rec.ToUpper()[0])
            {
                case 'Т'://либо тысяча(1), либо три(3)
                    if (_rec[3] == 'ч' || _rec[4] == 'ч' || _rec[3] == 'Ч' || _rec[4] == 'Ч')
                    //||(_rec[3]=='ч')|| (_rec[4] == 'ч'))
                    {
                        textBox2.Text += "1";
                        ////
                        for (int i = 1001; i <= 1999; i++)
                        {
                            if (EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper()) < minDiff)
                            {
                                minDiff = EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper());
                                answer = i;
                                // textBox8.Text = answer.ToString();
                            }
                        }
                        /////
                    }
                    else
                    {
                        textBox2.Text += "3";
                        ////
                        for (int i = 3001; i <= 3999; i++)
                        {
                            if (EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper()) < minDiff)
                            {
                                minDiff = EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper());
                                answer = i;
                                // textBox8.Text = answer.ToString();
                            }
                        }
                        /////
                    }
                    break;
                case 'Д'://либо два(2), либо девять(9)
                    if ((_rec[2] == 'в') || (_rec[2] == 'В'))
                    {
                        textBox2.Text += "9";
                        ////
                        for (int i = 9001; i <= 9999; i++)
                        {
                            if (EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper()) < minDiff)
                            {
                                minDiff = EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper());
                                answer = i;
                                // textBox8.Text = answer.ToString();
                            }
                        }
                        /////
                    }
                    else
                    {
                        textBox2.Text += "2";
                        ////
                        for (int i = 2001; i <= 2999; i++)
                        {
                            if (EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper()) < minDiff)
                            {
                                minDiff = EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper());
                                answer = i;
                                // textBox8.Text = answer.ToString();
                            }
                        }
                        /////
                    }
                    break;
                case 'Ч':

                    textBox2.Text += "4";
                    ////
                    for (int i = 4001; i <= 4999; i++)
                    {
                        if (EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper()) < minDiff)
                        {
                            minDiff = EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper());
                            answer = i;
                            // textBox8.Text = answer.ToString();
                        }
                    }
                    /////
                    break;
                case 'П':
                    textBox2.Text += "5";
                    ////
                    for (int i = 5001; i <= 5999; i++)
                    {
                        if (EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper()) < minDiff)
                        {
                            minDiff = EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper());
                            answer = i;
                            // textBox8.Text = answer.ToString();
                        }
                    }
                    /////
                    break;
                case 'Ш':
                    ////
                    for (int i = 6001; i <= 6999; i++)
                    {
                        if (EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper()) < minDiff)
                        {
                            minDiff = EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper());
                            answer = i;
                            // textBox8.Text = answer.ToString();
                        }
                    }
                    /////
                    textBox2.Text += "6";
                    break;
                case 'С':
                    ////
                    for (int i = 7001; i <= 7999; i++)
                    {
                        if (EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper()) < minDiff)
                        {
                            minDiff = EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper());
                            answer = i;
                            // textBox8.Text = answer.ToString();
                        }
                    }
                    /////
                    textBox2.Text += "7";
                    break;
                case 'В':
                    ////
                    for (int i = 8001; i <= 8999; i++)
                    {
                        if (EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper()) < minDiff)
                        {
                            minDiff = EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper());
                            answer = i;
                            // textBox8.Text = answer.ToString();
                        }
                    }
                    /////
                    textBox2.Text += "8";
                    break;
            }
            recognizedCaptcha = answer.ToString();

            //вывод окна с результатом
            //MessageBox.Show(answer.ToString());
        }


        /// <summary>
        /// Отображение распознавания для конкретного символа
        /// </summary>
        private void TestFromFile()
        {
            if (NET == null)
            {
                MessageBox.Show("Необходимо создать сеть!");
                return;
            }

            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            System.Drawing.Image img = System.Drawing.Image.FromFile(ofd.FileName);
            TestBm = new Bitmap(img);
            var res = TestImgDo(TestBm);
            double[] X = new double[NET.GetX];
            double[] Y;
            for (int i = 0; i < NET.GetX; i++)
                X[i] = res[i];

            TestBm.Dispose();
            img.Dispose();
            NET.NetOUT(X, out Y);
            StringBuilder a = new StringBuilder();
            a.Append(ofd.FileName + Environment.NewLine);
            for (int i = 0; i < NET.GetY; i++)
            {
                var _temp = "";
                try
                {
                    _temp = IdVal[i];
                }
                catch (Exception)
                {
                    _temp = "q";
                }
                a.Append(string.Format("ID = {0,000}    Val = {1}    {2:F4}\r\n", i, _convert[i], Y[i]) + Environment.NewLine);
            }
            textBox4.Text += Environment.NewLine;
            textBox4.Text += GetCharByMax(Y).ToString();
            MessageBox.Show(a.ToString());
        }

        private void button9_Click(object sender, EventArgs e)
        {
            NNtextBox.Text = "";
            TestCaptcha();
        }

        #region генерация капчи
        /// <summary>
        /// Генерация капчи
        /// </summary>
        private void generateCaptcha_Click(object sender, EventArgs e)
        {
            WebBrowser browser = new WebBrowser();
            browser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(browser_DocumentCompleted);
            browser.Navigate("https://4pda.ru/forum/index.php?act=login&CODE=00&return=http:%2F%2F4pda.ru%2F");
        }
        void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            string reg = @"turing.4pda.ru/captcha/[a-z0-9]+.gif";
            string _url = "";
            string tmpCaptcha = "";
            WebBrowser browser = sender as WebBrowser;
            HtmlElementCollection imgCollection = browser.Document.GetElementsByTagName("img");
            WebClient webClient = new WebClient();
            _url = imgCollection[imgCollection.Count - 1].OuterHtml;
            foreach (Match m in Regex.Matches(_url, reg))
                tmpCaptcha = m.Value;
            byte[] bArr = (new WebClient()).DownloadData(@"https://" + tmpCaptcha);
            pictureBox5.Image = new Bitmap(new MemoryStream(bArr));
        }
        private void button10_Click(object sender, EventArgs e)//сохранить капчу
        {
            Bitmap _toSave = new Bitmap(pictureBox5.Image);
            _toSave.Save(System.IO.Path.Combine(Application.StartupPath, @"test\" + textBox6.Text + ".gif"));
        }
        #endregion

        /// <summary>
        /// Распознавание капчи целиком с помощью НС
        /// </summary>
        private void button11_Click(object sender, EventArgs e)
        {
            button2_Click(sender, e);
            button9_Click(sender, e);
            MessageBox.Show(recognizedCaptcha);
            #region прогон по тестам

            /*
                                    int allImages = 0;
                                    int rightImages = 0;

                                    var file_arr = Directory.GetFiles(System.IO.Path.Combine(Application.StartupPath, @"test\"), "*.gif").OrderBy(fn => fn);
                                    foreach (string fn in file_arr)
                                    {
                                        allImages++;
                                        //labelTest.Text = allImages.ToString();
                                        Bitmap bmp = new Bitmap(fn);
                                        pictureBox5.Image = bmp;
                                        button2_Click(sender, e);
                                        button9_Click(sender, e);
                                        if ((System.IO.Path.GetFileNameWithoutExtension(fn) == recognizedCaptcha))
                                        {
                                            rightImages++;
                                        }
                                        bmp.Dispose();
                                    }
                                    // labelTest.Text = rightImages.ToString();
                                    */

            #endregion

        }

        /// <summary>
        /// Простое сравнение
        /// </summary>
        private void button13_Click(object sender, EventArgs e)
        {

            int allImages = 0;
            int rightImages = 0;
            int answer = 1000;
            string _rec = "";
            //var file_arr = Directory.GetFiles(System.IO.Path.Combine(Application.StartupPath, @"test\"), "*.gif").OrderBy(fn => fn);
            // foreach (string fn in file_arr)
            {
                //allImages++;
                ////labelTest.Text = allImages.ToString();
                //Bitmap bmp = new Bitmap(fn);
                //pictureBox5.Image = bmp;
                Bitmap colorimage = null;

                button2_Click(sender, e);
                //bmp.Dispose();
                _rec = "";
                string dir = System.IO.Path.Combine(Application.StartupPath, @"temp");
                var _arr = Directory.GetFiles(dir, "*.gif").OrderBy(_fn => _fn);
                foreach (string _fn in _arr)
                {
                    Bitmap temp = new Bitmap(_fn);
                    _rec += simpleCompare(temp).ToString();
                    temp.Dispose();
                }
                //
                answer = 1000;
                int minDiff = 50;
                switch (_rec.ToUpper()[0])
                {
                    case 'Т'://либо тысяча(1), либо три(3)
                        if (_rec[3] == 'ч' || _rec[4] == 'ч' || _rec[3] == 'Ч' || _rec[4] == 'Ч')
                        //||(_rec[3]=='ч')|| (_rec[4] == 'ч'))
                        {
                            textBox2.Text += "1";
                            for (int i = 1001; i <= 1999; i++)
                            {
                                if (EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper()) < minDiff)
                                {
                                    minDiff = EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper());
                                    answer = i;
                                }
                            }
                        }
                        else
                        {
                            textBox2.Text += "3";
                            for (int i = 3001; i <= 3999; i++)
                            {
                                if (EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper()) < minDiff)
                                {
                                    minDiff = EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper());
                                    answer = i;
                                }
                            }
                        }
                        break;
                    case 'Д'://либо два(2), либо девять(9)
                        if ((_rec[2] == 'в') || (_rec[2] == 'В'))
                        {
                            textBox2.Text += "9";
                            for (int i = 9001; i <= 9999; i++)
                            {
                                if (EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper()) < minDiff)
                                {
                                    minDiff = EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper());
                                    answer = i;
                                }
                            }
                        }
                        else
                        {
                            textBox2.Text += "2";
                            for (int i = 2001; i <= 2999; i++)
                            {
                                if (EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper()) < minDiff)
                                {
                                    minDiff = EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper());
                                    answer = i;
                                }
                            }
                        }
                        break;
                    case 'Ч':

                        textBox2.Text += "4";
                        for (int i = 4001; i <= 4999; i++)
                        {
                            if (EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper()) < minDiff)
                            {
                                minDiff = EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper());
                                answer = i;
                            }
                        }
                        break;
                    case 'П':
                        textBox2.Text += "5";
                        for (int i = 5001; i <= 5999; i++)
                        {
                            if (EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper()) < minDiff)
                            {
                                minDiff = EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper());
                                answer = i;
                            }
                        }
                        break;
                    case 'Ш':
                        for (int i = 6001; i <= 6999; i++)
                        {
                            if (EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper()) < minDiff)
                            {
                                minDiff = EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper());
                                answer = i;
                            }
                        }
                        textBox2.Text += "6";
                        break;
                    case 'С':
                        for (int i = 7001; i <= 7999; i++)
                        {
                            if (EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper()) < minDiff)
                            {
                                minDiff = EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper());
                                answer = i;
                            }
                        }
                        textBox2.Text += "7";
                        break;
                    case 'В':
                        for (int i = 8001; i <= 8999; i++)
                        {
                            if (EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper()) < minDiff)
                            {
                                minDiff = EditDistance(_rec.ToUpper(), numberDictionary[i].ToUpper());
                                answer = i;
                            }
                        }
                        textBox2.Text += "8";
                        break;
                }

                MessageBox.Show(answer.ToString());
                //if ((System.IO.Path.GetFileNameWithoutExtension(fn) == answer.ToString()))
                //{
                //    rightImages++;
                //}
            }
        }

        /// <summary>
        /// открытие папки и выбор файла с НС
        /// </summary>
        private void OpenNET()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "nns files (*.nns) | *.nns";
            ofd.InitialDirectory = System.IO.Path.Combine(Application.StartupPath, @"NeuralNetworks");
            if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            { return; }
            if (!System.IO.File.Exists(ofd.FileName))
            { return; }

            try
            {
                System.IO.FileStream fs = new System.IO.FileStream(ofd.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                NET =
                   (NeuralNW)bf.Deserialize(fs);
                fs.Close();

                ShowInfoNET();
                IdVal = NET.IdVal;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        /// <summary>
        /// Перевод битмапа в массив битов
        /// </summary>
        double[] TestImgDo(Bitmap bmp)
        {
            int W = bmp.Width;
            int H = bmp.Height;

            int N = W * H;
            double val = 0;
            double[] mas = new double[N];

            for (int j = 0, k = 0; j < H; j++)
            {
                for (int i = 0; i < W; i++)
                {
                    val = 0.3 * bmp.GetPixel(i, j).R + 0.59 * bmp.GetPixel(i, j).G + 0.11 * bmp.GetPixel(i, j).B;

                    mas[k++] = val > 127 ? -0.5 : 0.5;
                }
            }
            bmp.Dispose();
            return mas;
        }
    }
}

