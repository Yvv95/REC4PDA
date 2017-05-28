using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MainApp
{
    public class SymRows
    {
        public List<int> X = new List<int>();//начало прямоугольника(образа)
        public List<int> Y = new List<int>();//начало
        public List<int> W = new List<int>();//ширина
        public List<int> H = new List<int>();//высота

        public void AddSym(int _x, int _y, int _w, int _h)
        {
            this.X.Add(_x);
            this.Y.Add(_y);
            this.W.Add(_w);
            this.H.Add(_h);
        }
        public void AddRow(List<int> _x, List<int> _y, List<int> _w, List<int> _h)
        {
            this.X = _x;
            this.Y = _y;
            this.W = _w;
            this.H = _h;
        }
    }
}
