using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PixelW
{
    public partial class Canvas
    {
        public int Size { get; }
        private Color[,] pixels;
        private int _zoomLevel = 1; // 1 = 100%, 2 = 200%, etc.
        public const int MaxZoom = 32;
        public const int MinZoom = 4;


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
            int adjustSize=brushSize%2==0 ? brushSize -1: brushSize;
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
                if (ZoomLevel > 3) // Solo mostrar grid cuando el zoom es alto
                {
                    using (Pen gridPen = new Pen(Color.FromArgb(30, Color.Gray)))


                        for (int i = 0; i <= Size; i++)
                        {
                            {
                                g.DrawLine(gridPen, i * ZoomLevel, 0, i * ZoomLevel, Size * ZoomLevel);
                                g.DrawLine(gridPen, 0, i * ZoomLevel, Size * ZoomLevel, i * ZoomLevel);
                            }
                        }
                }
            
            g.Clear(Color.White);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

                for (int x = 0; x < Size; x++)
                {
                    for (int y = 0; y < Size; y++)
                    {
                        if (pixels[x, y] != Color.White && pixels[x, y] != Color.Transparent)
                        {
                            // Dibujar cada pixel con borde
                            g.FillRectangle(new SolidBrush(pixels[x, y]),
                                           x * ZoomLevel, y * ZoomLevel,
                                           ZoomLevel, ZoomLevel);
                            g.DrawRectangle(Pens.LightGray,
                                           x * ZoomLevel, y * ZoomLevel,
                                           ZoomLevel - 1, ZoomLevel - 1);
                        }
                    }
                }
            }
            return bmp;
        }
    }
}