﻿using System;
using System.IO;
using System.Runtime.InteropServices;

namespace NeuralNetworks
{
    //для сериализации
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    internal class DataToByte
    {
        [FieldOffset(0)]
        public double vDouble;
        [FieldOffset(0)]
        public int vInt;
        [FieldOffset(0)]
        public byte b1;
        [FieldOffset(1)]
        public byte b2;
        [FieldOffset(2)]
        public byte b3;
        [FieldOffset(3)]
        public byte b4;
        [FieldOffset(4)]
        public byte b5;
        [FieldOffset(5)]
        public byte b6;
        [FieldOffset(6)]
        public byte b7;
        [FieldOffset(7)]
        public byte b8;
    }
}
