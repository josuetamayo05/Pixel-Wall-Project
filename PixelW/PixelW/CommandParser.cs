using System;
using System.Drawing;

namespace PixelW
{
    internal class CommandParser
    {
        private readonly WallE _robot;

        public CommandParser(WallE robot)
        {
            _robot = robot ?? throw new ArgumentNullException(nameof(robot));
        }

        public void Execute(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new Exception("El código no puede estar vacío");

            string[] lines = code.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i++)
            {
                try
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    ProcessLine(line);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error en línea {i + 1}: {ex.Message}");
                }
            }
        }

        private void ProcessLine(string line)
        {
            if (line.StartsWith("Spawn("))
            {
                ParseSpawnCommand(line);
            }
            else if (line.StartsWith("Color("))
            {
                ParseColorCommand(line);
            }
            else if (line.StartsWith("DrawLine("))
            {
                ParseDrawLineCommand(line);
            }
            else if (line.StartsWith("//") || line.StartsWith("#"))
            {
                // Comentario, ignorar
                return;
            }
            else if (line.Contains("<-"))
            {
                ParseVariableAssignment(line);
            }
            else if (line.StartsWith("BrushSize("))
            {
                ParseBrushSizeCommand(line);
            }
            else if (line.StartsWith("DrawCircle("))
            {
                ParseDrawCircleCommand(line);
            }
            else if (line.StartsWith("DrawRectangle("))
            {
                ParseDrawRectangleCommand(line);
            }
            else
            {
                throw new Exception($"Comando no reconocido: {line}");
            }
        }
        private void ParseDrawRectangleCommand(string line)
        {
            var parts = line.TrimEnd(')').Split('(')[1].Split(',');
            if (parts.Length != 6)
                throw new Exception("Sintaxis incorrecta para DrawRectangle. Uso: DrawRectangle(dirX, dirY, distance, width, height)");

            int dirX = int.Parse(parts[0].Trim());
            int dirY = int.Parse(parts[1].Trim());
            int distance = int.Parse(parts[2].Trim());
            int width = int.Parse(parts[3].Trim());
            int height = int.Parse(parts[4].Trim());

            // Calcular posición central
            int centerX = _robot.X + dirX * distance;
            int centerY = _robot.Y + dirY * distance;

            _robot.DrawRectangle(centerX - _robot.X, centerY - _robot.Y, width, height);
        }
        private void ParseDrawCircleCommand(string line)
        {
            var parts = line.TrimEnd(')').Split('(')[1].Split(',');
            if (parts.Length != 3)
                throw new Exception("Sintaxis incorrecta para DrawCircle. Uso: DrawCircle(dirX, dirY, radio)");

            int dirX = int.Parse(parts[0].Trim());
            int dirY = int.Parse(parts[1].Trim());
            int radius = int.Parse(parts[2].Trim());

            _robot.DrawCircle(dirX, dirY, radius);
        }
        private void ParseBrushSizeCommand(string line)
        {
            var parts = line.TrimEnd(')').Split('(');
            if (parts.Length != 2)
                throw new Exception("Sintaxis incorrecta para BrushSize. Uso: BrushSize(3)");

            int size = int.Parse(parts[1]);
            _robot.SetBrushSize(size);
        }
        

        private void ParseSpawnCommand(string line)
        {
            var parts = line.Split(new[] { '(', ',', ')' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
                throw new Exception("Sintaxis incorrecta para Spawn. Uso: Spawn(x, y)");

            int x = int.Parse(parts[1].Trim());
            int y = int.Parse(parts[2].Trim());
            _robot.Spawn(x, y);
        }

        private void ParseColorCommand(string line)
        {
            int start = line.IndexOf('(') + 1;
            int end = line.IndexOf(')');
            if (start <= 0 || end <= 0)
                throw new Exception("Sintaxis incorrecta para Color. Uso: Color(\"NombreColor\")");

            string colorName = line.Substring(start, end - start).Trim('"', ' ', '\'');
            _robot.SetColor(colorName);
        }

        private void ParseDrawLineCommand(string line)
        {
            var parts = line.Split(new[] { '(', ',', ')' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4)
                throw new Exception("Sintaxis incorrecta para DrawLine. Uso: DrawLine(dirX, dirY, distancia)");

            int dirX = int.Parse(parts[1].Trim());
            int dirY = int.Parse(parts[2].Trim());
            int distance = int.Parse(parts[3].Trim());

            if (Math.Abs(dirX) > 1 || Math.Abs(dirY) > 1)
                throw new Exception("Las direcciones deben ser -1, 0 o 1");

            _robot.DrawLine(dirX, dirY, distance);
        }

        private void ParseVariableAssignment(string line)
        {
            // Implementación básica de asignación de variables
            var parts = line.Split(new[] { "<-" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                throw new Exception("Sintaxis incorrecta para asignación. Uso: variable <- valor");

            string varName = parts[0].Trim();
            string expression = parts[1].Trim();

            // Aquí iría la lógica para evaluar expresiones matemáticas
            // Por ahora solo soportamos asignaciones directas de números
            if (int.TryParse(expression, out int value))
            {
                // Guardar en un diccionario de variables (implementar esto)
                throw new NotImplementedException("Asignación de variables no implementada completamente");
            }
            else
            {
                throw new Exception($"No se puede evaluar la expresión: {expression}");
            }
        }
    }
}