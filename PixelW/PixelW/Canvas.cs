using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PixelW
{
    internal class Canvas
    {
        public int Size { get; }
        private Color[,] pixels;
        private int _zoomLevel = 1; // 1 = 100%, 2 = 200%, etc.
        private const int MaxZoom = 5;
        private const int MinZoom = 1;


        public int ZoomLevel
        {
            get => _zoomLevel;
            set => _zoomLevel = (value < MinZoom) ? MinZoom : (value > MaxZoom) ? MaxZoom : value;
        }

        public Canvas(int size)
        {
            Size = size;
            pixels = new Color[Size, Size];
            Clear();//inicializa en blanco
        }

        public void Clear()
        {
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                    pixels[x, y] = Color.White;
        }

        public Color GetPixel(int x, int y)
        {
            if (!IsWithinBounds(x, y))
                throw new IndexOutOfRangeException("Coordenadas fuera del canvas");
            return pixels[x, y];
        }

        public void DrawPixel(int x, int y, Color color, int brushSize = 1)
        {
            if (brushSize == 1)
            {
                if (IsWithinBounds(x, y)) pixels[x, y] = color;
                return;
            }
            //dibuja un pixel en la posicion x,y o un cuadrado alrededor de x,y si brushSize es mayor q 1
            int half = brushSize / 2;
            for (int i = x - half; i <= x + half; i++)
                for (int j = y - half; j <= y + half; j++)
                    if (IsWithinBounds(i, j)) pixels[i, j] = color;
        }
        public void FloodFill(int x, int y, Color newColor)
        {
            //rellena un area conexa del mismo color,
            Color targetColor = pixels[x, y];
            if (targetColor == newColor) return; //evit bucle inf

            Stack<(int, int)> pixelsToFill = new Stack<(int, int)>(); // usar pila para evitar recursion
            pixelsToFill.Push((x, y));

            while (pixelsToFill.Count > 0)
            {
                var (currentX, currentY) = pixelsToFill.Pop();//elimina y devuelve la parte superior de la pila, extrae y elimina laparte sup de la pila
                if (!IsWithinBounds(currentX, currentY) || pixels[currentX, currentY] != targetColor)
                    continue;

                pixels[currentX, currentY] = newColor;

                pixelsToFill.Push((currentX + 1, currentY));//agg pixel a la derecha
                pixelsToFill.Push((currentX - 1, currentY));//izq
                pixelsToFill.Push((currentX, currentY + 1));//abaj
                pixelsToFill.Push((currentX, currentY - 1));//arr
            }
        }


        public bool IsWithinBounds(int x, int y) => x >= 0 && x < Size && y >= 0 && y < Size;

        public Bitmap ToBitmap()
        {
            Bitmap bmp = new Bitmap(Size * ZoomLevel, Size * ZoomLevel);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);

                for (int x = 0; x < Size; x++)
                {
                    for (int y = 0; y < Size; y++)
                    {
                        if (pixels[x, y] != Color.White && pixels[x, y] != Color.Transparent)
                        {
                            g.FillRectangle(new SolidBrush(pixels[x, y]),
                                           x * ZoomLevel, y * ZoomLevel,
                                           ZoomLevel, ZoomLevel);
                        }
                    }
                }
            }
            return bmp;
        }
    }
}