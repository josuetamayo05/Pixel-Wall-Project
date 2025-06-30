using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PixelW
{
    public partial class Canvas
    {
        public int Size { get; }
        public Color[,] pixels;
        public int _zoomLevel = 8; 
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
            Clear();
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
        public void FloodFill(int startX, int startY, Color targetColor, Color newColor)
        {
            if (targetColor == newColor) return;

            var queue = new Queue<(int x, int y)>();
            queue.Enqueue((startX, startY));

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();

                if (!IsWithinBounds(x, y))continue;
                if (pixels[x, y] != targetColor) continue;

                pixels[x, y] = newColor;

                queue.Enqueue((x + 1, y)); // der
                queue.Enqueue((x - 1, y)); // izqa
                queue.Enqueue((x, y + 1)); // abajo
                queue.Enqueue((x, y - 1)); // arriba
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