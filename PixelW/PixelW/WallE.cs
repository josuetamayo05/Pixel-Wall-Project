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

                y++;
                err += 1 + 2 * y;
                if (2 * (err - x) + 1 > 0) //Decide si debemos mover x hacia adentro para mantener la forma circular
                {
                    x--;
                    err += 1 - 2 * x;
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

        private void DrawCirclePoints(int cx, int cy, int x, int y)
        {//dibujo los 8 simetricos al centro cx,cy lo divido en 8 octantes
            canvas.DrawPixel(cx + x, cy + y, CurrentColor, BrushSize); //arriba derecha
            canvas.DrawPixel(cx - x, cy + y, CurrentColor, BrushSize);//abajo derecha
            canvas.DrawPixel(cx + x, cy - y, CurrentColor, BrushSize);//arriba izq
            canvas.DrawPixel(cx - x, cy - y, CurrentColor, BrushSize);//abajo izq
            canvas.DrawPixel(cx + y, cy + x, CurrentColor, BrushSize);//derech arriba
            canvas.DrawPixel(cx - y, cy + x, CurrentColor, BrushSize);//derec abaj
            canvas.DrawPixel(cx + y, cy - x, CurrentColor, BrushSize);//izq arr
            canvas.DrawPixel(cx - y, cy - x, CurrentColor, BrushSize);//izq abaj
        }

        public int IsBrushColor(string colorName)
        {
            return CurrentColor.Name.Equals(colorName, StringComparison.OrdinalIgnoreCase)? 1: 0;
        }

        public void Fill() //rell espac conexos
        {
            if (CurrentColor == Color.Transparent)
                throw new Exception("No se puede rellenar con color transparente");

            canvas.FloodFill(X, Y, CurrentColor);
        }
    }
}