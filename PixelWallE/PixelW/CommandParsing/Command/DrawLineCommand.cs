using PixelW.CommandParsing.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PixelW.CommandParser;

namespace PixelW.CommandParsing.Command
{
    internal class DrawLineCommand:CommandProcessor
    {
        public DrawLineCommand(WallE robot, VariableManager variables,
                                      ExpressionEvaluator evaluator, LabelManager labelManager)
            : base(robot, variables, evaluator, labelManager) { }

        public override bool CanProcess(string command)
        {
            return command.TrimStart().StartsWith("DrawLine(", StringComparison.OrdinalIgnoreCase);
        }

        public override void Process(string command, int lineNumber, ParseResult result)
        {
            try
            {
                var parts = command.Split(new[] { '(', ',', ')' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 4)
                {
                    throw new Exception("Sintaxis incorrecta para DrawLine. Uso: DrawLine(dirX, dirY, distancia)");
                }

                int dirX = _evaluator.EvaluateNumericExpression(parts[1].Trim());
                int dirY = _evaluator.EvaluateNumericExpression(parts[2].Trim());
                int distance = _evaluator.EvaluateNumericExpression(parts[3].Trim());

                if (Math.Abs(dirX) > 1 || Math.Abs(dirY) > 1)
                {
                    throw new Exception("Las direcciones deben ser -1, 0 o 1");
                }

                _robot.DrawLine(dirX, dirY, distance);
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

