using PixelW.CommandParsing.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PixelW.CommandParser;

namespace PixelW.CommandParsing.Command
{
    internal class DrawCircleCommand:CommandProcessor
    {
        public DrawCircleCommand(WallE robot, VariableManager variables,
                                        ExpressionEvaluator evaluator, LabelManager labelManager)
            : base(robot, variables, evaluator, labelManager) { }

        public override bool CanProcess(string command)
        {
            return command.TrimStart().StartsWith("DrawCircle(", StringComparison.OrdinalIgnoreCase);
        }

        public override void Process(string command, int lineNumber, ParseResult result)
        {
            try
            {
                var parts = command.TrimEnd(')').Split('(')[1].Split(',');
                if (parts.Length != 3)
                {
                    throw new Exception("Sintaxis incorrecta para DrawCircle. Uso: DrawCircle(dirX, dirY, radio)");
                }

                int dirX = _evaluator.EvaluateNumericExpression(parts[0].Trim());
                int dirY = _evaluator.EvaluateNumericExpression(parts[1].Trim());
                int radius = _evaluator.EvaluateNumericExpression(parts[2].Trim());

                _robot.DrawCircle(dirX, dirY, radius);
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ErrorInfo
                {
                    LineNumber = lineNumber,
                    Message = ex.Message,
                    Type = ErrorType.Syntactic,
                    CodeSnippet = command
                });
            }
        }
    }
}
