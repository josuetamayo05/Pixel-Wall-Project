using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PixelW
{
    internal class CommandParser
    {
       

        private readonly WallE _robot;
        private readonly VariableManager _variables;
        private int currentLineNumber;
        public CommandParser(WallE robot, VariableManager variables)
        {
            _robot = robot ?? throw new ArgumentNullException(nameof(robot));
            _variables = variables ?? throw new ArgumentNullException(nameof(variables));
        }
        private int ParseFunctionCall(string line)
        {
            if (line.StartsWith("GetActualX()")) return _robot.GetActualX();
            if (line.StartsWith("GetActualY()")) return _robot.GetActualY();
            if (line.StartsWith("GetCanvasSize()")) return _robot.GetCanvasSize();
            if (line.StartsWith("IsBrushColor("))
            {
                var parts = line.TrimEnd(')').Split('(')[1].Split(',');
                string colorName = parts[0].Trim('"', ' ', '\'');
                return _robot.CurrentColor.Name.Equals(colorName, StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            }

            throw new Exception($"Función no reconocida: {line}");
        }


        public Dictionary<string, int> _labels = new Dictionary<string, int>(); // Guarda línea de cada etiqueta

        private void ParseLabel(string line)
        {
            string labelName = line.TrimEnd(':').Trim();
            if (string.IsNullOrWhiteSpace(labelName))
                throw new Exception("El nombre de etiqueta no puede estar vacío");

            _labels[labelName] = currentLineNumber;
        }

        private void ParseGoTo(string line)
        {
            // Elimina espacios innecesarios y verifica formato básico
            line = line.Trim();
            if (!line.StartsWith("GoTo", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Formato incorrecto: debe comenzar con 'GoTo'");

            // Usa expresiones regulares para mayor flexibilidad
            var match = System.Text.RegularExpressions.Regex.Match(
                line,
                @"GoTo\s*\[([^\]]+)\]\s*\(([^)]+)\)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (!match.Success)
                throw new Exception("Formato incorrecto. Uso: GoTo [etiqueta] (condición)");

            string label = match.Groups[1].Value.Trim();
            string condition = match.Groups[2].Value.Trim();

            if (!_labels.ContainsKey(label))
                throw new Exception($"Etiqueta no encontrada: '{label}'");

            if (EvaluateCondition(condition))
                currentLineNumber = _labels[label];
        }

        private bool EvaluateCondition(string condition)
        {
            var operators = new[] { "==", "!=", "<=", ">=", "<", ">" };
            foreach (var op in operators)
            {
                if (condition.Contains(op))
                {
                    var parts = condition.Split(new[] { op }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2)
                        throw new Exception($"Condición mal formada: {condition}");

                    int left = EvaluateExpression(parts[0].Trim());
                    int right = EvaluateExpression(parts[1].Trim());

                    switch (op)
                    {
                        case "==": return left == right;
                        case "!=": return left != right;
                        case "<": return left < right;
                        case "<=": return left <= right;
                        case ">": return left > right;
                        case ">=": return left >= right;
                    }
                }
            }
            throw new Exception($"Operador no soportado en condición: {condition}");
        }
        private void ParseVariableAssignment(string line)
        {
            var parts = line.Split(new[] { "<-" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                throw new Exception("Sintaxis incorrecta para asignación. Uso: variable <- valor");

            string varName = parts[0].Trim();
            string expression = parts[1].Trim();

            // Validar nombre de variable
            if (!IsValidVariableName(varName))
                throw new Exception($"Nombre de variable inválido: '{varName}'");

            // Evaluar expresión (versión simplificada)
            int value = EvaluateExpression(expression);
            _variables.Assign(varName, value);
        }

        private bool IsValidVariableName(string name)
        {
            // Implementar validación según especificaciones
            // Debe contener solo letras, números y guiones, no empezar con número o guión
            return !string.IsNullOrEmpty(name) &&
                   !char.IsDigit(name[0]) &&
                   name.All(c => char.IsLetterOrDigit(c) || c == '-' || c=='_');
        }
        private int EvaluateExpression(string expr)
        {
            //caso funcion nuevo
            if(expr.Contains("(") && expr.EndsWith(")"))
                return ParseFunctionCall(expr);         
            // Caso simple: número directo
            if (int.TryParse(expr, out int value))
                return value;

            // Caso variable existente
            if (_variables.Exists(expr))
                return _variables.GetValue(expr);

            // Operaciones básicas
            char[] ops = { '+', '-', '*', '/' };
            foreach (char op in ops)
            {
                if (expr.Contains(op.ToString()))
                {
                    var parts = expr.Split(op);
                    if (parts.Length != 2) continue;

                    int left = EvaluateExpression(parts[0].Trim());
                    int right = EvaluateExpression(parts[1].Trim());

                    switch (op)
                    {
                        case '+': return left + right;
                        case '-': return left - right;
                        case '*': return left * right;
                        case '/':
                            if (right == 0) throw new Exception("División por cero");
                            return left / right;
                        default:
                            throw new Exception($"Operador no soportado: {op}");
                    }
                }
            }

            throw new Exception($"Expresión no válida: '{expr}'");
        }
        

        public void Execute(string code)//analiza y ejecuta cada linea
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new Exception("El código no puede estar vacío");

            string[] lines = code.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            for (currentLineNumber = 0; currentLineNumber < lines.Length; currentLineNumber++)
            {
                try
                {
                    string line = lines[currentLineNumber].Trim();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    ProcessLine(line);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error en línea {currentLineNumber + 1}: {ex.Message}");
                }
            }
        }
        private int ParseGetColorCount(string line)
        {
            var parts = line.TrimEnd(')').Split('(')[1].Split(',');
            if (parts.Length != 5)
                throw new Exception("Sintaxis incorrecta para GetColorCount. Uso: GetColorCount(\"color\", x1, y1, x2, y2)");

            string colorName = parts[0].Trim('"', ' ', '\'');
            int x1 = int.Parse(parts[1].Trim());
            int y1 = int.Parse(parts[2].Trim());
            int x2 = int.Parse(parts[3].Trim());
            int y2 = int.Parse(parts[4].Trim());

            return _robot.GetColorCount(colorName, x1, y1, x2, y2);
        }

        private void ProcessLine(string line)
        {
            if(line.EndsWith(":"))
            {
                ParseLabel(line);
                return;
            }
            if (line.StartsWith("Spawn("))
            {
                ParseSpawnCommand(line);
            }
            else if (line.StartsWith("Color("))
            {
                ParseColorCommand(line);
            }
            else if (line.TrimStart().StartsWith("GoTo", StringComparison.OrdinalIgnoreCase))
            {
                ParseGoTo(line);
            }
            else if (line.EndsWith(":"))
            {
                ParseLabel(line);
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
            else if (line.StartsWith("GetColorCount("))
            {
                ParseGetColorCount(line);
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
            else if(line.StartsWith("Fill(") || line.Trim() == "Fill")
            {
                _robot.Fill();
            }
            else if (line.StartsWith("Spawn("))
            {
                ParseSpawnCommand(line);
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
        
        private int GetValueFromExpression(string expr)
        {
            if (int.TryParse(expr, out int value))
                return value;
            return _variables.GetValue(expr);
        }
    }
}