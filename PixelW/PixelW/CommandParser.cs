using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PixelW
{
    internal class CommandParser
    {
        private readonly WallE _robot;
        private readonly VariableManager variables;
        private int currentLineNumber;
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


        private Dictionary<string, int> _labels = new Dictionary<string, int>(); // Guarda línea de cada etiqueta

        private void ParseLabel(string line)
        {
            string labelName = line.TrimEnd(':');
            _labels[labelName] = currentLineNumber;
        }

        private void ParseGoTo(string line)
        {
            var parts = line.Split(new[] { '[', ']', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
                throw new Exception("Formato: `GoTo [label] (condición)`");

            string label = parts[1].Trim();
            string condition = parts[2].Trim();

            if (!_labels.ContainsKey(label))
                throw new Exception($"Etiqueta no encontrada: '{label}'");

            if (EvaluateCondition(condition))
                currentLineNumber = _labels[label] - 1; // -1 porque el for loop incrementará
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
            if (int.TryParse(expression, out int value))
            {
                variables.Assign(varName, value);
            }
            else
            {
                throw new Exception($"No se puede evaluar la expresión: {expression}");
            }
        }

        private bool IsValidVariableName(string name)
        {
            // Implementar validación según especificaciones
            // Debe contener solo letras, números y guiones, no empezar con número o guión
            return !string.IsNullOrEmpty(name) &&
                   !char.IsDigit(name[0]) &&
                   name.All(c => char.IsLetterOrDigit(c) || c == '-');
        }
        private int EvaluateExpression(string expr)
        {
            // Caso simple: número directo
            if (int.TryParse(expr, out int value))
                return value;

            // Caso variable existente
            if (variables.Exists(expr))
                return variables.GetValue(expr);

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
        public CommandParser(WallE robot, VariableManager _variables)
        {
            _robot = robot ?? throw new ArgumentNullException(nameof(robot));
           // _variables = variables ?? throw new ArgumentNullException(nameof(_variables));
        }

        public void Execute(string code)
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
            else if(line.StartsWith("Fill(") || line.Trim() == "Fill")
            {
                _robot.Fill();
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
            return variables.GetValue(expr);
        }
    }
}