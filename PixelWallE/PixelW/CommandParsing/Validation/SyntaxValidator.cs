using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static PixelW.CommandParser;

namespace PixelW.CommandParsing.Validation
{
    internal class SyntaxValidator : ErrorValidator
    {
        private static readonly string[] ValidColors =
        {
            "Red", "Green", "Blue", "Yellow", "Orange",
            "Purple", "Black", "White", "Transparent"
        };

        public override void Validate(string line, int lineNumber, ParseResult result)
        {
            if (string.IsNullOrWhiteSpace(line)) return;

            ValidateParentheses(line, lineNumber, result);
            ValidateSpawnCommand(line, lineNumber, result);
            ValidateDrawCommands(line, lineNumber, result);
            ValidateVariableAssignment(line, lineNumber, result);
            ValidateGoToCommand(line, lineNumber, result);
            ValidateFunctionCalls(line, lineNumber, result);
        }

        private void ValidateParentheses(string line, int lineNumber, ParseResult result)
        {
            if (line.Count(c => c == '(') != line.Count(c => c == ')'))
            {
                AddError(result, lineNumber,
                        "Paréntesis no balanceados",
                        ErrorType.Syntactic, line);
            }
        }

        private void ValidateSpawnCommand(string line, int lineNumber, ParseResult result)
        {
            if (line.StartsWith("Spawn("))
            {
                if (!Regex.IsMatch(line, @"^Spawn\(\s*\d+\s*,\s*\d+\s*\)$"))
                {
                    AddError(result, lineNumber,
                            "Sintaxis incorrecta para Spawn. Uso: Spawn(x, y)",
                            ErrorType.Syntactic, line);
                }
            }
        }

        

        private void ValidateDrawCommands(string line, int lineNumber, ParseResult result)
        {
            if (line.StartsWith("DrawLine("))
            {
                if (!IsValidParameterizedCommand(line, "DrawLine", 3))
                {
                    AddError(result, lineNumber,
                            "Sintaxis incorrecta para DrawLine. Uso: DrawLine(dirX, dirY, distance)",
                            ErrorType.Syntactic, line);
                }
            }
            else if (line.StartsWith("DrawCircle("))
            {
                if (!IsValidParameterizedCommand(line, "DrawCircle", 3))
                {
                    AddError(result, lineNumber,
                            "Sintaxis incorrecta para DrawCircle. Uso: DrawCircle(dirX, dirY, radius)",
                            ErrorType.Syntactic, line);
                }
            }
            else if (line.StartsWith("DrawRectangle("))
            {
                if (!IsValidParameterizedCommand(line, "DrawRectangle(", 3))
                {
                    AddError(result, lineNumber,
                            "Sintaxis incorrecta para DrawRectangle. Uso: DrawCircle(dirX, dirY, radius)",
                            ErrorType.Syntactic, line);
                }
            }
        }

        private bool IsValidParameterizedCommand(string line, string command, int paramCount)
        {
            if (!line.EndsWith(")"))
            {
                return false;
            }

            int startIndex = line.IndexOf('(') + 1;
            int length = line.Length - startIndex - 1;
            string content = line.Substring(startIndex, length).Trim();

            string[] parameters = content.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                         .Select(p => p.Trim())
                                         .ToArray();

            // Verifica que haya exactamente 'paramCount' parámetros
            if (parameters.Length != paramCount)
            {
                return false;
            }

            string paramPattern = @"^([a-zA-Z_][a-zA-Z0-9_]*|\d+|[-+*/%()\s]+)$";

            foreach (string param in parameters)
            {
                if (!Regex.IsMatch(param, paramPattern))
                {
                    return false;
                }
            }

            return true;
        }

        //////private void ValidateDrawCommands(string line, int lineNumber, ParseResult result)
        //////{
        //////    if (line.StartsWith("DrawLine("))
        //////    {
        //////        if (!Regex.IsMatch(line, @"^DrawLine\(\s*([a-zA-Z_][a-zA-Z0-9_]*|\d+|[-+*/%()\s]+)\s*,\s*([a-zA-Z_][a-zA-Z0-9_]*|\d+|[-+*/%()\s]+)\s*,\s*([a-zA-Z_][a-zA-Z0-9_]*|\d+|[-+*/%()\s]+)\s*\)$"))
        //////        {
        //////            AddError(result, lineNumber,
        //////                    "Sintaxis incorrecta para DrawLine. Uso: DrawLine(dirX, dirY, distance)",
        //////                    ErrorType.Syntactic, line);
        //////        }
        //////    }
        //////    else if (line.StartsWith("DrawCircle("))
        //////    {
        //////        if (!Regex.IsMatch(line, @"^DrawCircle\(\s*([a-zA-Z_][a-zA-Z0-9_]*|\d+|[-+*/%()\s]+)\s*,\s*([a-zA-Z_][a-zA-Z0-9_]*|\d+|[-+*/%()\s]+)\s*,\s*([a-zA-Z_][a-zA-Z0-9_]*|\d+|[-+*/%()\s]+)\s*\)$"))
        //////        {
        //////            AddError(result, lineNumber,
        //////                    "Sintaxis incorrecta para DrawCircle. Uso: DrawCircle(dirX, dirY, radius)",
        //////                    ErrorType.Syntactic, line);
        //////        }
        //////    }
        //////    else if (line.StartsWith("DrawRectangle("))
        //////    {
        //////        if (!Regex.IsMatch(line, @"^DrawRectangle\(\s*([a-zA-Z_][a-zA-Z0-9_]*|\d+|[-+*/%()\s]+)\s*,\s*([a-zA-Z_][a-zA-Z0-9_]*|\d+|[-+*/%()\s]+)\s*,\s*([a-zA-Z_][a-zA-Z0-9_]*|\d+|[-+*/%()\s]+)\s*,\s*([a-zA-Z_][a-zA-Z0-9_]*|\d+|[-+*/%()\s]+)\s*,\s*([a-zA-Z_][a-zA-Z0-9_]*|\d+|[-+*/%()\s]+)\s*\)$"))
        //////        {
        //////            AddError(result, lineNumber,
        //////                    "Sintaxis incorrecta para DrawRectangle. Uso: DrawRectangle(dirX, dirY, distance, width, height)",
        //////                    ErrorType.Syntactic, line);
        //////        }
        //////    }
        //////}

        private void ValidateVariableAssignment(string line, int lineNumber, ParseResult result)
        {
            if (line.Contains("<-"))
            {
                if (!Regex.IsMatch(line, @"^[a-zA-Z_][a-zA-Z0-9_]*\s*<-\s*.+$"))
                {
                    AddError(result, lineNumber,
                            "Sintaxis incorrecta para asignación. Uso: variable <- expresión",
                            ErrorType.Syntactic, line);
                }

                var varName = line.Split(new[] { "<-" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                if (!Regex.IsMatch(varName, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
                {
                    AddError(result, lineNumber,
                            $"Nombre de variable inválido: '{varName}'. Debe comenzar con letra y puede contener letras, números y _",
                            ErrorType.Syntactic, line);
                }
            }
        }

        private void ValidateGoToCommand(string line, int lineNumber, ParseResult result)
        {
            if (line.TrimStart().StartsWith("GoTo", StringComparison.OrdinalIgnoreCase))
            {
                if (!Regex.IsMatch(line, @"^GoTo\s*\[\s*[a-zA-Z_][a-zA-Z0-9_]*\s*\]\s*\(\s*.+\s*\)$", RegexOptions.IgnoreCase))
                {
                    AddError(result, lineNumber,
                            "Sintaxis incorrecta para GoTo. Uso: GoTo [etiqueta] (condición)",
                            ErrorType.Syntactic, line);
                }
            }
        }

        private void ValidateFunctionCalls(string line, int lineNumber, ParseResult result)
        {
            string[] functions = { "GetActualX", "GetActualY", "GetCanvasSize",
                                 "GetColorCount", "IsBrushColor", "IsBrushSize", "IsCanvasColor" };

            foreach (var func in functions)
            {
                if (line.StartsWith($"{func}("))
                {
                    if (!line.EndsWith(")"))
                    {
                        AddError(result, lineNumber,
                                $"Sintaxis incorrecta para {func}. Falta cerrar paréntesis",
                                ErrorType.Syntactic, line);
                    }

                    if (func == "GetColorCount" && !Regex.IsMatch(line,
                        @"^GetColorCount\(\s*""[^""]+""\s*,\s*\d+\s*,\s*\d+\s*,\s*\d+\s*,\s*\d+\s*\)$"))
                    {
                        AddError(result, lineNumber,
                                "Sintaxis incorrecta para GetColorCount. Uso: GetColorCount(\"color\", x1, y1, x2, y2)",
                                ErrorType.Syntactic, line);
                    }
                    else if (func == "IsCanvasColor" && !Regex.IsMatch(line,
                             @"^IsCanvasColor\(\s*""[^""]+""\s*,\s*[-]?\d+\s*,\s*[-]?\d+\s*\)$"))
                    {
                        AddError(result, lineNumber,
                                "Sintaxis incorrecta para IsCanvasColor. Uso: IsCanvasColor(\"color\", vertical, horizontal)",
                                ErrorType.Syntactic, line);
                    }
                }
            }
        }
    }    

}
