using System.Collections.Generic;
using System.Drawing;

namespace PixelW
{
    internal class Canvas
    {
        public int Size { get; }
        private Color[,] pixels; //matriz de colores

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
        {//Form1 llama a este método para actualizar picCanvas.Image después de cada dibujo
            Bitmap bmp = new Bitmap(Size, Size); //crear btmp vacio de tama;o del canvas
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                    bmp.SetPixel(x, y, pixels[x, y]); //copiar cada pixel de la matriz al bitmap
            return bmp;
        }
    }
}