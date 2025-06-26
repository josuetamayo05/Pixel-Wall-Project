using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace PixelW
{
    internal partial class CommandParser
    {
       

        private readonly WallE _robot;
        private readonly VariableManager _variables;
        private int currentLineNumber;
        public CommandParser(WallE robot, VariableManager variables)
        {
            _robot = robot ?? throw new ArgumentNullException(nameof(robot));
            _variables = variables ?? throw new ArgumentNullException(nameof(variables));
        }
        public class ExecutionResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public int ErrorLine { get; set; }

            public static ExecutionResult SuccessResult() => new ExecutionResult { Success = true };
        }
        public class ParseResult
        {
            public bool Success { get; set; }
            public List<ErrorInfo> Errors { get; } = new List<ErrorInfo>();
        }
        public class ErrorInfo
        {
            public int LineNumber { get; set; }
            public string Message { get; set; }
            public ErrorType Type { get; set; }
            public string CodeSnippet {  get; set; }
        }

        public enum ErrorType
        {
            Syntactic,
            Semantic,
            Runtime
        }

        public Dictionary<string, int> _labels = new Dictionary<string, int>(); // Guarda línea de cada etiqueta

        
        private void ValidateSyntax(string line, int lineNumber,ParseResult result)
        {
            if (line.Count(c => c == '(') != line.Count(c => c == ')'))
            {
                result.Errors.Add(new ErrorInfo
                {
                    LineNumber = lineNumber,
                    Message = "Paréntesis no balanceados",
                    Type = ErrorType.Syntactic,
                    CodeSnippet = line
                });
            }

            if (line.StartsWith("Spawn") && !line.Contains(","))
            {
                result.Errors.Add(new ErrorInfo
                {
                    LineNumber = lineNumber,
                    Message = "Faltan parámetros en comando Spawn",
                    Type = ErrorType.Syntactic,
                    CodeSnippet = line
                });
            }

        }
        private void ValidateSemantics(string line, int lineNumber, ParseResult result)
        {
            if (line.TrimStart().StartsWith("GoTo", StringComparison.OrdinalIgnoreCase))
            {
                string label = ExtractLabel(line, lineNumber, result);

                if (label != null && !_labels.ContainsKey(label))
                {
                    result.Errors.Add(new ErrorInfo
                    {
                        LineNumber = lineNumber,
                        Message = $"Etiqueta '{label}' no definida",
                        Type = ErrorType.Semantic,
                        CodeSnippet = line
                    });
                }
            }
        }

        private string ExtractLabel(string goToLine, int lineNumber, ParseResult result)
        {
            try
            {
                // Usar expresión regular para extraer la etiqueta
                var match = System.Text.RegularExpressions.Regex.Match(
                    goToLine,
                    @"GoTo\s*\[([^\]]+)\]\s*\(([^)]+)\)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (!match.Success)
                    throw new Exception("Formato incorrecto para GoTo. Uso: GoTo [etiqueta] (condición)");

                string label = match.Groups[1].Value.Trim();

                // Validar nombre de etiqueta
                if (!IsValidLabelName(label))
                {
                    throw new Exception($"Nombre de etiqueta inválido: '{label}'");
                }

                return label;
            }
            catch (Exception ex)
            {
                result?.Errors.Add(new ErrorInfo
                {
                    LineNumber = lineNumber,
                    Message = ex.Message,
                    Type = ErrorType.Syntactic,
                    CodeSnippet = goToLine
                });
                return null;
            }
        }

        private bool IsValidLabelName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (char.IsDigit(name[0]) || name[0] == '-')
                return false;

            return name.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-');
        }
        public ParseResult Execute(string code)
        {
            var result = new ParseResult();
            var lines = code.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            _labels.Clear();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (IsLabelLine(line))
                {
                    string labelName = line;
                    if (_labels.ContainsKey(labelName))
                    {
                        result.Errors.Add(new ErrorInfo
                        {
                            LineNumber = i + 1,
                            Message = $"Etiqueta duplicada: '{labelName}'",
                            Type = ErrorType.Semantic,
                            CodeSnippet = line
                        });
                    }
                    else
                    {
                        _labels[labelName] = i;
                    }
                }
            }

            for (currentLineNumber = 0; currentLineNumber < lines.Length; currentLineNumber++)
            {
                string line = lines[currentLineNumber].Trim();
                if (string.IsNullOrWhiteSpace(line) || IsLabelLine(line)) continue;

                ProcessLine(line, result); 
            }

            result.Success = result.Errors.Count == 0; 
            return result;
        }

        private bool IsLabelLine(string line)
        {
            return !string.IsNullOrWhiteSpace(line) &&
                   !line.StartsWith("Spawn(") &&
                   !line.StartsWith("Color(") &&
                   !line.StartsWith("GoTo") &&
                   Regex.IsMatch(line, @"^[a-zA-Z][a-zA-Z0-9_-]*$");
        }
        
        private void ParseGoTo(string line, ParseResult result = null)
        {
            try
            {
                var match = Regex.Match(line,
                    @"GoTo\s*\[\s*([a-zA-Z][a-zA-Z0-9_-]*)\s*\]\s*\(\s*(.*)\s*\)",
                    RegexOptions.IgnoreCase);

                if (!match.Success || match.Groups.Count != 3)
                    throw new Exception("Formato incorrecto para GoTo. Uso: GoTo [etiqueta] (condición)");

                string label = match.Groups[1].Value;
                string condition = match.Groups[2].Value;

                if (!_labels.TryGetValue(label, out int targetLine))
                    throw new Exception($"Etiqueta no encontrada: '{label}'");

                if (EvaluateBooleanExpression(condition))
                {
                    currentLineNumber = targetLine - 1; // -1 porque después se incrementa
                }
            }
            catch (Exception ex)
            {
                result?.Errors.Add(new ErrorInfo
                {
                    LineNumber = currentLineNumber + 1,
                    Message = $"Error en GoTo: {ex.Message}",
                    Type = ErrorType.Runtime,
                    CodeSnippet = line
                });
            }
        }

        private bool EvaluateBooleanExpression(string expr)
        {
            expr = expr.Trim();

            expr = EvaluateParentheses(expr);

            string[] operators = { "&&", "||" };
            foreach (var op in operators)
            {
                if (expr.Contains(op))
                {
                    var parts = expr.Split(new[] { op }, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        bool left = EvaluateBooleanExpression(parts[0]);
                        bool right = EvaluateBooleanExpression(parts[1]);
                        return op == "&&" ? left && right : left || right;
                    }
                }
            }

            return EvaluateSimpleComparison(expr);
        }

        private string EvaluateParentheses(string expr)
        {
            while (expr.Contains("(") && expr.Contains(")"))
            {
                int lastOpen = expr.LastIndexOf('(');
                int close = expr.IndexOf(')', lastOpen);

                if (close == -1) break;

                string inner = expr.Substring(lastOpen + 1, close - lastOpen - 1);
                bool innerResult = EvaluateBooleanExpression(inner);
                expr = expr.Remove(lastOpen, close - lastOpen + 1)
                          .Insert(lastOpen, innerResult ? "1" : "0");
            }
            return expr;
        }
        private bool EvaluateSimpleComparison(string expr)
        {
            string[] comparators = { "==", "!=", "<=", ">=", "<", ">" };
            foreach (var comp in comparators)
            {
                if (expr.Contains(comp))
                {
                    var parts = expr.Split(new[] { comp }, StringSplitOptions.None);
                    if (parts.Length != 2) continue;

                    int left = EvaluateNumericExpression(parts[0].Trim());
                    int right = EvaluateNumericExpression(parts[1].Trim());

                    switch (comp)
                    {
                        case "==": return left == right;
                        case "!=": return left != right;
                        case "<=": return left <= right;
                        case ">=": return left >= right;
                        case "<": return left < right;
                        case ">": return left > right;
                    }
                }
            }

            return EvaluateNumericExpression(expr) != 0;
        }


        private void ParseVariableAssignment(string line, ParseResult result = null)
        {
            try
            {
                var parts = line.Split(new[] { "<-" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    throw new Exception("Sintaxis incorrecta para asignación. Uso: variable <- expresión");

                string varName = parts[0].Trim();
                string expression = parts[1].Trim();

                if (!_variables.IsValidVariableName(varName))
                    throw new Exception($"Nombre de variable inválido: '{varName}'");

                try
                {
                    if (expression.StartsWith("GetActualX()") ||
                        expression.StartsWith("GetActualY()") ||
                        expression.StartsWith("GetCanvasSize()") ||
                        expression.StartsWith("IsBrushColor(")||
                        expression.StartsWith("IsBrushSize(")||
                        expression.StartsWith("IsCanvasColor(")||
                        expression.StartsWith("GetColorCount"))
                    {
                        int funcValue = ParseFunctionCall(expression);
                        _variables.Assign(varName, funcValue);
                    }
                    else
                    {
                        object value;
                        if (expression.Contains("&&") || expression.Contains("||") ||
                            expression.Contains("==") || expression.Contains("!=") ||
                            expression == "true" || expression == "false")
                        {
                            value = EvaluateBooleanExpression(expression);
                        }
                        else
                        {
                            value = EvaluateNumericExpression(expression);
                        }
                        _variables.Assign(varName, value);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"No se pudo evaluar la expresión: '{expression}'. Error: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                if (result != null)
                {
                    result.Errors.Add(new ErrorInfo
                    {
                        LineNumber = currentLineNumber + 1,
                        Message = ex.Message,
                        Type = ErrorType.Runtime,
                        CodeSnippet = line
                    });
                }
                else throw;
            }
        }

        private int ParseFunctionCall(string line)
        {
            if (line.StartsWith("GetActualX()")) return _robot.GetActualX();
            if (line.StartsWith("GetActualY()")) return _robot.GetActualY();
            if (line.StartsWith("GetCanvasSize()")) return _robot.GetCanvasSize();
            if (line.StartsWith("IsBrushColor("))
            {
                int start = line.IndexOf('(') + 1;
                int end = line.LastIndexOf(')');
                if (start < 0 || end <= start)
                    throw new Exception($"Sintaxis incorrecta en IsBrushColor: {line}");

                string paramContent = line.Substring(start, end - start).Trim();

                string colorName = paramContent.Trim('"', '\'', ' ');

                if (string.IsNullOrWhiteSpace(colorName))
                    throw new Exception("Nombre de color no puede estar vacío");

                return _robot.IsBrushColor(colorName);
            }
            if (line.StartsWith("IsBrushSize("))
            {
                var parts = line.TrimEnd(')').Split('(')[1].Split(',');
                int size = EvaluateParameter(parts[0].Trim());
                return _robot.IsBrushSize(size);
            }
            if (line.StartsWith("IsCanvasColor("))
            {
                var parts = line.TrimEnd(')').Split('(')[1].Split(',');
                string colorName = parts[0].Trim('"', ' ', '\'');
                int vertical = EvaluateParameter(parts[1].Trim());
                int horizontal = EvaluateParameter(parts[2].Trim());
                return _robot.IsCanvasColor(colorName, vertical, horizontal);
            }
            if (line.StartsWith("GetColorCount("))
            {
                var parts = line.TrimEnd(')').Split('(')[1].Split(',');
                string colorName = parts[0].Trim('"', ' ', '\'');
                int x1 = EvaluateParameter(parts[1].Trim());
                int y1 = EvaluateParameter(parts[2].Trim());
                int x2 = EvaluateParameter(parts[3].Trim());
                int y2 = EvaluateParameter(parts[4].Trim());
                return _robot.GetColorCount(colorName, x1, y1, x2, y2);
            }

            throw new Exception($"Función no reconocida: {line}");
        }
        private void ProcessLabel(string line, ParseResult result)
        {
            string labelName = line.Trim();

            if (!Regex.IsMatch(labelName, @"^[a-zA-Z][a-zA-Z0-9_-]*$"))
            {
                throw new Exception($"Nombre de etiqueta inválido: '{labelName}'");
            }

            if (_labels.ContainsKey(labelName))
            {
                throw new Exception($"Etiqueta duplicada: '{labelName}'");
            }

            _labels[labelName] = currentLineNumber;
        }
        
        private int EvaluateNumericExpression(string expression)
        {
            expression = HandleParentheses(expression);
            string[] operators = {"**", "*", "/", "%", "+", "-"};
            foreach(string op in operators)
            {
                if (expression.Contains(op))
                {
                    var parts= expression.Split(new[] { op }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2) continue;
                    int left = EvaluateNumericExpression(parts[0].Trim());
                    int right=EvaluateNumericExpression(parts[1].Trim());
                    switch (op)
                    {
                        case "**":return (int)Math.Pow(left, right);
                        case "*": return left * right; 
                        case "/":
                            if (right == 0) throw new Exception("No se puede dividir por cero"); 
                            return left / right;
                        case "%": return left% right;
                        case "+": return left + right;
                        case "-": return left - right;
                    }
                }
            }
            if (_variables.Exists(expression))
            {
                var result = _variables.GetValue(expression);

                if (result is int intValue)
                {
                    return intValue;
                }
                throw new Exception($"La variable '{expression}' no es numérica (tiene valor {result} de tipo {result.GetType().Name})");
            }
            if (int.TryParse(expression, out int value)) return value;
            if (_variables.Exists(expression)) return (int)_variables.GetValue(expression);

            throw new Exception($"Expresión numérica no válida: '{expression}'");
        }

        private string HandleParentheses(string expression)
        {
            while (expression.Contains("(") && expression.Contains(")"))
            {
                int open = expression.LastIndexOf('(');
                int close = expression.LastIndexOf(')');
                if (close == -1) throw new Exception("Parentesis no balanceados");
                string inner = expression.Substring(open+1, close - open-1);
                object result = EvaluateBooleanExpression(inner);
                expression = expression.Remove(open, close-1).Insert(open,result.ToString());
            }
            return expression; 
        }

        private bool IsValidVariableName(string name)
        {
            return !string.IsNullOrEmpty(name) &&
                   !char.IsDigit(name[0]) &&
                   name.All(c => char.IsLetterOrDigit(c) || c == '-' || c=='_');
        }
        
        
        private int ParseGetColorCount(string line, ParseResult result=null)
        {
            const int Error_Value = -1;
            try
            {
                var parts = line.TrimEnd(')').Split('(')[1].Split(',');
                if (parts.Length != 5)
                    throw new Exception("Sintaxis incorrecta para GetColorCount. Uso: GetColorCount(\"color\", x1, y1, x2, y2)");

                string colorName = parts[0].Trim('"', ' ', '\'');
                int x1 = EvaluateParameter(parts[1].Trim());
                int y1 = EvaluateParameter(parts[2].Trim());
                int x2 = EvaluateParameter(parts[3].Trim());
                int y2 = EvaluateParameter(parts[4].Trim());

                return _robot.GetColorCount(colorName, x1, y1, x2, y2);
            }
            catch (Exception ex) {
                if (result != null)
                {
                    result.Errors.Add(new ErrorInfo
                    {
                        LineNumber = currentLineNumber + 1,
                        Message = ex.Message,
                        Type = ErrorType.Runtime,
                        CodeSnippet = line
                    });
                    return Error_Value;
                }
                else
                {
                    throw;
                }
            }
        }

        public void ParseSizeCommand(string line, ParseResult result = null)
        {
            try
            {
                var parts = line.TrimEnd(')').Split('(');
                if (parts.Length != 2) throw new Exception("Sintaxis incorrecta para Size. Uso Size(3)");
                int size = EvaluateParameter(parts[1]);
                _robot.Size(size);
            }
            catch (Exception ex) {
                if (result != null)
                {
                    result.Errors.Add(new ErrorInfo
                    {
                        LineNumber = currentLineNumber + 1,
                        Message = ex.Message,
                        Type = ErrorType.Syntactic,
                        CodeSnippet = line
                    });
                }
                else
                {
                    throw;
                }
            }
        }

        
        private void ProcessLine(string line, ParseResult result = null)
        {
            try
            {
                if (IsValidLabelName(line) && !line.Contains(" ") && !line.Contains("("))
                {
                    _labels[line] = currentLineNumber;
                    return;
                }
                if (line.StartsWith("Spawn("))
                {
                    ParseSpawnCommand(line, result);
                }
                else if (line.StartsWith("Color("))
                {
                    ParseColorCommand(line, result);
                }
                else if (line.TrimStart().StartsWith("GoTo", StringComparison.OrdinalIgnoreCase))
                {
                    ParseGoTo(line, result);
                }
                
                
                else if (line.StartsWith("DrawLine("))
                {
                    ParseDrawLineCommand(line, result);
                }
                else if (line.StartsWith("//") || line.StartsWith("#"))
                {
                    return;
                }
                else if (line.StartsWith("GetColorCount("))
                {
                    ParseGetColorCount(line, result);
                }
                else if (line.Contains("<-"))
                {
                    ParseVariableAssignment(line, result);
                }
                else if (line.StartsWith("Size("))
                {
                    ParseSizeCommand(line, result);
                }
               
                else if (line.StartsWith("DrawRectangle("))
                {
                    ParseDrawRectangleCommand(line, result);
                }
                else if (line.StartsWith("DrawCircle("))
                {
                    ParseDrawCircleCommand(line, result);
                }

                else if (line.StartsWith("Fill(") || line.Trim() == "Fill")
                {
                    _robot.Fill();
                }
                else
                {
                    var error = $"Comando no reconocido: {line}";
                    if (result != null)
                    {
                        result.Errors.Add(new ErrorInfo
                        {
                            LineNumber = currentLineNumber + 1,
                            Message = error,
                            Type = ErrorType.Syntactic,
                            CodeSnippet = line
                        });
                    }
                    else
                    {
                        throw new Exception(error);
                    }
                }
            }
            catch (Exception ex)
            {
                if (result != null)
                {
                    result.Errors.Add(new ErrorInfo
                    {
                        LineNumber = currentLineNumber + 1,
                        Message = ex.Message,
                        Type = ErrorType.Runtime,
                        CodeSnippet = line
                    });
                }
                
            }
        }
        private void ParseDrawRectangleCommand(string line, ParseResult result=null)
        {
            try
            {
                var parts = line.TrimEnd(')').Split('(')[1].Split(',');
                if (parts.Length != 5)
                    throw new Exception("Sintaxis incorrecta para DrawRectangle. Uso: DrawRectangle(dirX, dirY, distance, width, height)");

                int dirX = EvaluateParameter(parts[0].Trim());
                int dirY = EvaluateParameter(parts[1].Trim());
                int distance = EvaluateParameter(parts[2].Trim());
                int width = EvaluateParameter(parts[3].Trim());
                int height = EvaluateParameter(parts[4].Trim());

                _robot.DrawRectangle(dirX, dirY, distance, width, height);
            }
            catch (Exception ex) { 
                if (result != null)
                {
                    result.Errors.Add(new ErrorInfo
                    {
                        LineNumber = currentLineNumber + 1,
                        Message = ex.Message,
                        Type = ErrorType.Runtime,
                        CodeSnippet = line
                    });
                }
            }
        }
        private void ParseDrawCircleCommand(string line, ParseResult result = null)
        {
            try
            {
                var parts = line.TrimEnd(')').Split('(')[1].Split(',');
                if (parts.Length != 3)
                    throw new Exception("Sintaxis incorrecta para DrawCircle. Uso: DrawCircle(dirX, dirY, radio)");

                int dirX = EvaluateParameter(parts[0].Trim());
                int dirY = EvaluateParameter(parts[1].Trim());
                int radius = EvaluateParameter(parts[2].Trim());

                _robot.DrawCircle(dirX, dirY, radius);
            }
            catch ( Exception ex) {
                if(result != null)
                {
                    result.Errors.Add(new ErrorInfo
                    {
                        Message = ex.Message,
                        LineNumber = currentLineNumber + 1,
                        Type=ErrorType.Syntactic,
                        CodeSnippet = line
                    });
                }
            }
                    
        }
        
        private void ParseSpawnCommand(string line,ParseResult result=null)
        {
            try
            {
                var parts = line.Split(new[] { '(', ',', ')' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 3)
                {
                    throw new Exception("Sintaxis incorrecta para Spawn. Uso: Spawn(x, y)");
                }

                int x = EvaluateParameter(parts[1].Trim());
                int y = EvaluateParameter(parts[2].Trim());
                _robot.Spawn(x, y);
            }
            catch (Exception ex)
            {
                if (result != null)
                {
                    result.Errors.Add(new ErrorInfo
                    {
                        LineNumber = currentLineNumber + 1,
                        Message = ex.Message,
                        Type = ErrorType.Syntactic,
                        CodeSnippet = line
                    });
                }
                else
                {
                    throw;
                }
            }
        }   

        private void ParseColorCommand(string line, ParseResult result = null)
        {
            try
            {
                int start = line.IndexOf('(') + 1;
                int end = line.IndexOf(')');
                if (start <= 0 || end <= 0)
                    throw new Exception("Sintaxis incorrecta para Color. Uso: Color(\"NombreColor\")");

                string colorName = line.Substring(start, end - start).Trim('"', ' ', '\'');
                _robot.SetColor(colorName);
            }
            catch (Exception ex)
            {
                if (result != null)
                {
                    result.Errors.Add(new ErrorInfo
                    {
                        LineNumber = currentLineNumber + 1,
                        Message = ex.Message,
                        Type = ErrorType.Syntactic,
                        CodeSnippet = line
                    });
                }
                else
                {
                    throw;
                }
            }
        }

        private void ParseDrawLineCommand(string line,ParseResult result=null)
        {
            try
            {
                var parts = line.Split(new[] { '(', ',', ')' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 4)
                    throw new Exception("Sintaxis incorrecta para DrawLine. Uso: DrawLine(dirX, dirY, distancia)");

                int dirX = EvaluateParameter(parts[1].Trim());
                int dirY = EvaluateParameter(parts[2].Trim());
                int distance = EvaluateParameter(parts[3].Trim());

                if (Math.Abs(dirX) > 1 || Math.Abs(dirY) > 1)
                    throw new Exception("Las direcciones deben ser -1, 0 o 1");

                _robot.DrawLine(dirX, dirY, distance);
            }
            catch (Exception ex)
            {
                if (result != null)
                {
                    result.Errors.Add(new ErrorInfo
                    {
                        LineNumber = currentLineNumber + 1,
                        Message = ex.Message,
                        Type = ErrorType.Syntactic,
                        CodeSnippet = line
                    });
                }
                else throw;
            }
        }
        private int EvaluateParameter(string param)
        {
            if (_variables.Exists(param))
            {
                var value = _variables.GetValue(param);
                if (value is int intValue) return intValue;
                throw new Exception($"La variable {param} no es numérica");
            }

            if (int.TryParse(param, out int result)) return result;

            throw new Exception($"Parámetro no válido: {param}");
        }
    }
}