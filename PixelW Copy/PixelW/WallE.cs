using System;
using System.Drawing;

namespace PixelW
{
    internal class WallE
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public Color CurrentColor { get; private set; } = Color.Black;
        public int BrushSize { get; private set; } = 1;

        private readonly Canvas canvas;
        public int GetActualX() => X;
        public int GetActualY() => Y;
        public int GetCanvasSize() => canvas.Size;

        public WallE(Canvas canvas)
        {
            this.canvas = canvas;
        }

        public void Spawn(int x, int y)
        {
            if (x < 0 || x >= canvas.Size || y < 0 || y >= canvas.Size)
                throw new ArgumentException("Posición fuera de los límites");
            X=x;
            Y=y;    
        }

        public void SetColor(string colorName)
        {
            switch (colorName)
            {
                case "Red":
                    CurrentColor = Color.Red;
                    break;
                case "Green":
                    CurrentColor = Color.Green;
                    break;
                case "Blue":
                    CurrentColor = Color.Blue;
                    break;
                case "Yellow":
                    CurrentColor = Color.Yellow;
                    break;
                case "Black":
                    CurrentColor = Color.Black;
                    break;
                case "White":
                    CurrentColor = Color.White;
                    break;
                case "Transparent":
                    CurrentColor = Color.Transparent;
                    break;
                case "Orange":
                    CurrentColor = Color.Orange;
                    break;
                case "Purple":
                    CurrentColor = Color.Purple;    
                    break;
                case "OrangeRed":
                    CurrentColor = Color.OrangeRed;
                    break;
                case "DarkBlue":
                    CurrentColor = Color.DarkBlue;
                    break;
                case "DarkRed":
                    CurrentColor = Color.DarkRed;
                    break;
                case "Gold":
                    CurrentColor = Color.Gold;
                    break;
                case "DarkGreen":
                    CurrentColor = Color.DarkGreen;
                    break;
                case "Firebrick":
                    CurrentColor = Color.Firebrick;
                    break;
                default:
                    throw new Exception($"Color no soportado: {colorName}");
            }
        }
        public void DrawRectangle(int offsetX, int offsetY, int width, int height)
        {
            int centerX = X + offsetX;
            int centerY = Y + offsetY;

            int startX = centerX - width / 2;
            int startY = centerY - height / 2;
            int endX = centerX + width / 2;
            int endY = centerY + height / 2;

            // Validar bordes
            if (!canvas.IsWithinBounds(startX, startY) || !canvas.IsWithinBounds(endX, endY))
                throw new Exception("El rectángulo excede los límites del canvas");

            // Dibujar los 4 lados
            DrawHorizontalLine(startX, endX, startY); // Lado superior
            DrawHorizontalLine(startX, endX, endY);   // Lado inferior
            DrawVerticalLine(startY, endY, startX);   // Lado izquierdo
            DrawVerticalLine(startY, endY, endX);     // Lado derecho
        }

        private void DrawHorizontalLine(int x1, int x2, int y)
        {
            for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
            {
                canvas.DrawPixel(x, y, CurrentColor, BrushSize);
            }
        }

        private void DrawVerticalLine(int y1, int y2, int x)
        {
            for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
            {
                canvas.DrawPixel(x, y, CurrentColor, BrushSize);
            }
        }

        public void DrawLine(int dirX, int dirY,int distance)
        {
            for(int i = 0;i<distance;i++)
            {
                X+= dirX;
                Y+= dirY;
                if(CurrentColor!=Color.Transparent)
                       canvas.DrawPixel(X, Y,CurrentColor,BrushSize);
            }
        }
        public void SetBrushSize(int size)
        {
            if (size < 1)
                throw new Exception("El tamaño debe ser mayor a 0");

            BrushSize = size % 2 == 0 ? size - 1 : size;
        }
        public void DrawCircle(int offsetX, int offsetY, int radius)
        {
            int centerX = X + offsetX;
            int centerY = Y + offsetY;

            if (!canvas.IsWithinBounds(centerX, centerY) ||
                !canvas.IsWithinBounds(centerX + radius, centerY + radius) ||
                !canvas.IsWithinBounds(centerX - radius, centerY - radius))
            {
                throw new Exception("El círculo excede los límites del canvas");
            }

            // Algoritmo del punto medio para círculos
            int x = radius;
            int y = 0;
            int err = 0;

            while (x >= y)
            {
                DrawCirclePoints(centerX, centerY, x, y);

                y += 1;
                err += 1 + 2 * y;
                if (2 * (err - x) + 1 > 0)
                {
                    x -= 1;
                    err += 1 - 2 * x;
                }
            }
        }

        private void DrawCirclePoints(int cx, int cy, int x, int y)
        {
            canvas.DrawPixel(cx + x, cy + y, CurrentColor, BrushSize);
            canvas.DrawPixel(cx - x, cy + y, CurrentColor, BrushSize);
            canvas.DrawPixel(cx + x, cy - y, CurrentColor, BrushSize);
            canvas.DrawPixel(cx - x, cy - y, CurrentColor, BrushSize);
            canvas.DrawPixel(cx + y, cy + x, CurrentColor, BrushSize);
            canvas.DrawPixel(cx - y, cy + x, CurrentColor, BrushSize);
            canvas.DrawPixel(cx + y, cy - x, CurrentColor, BrushSize);
            canvas.DrawPixel(cx - y, cy - x, CurrentColor, BrushSize);
        }

        public void Fill()
        {
            if (CurrentColor == Color.Transparent)
                throw new Exception("No se puede rellenar con color transparente");

            canvas.FloodFill(X, Y, CurrentColor);
        }
    }
}