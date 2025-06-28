using PixelW.CommandParsing.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PixelW.CommandParser;

namespace PixelW.CommandParsing.Command
{
    internal class VariableAssignment:CommandProcessor
    {
        public VariableAssignment(WallE robot, VariableManager variables,
                                        ExpressionEvaluator evaluator, LabelManager labelManager)
            : base(robot, variables, evaluator, labelManager) { }

        public override bool CanProcess(string command)
        {
            return command.Contains("<-");
        }

        public override void Process(string command, int lineNumber, ParseResult result)
        {
            try
            {
                var parts = command.Split(new[] { "<-" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    throw new Exception("Sintaxis incorrecta para asignación. Uso: variable <- expresión");
                }

                string varName = parts[0].Trim();
                string expression = parts[1].Trim();

                if (!_variables.IsValidVariableName(varName))
                {
                    throw new Exception($"Nombre de variable inválido: '{varName}'");
                }

                object value;
                if (expression.StartsWith("GetActualX()") || expression.StartsWith("GetActualY()") ||
                    expression.StartsWith("GetCanvasSize()") || expression.StartsWith("IsBrushColor(") ||
                    expression.StartsWith("IsBrushSize(") || expression.StartsWith("IsCanvasColor(") ||
                    expression.StartsWith("GetColorCount"))
                {
                    value = ParseFunctionCall(expression);
                }
                else if (expression.Contains("&&") || expression.Contains("||") ||
                         expression.Contains("==") || expression.Contains("!=") ||
                         expression == "true" || expression == "false")
                {
                    value = _evaluator.EvaluateBooleanExpression(expression);
                }
                else
                {
                    value = _evaluator.EvaluateNumericExpression(expression);
                }

                _variables.Assign(varName, value);
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ErrorInfo
                {
                    LineNumber = lineNumber,
                    Message = ex.Message,
                    Type = ErrorType.Runtime,
                    CodeSnippet = command
                });
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
                int size = _evaluator.EvaluateParameter(parts[0].Trim());
                return _robot.IsBrushSize(size);
            }
            if (line.StartsWith("IsCanvasColor("))
            {
                var parts = line.TrimEnd(')').Split('(')[1].Split(',');
                string colorName = parts[0].Trim('"', ' ', '\'');
                int vertical = _evaluator.EvaluateParameter(parts[1].Trim());
                int horizontal = _evaluator.EvaluateParameter(parts[2].Trim());
                return _robot.IsCanvasColor(colorName, vertical, horizontal);
            }
            if (line.StartsWith("GetColorCount("))
            {
                var parts = line.TrimEnd(')').Split('(')[1].Split(',');
                string colorName = parts[0].Trim('"', ' ', '\'');
                int x1 = _evaluator.EvaluateParameter(parts[1].Trim());
                int y1 = _evaluator.EvaluateParameter(parts[2].Trim());
                int x2 = _evaluator.EvaluateParameter(parts[3].Trim());
                int y2 = _evaluator.EvaluateParameter(parts[4].Trim());
                return _robot.GetColorCount(colorName, x1, y1, x2, y2);
            }

            throw new Exception($"Función no reconocida: {line}");
        }
    }
}
