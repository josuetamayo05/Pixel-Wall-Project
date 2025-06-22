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

        public Canvas Clone()
        {
            var newCanvas = new Canvas(canvas.Size);
            newCanvas.ZoomLevel = canvas._zoomLevel;

            for (int x = 0; x < canvas.Size; x++)
            {
                for (int y = 0; y < canvas.Size; y++)
                {
                    newCanvas.pixels[x, y] = canvas.pixels[x, y];
                }
            }

            return newCanvas;
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

        public int IsCanvasColor(string color, int vertical, int horizontal)
        {
            int targetX = X + horizontal;
            int targetY = Y + vertical;

            if (!canvas.IsWithinBounds(targetX, targetY))
                return 0;

            Color targetColor = Color.FromName(color);
            return canvas.GetPixel(targetX, targetY).ToArgb() == targetColor.ToArgb() ? 1 : 0;
        }
        public void DrawRectangle(int dirX, int dirY, int distance, int width, int height)
        {
            if (Math.Abs(dirX) > 1 || Math.Abs(dirY) > 1)
                throw new Exception("Las direcciones deben ser -1, 0 o 1");

            int centerX = X + dirX * distance;
            int centerY = Y + dirY * distance;

            int startX = centerX - width / 2;
            int startY = centerY - height / 2;
            int endX = centerX + width / 2;
            int endY = centerY + height / 2;

            DrawHorizontalLine(startX, endX, startY); 
            DrawHorizontalLine(startX, endX, endY);   
            DrawVerticalLine(startY, endY, startX);   
            DrawVerticalLine(startY, endY, endX);     

            X = centerX;
            Y = centerY;
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
        private void SafeDrawPixel(int x, int y)
        {
            if (canvas.IsWithinBounds(x, y) && CurrentColor != Color.Transparent)
            {
                canvas.DrawPixel(x, y, CurrentColor, BrushSize);
            }
        }
        

        public void DrawLine(int dirX, int dirY, int distance)
        {
            if (!canvas.IsWithinBounds(X, Y))
                throw new Exception($"Posición inicial ({X}, {Y}) fuera del canvas");

            for (int i = 0; i < distance; i++)
            {
                X += dirX;
                Y += dirY;
                SafeDrawPixel(X, Y);
            }
        }

        public void DrawCircle(int dirX, int dirY, int radius)
        {
            int centerX = X + dirX;
            int centerY = Y + dirY;

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

            X = centerX;
            Y = centerY;
        }

        private void DrawCirclePoints(int cx, int cy, int x, int y)
        {
            SafeDrawPixel(cx + x, cy + y);
            SafeDrawPixel(cx - x, cy + y);
            SafeDrawPixel(cx + x, cy - y);
            SafeDrawPixel(cx - x, cy - y);
            SafeDrawPixel(cx + y, cy + x);
            SafeDrawPixel(cx - y, cy + x);
            SafeDrawPixel(cx + y, cy - x);
            SafeDrawPixel(cx - y, cy - x);
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

        public int IsBrushSize(int size)
        { 
            return this.BrushSize == size ? 1 : 0;
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