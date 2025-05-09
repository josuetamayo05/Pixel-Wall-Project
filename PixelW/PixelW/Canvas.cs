using System.Collections.Generic;
using System.Drawing;

namespace PixelW
{
    internal class Canvas
    {
        public int Size { get; }
        private Color[,] pixels;

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

        public void DrawPixel(int x, int y, Color color, int brushSize = 1)
        {
            if (brushSize == 1)
            {
                if (IsWithinBounds(x, y)) pixels[x, y] = color;
                return;
            }

            int half = brushSize / 2;
            for (int i = x - half; i <= x + half; i++)
                for (int j = y - half; j <= y + half; j++)
                    if (IsWithinBounds(i, j)) pixels[i, j] = color;
        }
        public void FloodFill(int x, int y, Color newColor)
        {
            Color targetColor = pixels[x, y];
            if (targetColor == newColor) return;

            Stack<(int, int)> pixelsToFill = new Stack<(int, int)>();
            pixelsToFill.Push((x, y));

            while (pixelsToFill.Count > 0)
            {
                var (currentX, currentY) = pixelsToFill.Pop();
                if (!IsWithinBounds(currentX, currentY) || pixels[currentX, currentY] != targetColor)
                    continue;

                pixels[currentX, currentY] = newColor;

                pixelsToFill.Push((currentX + 1, currentY));
                pixelsToFill.Push((currentX - 1, currentY));
                pixelsToFill.Push((currentX, currentY + 1));
                pixelsToFill.Push((currentX, currentY - 1));
            }
        }


        public bool IsWithinBounds(int x, int y) => x >= 0 && x < Size && y >= 0 && y < Size;

        public Bitmap ToBitmap()
        {
            Bitmap bmp = new Bitmap(Size, Size);
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                    bmp.SetPixel(x, y, pixels[x, y]);
            return bmp;
        }
    }
}