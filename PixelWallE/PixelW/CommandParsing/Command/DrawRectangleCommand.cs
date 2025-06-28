using PixelW.CommandParsing.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PixelW.CommandParser;

namespace PixelW.CommandParsing.Command
{
    internal class DrawRectangleCommand:CommandProcessor
    {
        public DrawRectangleCommand(WallE robot, VariableManager variables,
                                           ExpressionEvaluator evaluator, LabelManager labelManager)
            : base(robot, variables, evaluator, labelManager) { }

        public override bool CanProcess(string command)
        {
            return command.TrimStart().StartsWith("DrawRectangle(", StringComparison.OrdinalIgnoreCase);
        }

        public override void Process(string command, int lineNumber, ParseResult result)
        {
            try
            {
                var parts = command.TrimEnd(')').Split('(')[1].Split(',');
                if (parts.Length != 5)
                {
                    throw new Exception("Sintaxis incorrecta para DrawRectangle. Uso: DrawRectangle(dirX, dirY, distance, width, height)");
                }

                int dirX = _evaluator.EvaluateNumericExpression(parts[0].Trim());
                int dirY = _evaluator.EvaluateNumericExpression(parts[1].Trim());
                int distance = _evaluator.EvaluateNumericExpression(parts[2].Trim());
                int width = _evaluator.EvaluateNumericExpression(parts[3].Trim());
                int height = _evaluator.EvaluateNumericExpression(parts[4].Trim());

                _robot.DrawRectangle(dirX, dirY, distance, width, height);
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
