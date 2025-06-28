using PixelW.CommandParsing.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static PixelW.CommandParser;

namespace PixelW.CommandParsing.Validation
{
    internal class SemanticValidator : ErrorValidator
    {
        private readonly LabelManager _labelManager;
        private readonly VariableManager _variableManager;
        private readonly WallE _robot;

        public SemanticValidator(LabelManager labelManager, VariableManager variableManager, WallE robot)
        {
            _labelManager = labelManager;
            _variableManager = variableManager;
            _robot = robot;
        }

        public override void Validate(string line, int lineNumber, ParseResult result)
        {
            if (string.IsNullOrWhiteSpace(line)) return;

            ValidateGoToLabels(line, lineNumber, result);
            ValidateSpawnPosition(line, lineNumber, result);
            ValidateColorCommands(line, lineNumber, result);
            ValidateFunctionCalls(line, lineNumber, result);
        }

        private void ValidateGoToLabels(string line, int lineNumber, ParseResult result)
        {
            if (line.TrimStart().StartsWith("GoTo", StringComparison.OrdinalIgnoreCase))
            {
                string label = ExtractLabel(line, lineNumber, result);
                if (label != null && !_labelManager.LabelExists(label))
                {
                    AddError(result, lineNumber,
                            $"Etiqueta '{label}' no definida",
                            ErrorType.Semantic, line);
                }
            }
        }

        public string ExtractLabel(string goToLine, int lineNumber, ParseResult result)
        {
            try
            {
                var match = Regex.Match(goToLine,
                    @"GoTo\s*\[\s*([a-zA-Z][a-zA-Z0-9_-]*)\s*\]\s*\(\s*(.*)\s*\)",
                    RegexOptions.IgnoreCase);

                if (!match.Success || match.Groups.Count != 3)
                {
                    AddError(result, lineNumber,
                            "Formato incorrecto para GoTo. Uso: GoTo [etiqueta] (condición)",
                            ErrorType.Syntactic, goToLine);
                    return null;
                }

                string label = match.Groups[1].Value.Trim();

                if (!IsValidIdentifier(label))
                {
                    AddError(result, lineNumber,
                            $"Nombre de etiqueta inválido: '{label}'. Solo puede contener letras, números, guiones y guiones bajos",
                            ErrorType.Semantic, goToLine);
                    return null;
                }

                return label;
            }
            catch (Exception ex)
            {
                AddError(result, lineNumber,
                        $"Error al extraer etiqueta: {ex.Message}",
                        ErrorType.Semantic, goToLine);
                return null;
            }
        }

       

        

        private void ValidateSpawnPosition(string line, int lineNumber, ParseResult result)
        {
            if (line.StartsWith("Spawn("))
            {
                try
                {
                    var parts = line.Split(new[] { '(', ',', ')' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        int x = int.Parse(parts[1].Trim());
                        int y = int.Parse(parts[2].Trim());

                        if (x < 0 || y < 0 ||
                            x >= _robot.GetCanvasSize() ||
                            y >= _robot.GetCanvasSize())
                        {
                            AddError(result, lineNumber,
                                    $"Posición inicial ({x}, {y}) fuera del canvas (tamaño: {_robot.GetCanvasSize()})",
                                    ErrorType.Semantic, line);
                        }
                    }
                }
                catch
                {
                }
            }
        }

        private void ValidateColorCommands(string line, int lineNumber, ParseResult result)
        {
            if (line.StartsWith("Color("))
            {
                var match = Regex.Match(line, @"Color\(([^)]+)\)");
                if (match.Success)
                {
                    string color = match.Groups[1].Value.Trim('"', '\'', ' ');
                    if (color.Equals("Transparent", StringComparison.OrdinalIgnoreCase) &&
                        lineNumber == 1)
                    {
                        AddError(result, lineNumber,
                                "No se puede comenzar con color Transparent",
                                ErrorType.Semantic, line);
                    }
                }
            }
        }

        

        private void ValidateFunctionCalls(string line, int lineNumber, ParseResult result)
        {
            if (line.StartsWith("GetColorCount("))
            {
                var parts = line.Split(new[] { '(', ',', ')' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 6)
                {
                    int x1 = int.Parse(parts[2].Trim());
                    int y1 = int.Parse(parts[3].Trim());
                    int x2 = int.Parse(parts[4].Trim());
                    int y2 = int.Parse(parts[5].Trim());

                    if (x1 > x2 || y1 > y2)
                    {
                        AddError(result, lineNumber,
                                "Coordenadas inválidas. x1,y1 deben ser la esquina superior izquierda y x2,y2 la inferior derecha",
                                ErrorType.Semantic, line);
                    }
                }
            }
        }

        private bool IsValidIdentifier(string name)
        {
            return Regex.IsMatch(name, @"^[a-zA-Z_][a-zA-Z0-9_-]*$") &&
                   !IsReservedKeyword(name);
        }

        private bool IsReservedKeyword(string word)
        {
            string[] keywords = { "Spawn", "Color", "Size", "DrawLine",
                                 "DrawCircle", "DrawRectangle", "Fill",
                                 "GoTo", "true", "false", "and", "or" };
            return keywords.Contains(word, StringComparer.OrdinalIgnoreCase);
        }

        private bool IsFunctionCall(string word, string line)
        {
            string[] functions = { "GetActualX", "GetActualY", "GetCanvasSize",
                                 "GetColorCount", "IsBrushColor", "IsBrushSize",
                                 "IsCanvasColor" };
            return functions.Contains(word) && line.Contains($"{word}(");
        }
    }

}
