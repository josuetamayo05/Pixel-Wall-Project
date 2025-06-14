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

        public void Spawn(int x, int y)//coloca al wall en la posicion x,y del canvas
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
        public void DrawRectangle(int dirX, int dirY, int distance, int width, int height)
        {
            // Validar dirección (debe ser -1, 0 o 1)
            if (Math.Abs(dirX) > 1 || Math.Abs(dirY) > 1)
                throw new Exception("Las direcciones deben ser -1, 0 o 1");

            // Calcular posición central
            int centerX = X + dirX * distance;
            int centerY = Y + dirY * distance;

            // Calcular esquinas del rectángulo
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

            // Mover Wall-E al centro del rectángulo (opcional, según especificación)
            X = centerX;
            Y = centerY;
        }

        // Métodos auxiliares (mantener los existentes)
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

        public void DrawLine(int dirX, int dirY, int distance)
        {
            for (int i = 0; i < distance; i++)
            {
                X += dirX;
                Y += dirY;
                if (CurrentColor != Color.Transparent)
                {
                    // Dibuja en la posición actual (X,Y) de la cuadrícula
                    canvas.DrawPixel(X, Y, CurrentColor);
                }
            }
        }

        public void DrawCircle(int dirX, int dirY, int radius)
        {
            int centerX = X + dirX;
            int centerY = Y + dirY;

            // Validar bordes del canvas
            if (!canvas.IsWithinBounds(centerX - radius, centerY - radius) ||
                !canvas.IsWithinBounds(centerX + radius, centerY + radius))
            {
                throw new Exception("El círculo excede los límites del canvas");
            }

            // Algoritmo mejorado de Bresenham para círculos
            int x = 0;
            int y = radius;
            int d = 3 - 2 * radius;

            DrawCirclePoints(centerX, centerY, x, y);

            while (y >= x)
            {
                x++;

                if (d > 0)
                {
                    y--;
                    d = d + 4 * (x - y) + 10;
                }
                else
                {
                    d = d + 4 * x + 6;
                }

                DrawCirclePoints(centerX, centerY, x, y);
            }

            // Mover Wall-E al centro del círculo
            X = centerX;
            Y = centerY;
        }

        private void DrawCirclePoints(int cx, int cy, int x, int y)
        {
            // Dibuja los 8 puntos simétricos con manejo de grosor
            if (BrushSize == 1)
            {
                // Versión precisa para pincel delgado
                canvas.DrawPixel(cx + x, cy + y, CurrentColor);
                canvas.DrawPixel(cx - x, cy + y, CurrentColor);
                canvas.DrawPixel(cx + x, cy - y, CurrentColor);
                canvas.DrawPixel(cx - x, cy - y, CurrentColor);
                canvas.DrawPixel(cx + y, cy + x, CurrentColor);
                canvas.DrawPixel(cx - y, cy + x, CurrentColor);
                canvas.DrawPixel(cx + y, cy - x, CurrentColor);
                canvas.DrawPixel(cx - y, cy - x, CurrentColor);
            }
            else
            {
                // Versión para pincel grueso (dibuja pequeños cuadrados)
                int halfSize = BrushSize / 2;
                for (int i = -halfSize; i <= halfSize; i++)
                {
                    for (int j = -halfSize; j <= halfSize; j++)
                    {
                        canvas.DrawPixel(cx + x + i, cy + y + j, CurrentColor, 1);
                        canvas.DrawPixel(cx - x + i, cy + y + j, CurrentColor, 1);
                        canvas.DrawPixel(cx + x + i, cy - y + j, CurrentColor, 1);
                        canvas.DrawPixel(cx - x + i, cy - y + j, CurrentColor, 1);
                        canvas.DrawPixel(cx + y + i, cy + x + j, CurrentColor, 1);
                        canvas.DrawPixel(cx - y + i, cy + x + j, CurrentColor, 1);
                        canvas.DrawPixel(cx + y + i, cy - x + j, CurrentColor, 1);
                        canvas.DrawPixel(cx - y + i, cy - x + j, CurrentColor, 1);
                    }
                }
            }
        }

        public int GetColorCount(string colorName, int x1, int y1, int x2, int y2)
        {
            // Validar coordenadas primero
            if (!canvas.IsWithinBounds(x1, y1) || !canvas.IsWithinBounds(x2, y2))
                return 0;

            // Normalizar coordenadas (x1,y1 será esquina superior izquierda)
            int startX = Math.Min(x1, x2);
            int endX = Math.Max(x1, x2);
            int startY = Math.Min(y1, y2);
            int endY = Math.Max(y1, y2);

            Color targetColor;
            if (colorName.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
            {
                targetColor = Color.Transparent;
            }
            else
            {
                targetColor = Color.FromName(colorName);
                if (targetColor.ToArgb() == 0) // Color.FromName devuelve ARGB=0 para nombres inválidos
                    throw new Exception($"Color no válido: {colorName}");
            }

            int count = 0;
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    if (canvas.IsWithinBounds(x, y)) // Doble verificación por seguridad
            {
                        if (targetColor == Color.Transparent)
                        {
                            if (canvas.GetPixel(x, y) == Color.Transparent)
                                count++;
                        }
                        else if (canvas.GetPixel(x, y).ToArgb() == targetColor.ToArgb())
                        {
                            count++;
                        }
                    }
                }
            }
            return count;
        }

        public int IsBrushColor(string colorName)
        {
            return CurrentColor.Name.Equals(colorName, StringComparison.OrdinalIgnoreCase)? 1: 0;
        }
       
        public void Size(int k)
        {
            if (k <= 0)
            {
                throw new ArgumentOutOfRangeException("El tamaño del pincel debe ser mayor que 0");
            }
            BrushSize = k % 2 == 0 ? k - 1 : k;
        }

        public void Fill() //rell espac conexos
        {
            if (CurrentColor == Color.Transparent)
                throw new Exception("No se puede rellenar con color transparente");

            canvas.FloodFill(X, Y, CurrentColor);
        }
    }
}