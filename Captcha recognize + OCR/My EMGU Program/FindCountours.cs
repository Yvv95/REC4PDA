using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;

namespace MainApp
{
    class FindCountours
    {    
        public void IdentifyContours(Bitmap colorImage, int thresholdValue, int symW, int symH, bool invert, out Bitmap processedGray, out Bitmap processedColor, out List<int> X, out List<int> Y, out List<int> W, out List<int> H)
        {
            //перевод в оттенки серого
            Image<Gray, byte> grayImage = new Image<Gray, byte>(colorImage);
            Image<Bgr, byte> color = new Image<Bgr, byte>(colorImage);         
            //нормализация изображения
            grayImage = grayImage.ThresholdBinary(new Gray(thresholdValue), new Gray(255));
            if (invert)   
                grayImage._Not();
            X=new List<int>();
            Y = new List<int>();
            W = new List<int>();
            H = new List<int>();
            //непосредственно вычисление контуров
            using (MemStorage storage = new MemStorage())
            {
                for (Contour<Point> contours = grayImage.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_TREE, storage); contours != null; contours = contours.HNext)
                {
                    Contour<Point> currentContour = contours.ApproxPoly(contours.Perimeter * 0.015, storage);                        
                    //ограничения на размеры выделяемого контура
                    if ((currentContour.BoundingRectangle.Width > symW)&&(currentContour.BoundingRectangle.Height > symH))
                    {
                        CvInvoke.cvDrawContours(color, contours, new MCvScalar(255), new MCvScalar(255), -1, 2, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, new Point(0, 0));
                        color.Draw(currentContour.BoundingRectangle, new Bgr(0, 255, 0), 2);
                    }
                    X.Add(currentContour.MCvContour.rect.X);                              
                    Y.Add(currentContour.MCvContour.rect.Y);                  
                    W.Add(currentContour.MCvContour.rect.Width);                  
                    H.Add(currentContour.MCvContour.rect.Height);
                    //CvInvoke.cvSetImageROI(colorImage, rect);
                }
            }
            processedColor = color.ToBitmap();
            processedGray = grayImage.ToBitmap();//выходное изображение
        }
        /*
Метод аппроксимации:
CV_CHAIN_CODE – на выходе очерчивает контур в цепном коде Фримена [1]. Все другие методы выводят многоугольники;
CV_CHAIN_APPROX_NONE – переводит все точки с цепного кода в точки;
CV_CHAIN_APPROX_SIMPLE – сжимает горизонтальные, вертикальные, и диагональные доли;
CV_CHAIN_APPROX_TC89_L1, CV_CHAIN_APPROX_TC89_KCOS – применяет одну из разновидностей алгоритма апроксимации цепочки Teh-Chin.
CV_LINK_RUNS – использует полностью различный алгоритм поиска контура через соединение горизонтальных долей. Только CV_RETR_LIST режим поиска может использоваться с этим методом.
  
Режим
CV_RETR_EXTERNAL – находятся только критические внешние контуры;
CV_RETR_LIST – находятся все контуры, и помещает их в список
CV_RETR_CCOMP – находятся все контуры, и записывают их в иерархию с двумя уровнями: верхний уровень – внешние границы компонентов, второй уровень – границы отверстий
CV_RETR_TREE – находятся все контуры, и записывается полная иерархия вложенных контуров.       
         
         */

    }
}
