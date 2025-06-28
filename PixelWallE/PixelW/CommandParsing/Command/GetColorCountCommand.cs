using PixelW.CommandParsing.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PixelW.CommandParser;

namespace PixelW.CommandParsing.Command
{
    internal class GetColorCountCommand:CommandProcessor
    {
        public GetColorCountCommand(WallE robot, VariableManager variables,
                                           ExpressionEvaluator evaluator, LabelManager labelManager)
            : base(robot, variables, evaluator, labelManager) { }

        public override bool CanProcess(string command)
        {
            return command.TrimStart().StartsWith("GetColorCount(", StringComparison.OrdinalIgnoreCase);
        }

        public override void Process(string command, int lineNumber, ParseResult result)
        {
            try
            {
                var parts = command.TrimEnd(')').Split('(')[1].Split(',');
                if (parts.Length != 5)
                {
                    throw new Exception("Sintaxis incorrecta para GetColorCount. Uso: GetColorCount(\"color\", x1, y1, x2, y2)");
                }

                string colorName = parts[0].Trim('"', ' ', '\'');
                int x1 = _evaluator.EvaluateNumericExpression(parts[1].Trim());
                int y1 = _evaluator.EvaluateNumericExpression(parts[2].Trim());
                int x2 = _evaluator.EvaluateNumericExpression(parts[3].Trim());
                int y2 = _evaluator.EvaluateNumericExpression(parts[4].Trim());

                int count = _robot.GetColorCount(colorName, x1, y1, x2, y2);

                // Si necesitas devolver este valor para usarlo en asignaciones,
                // podrías almacenarlo en el ParseResult
                result.LastReturnValue = count;
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
    }
}
