using PixelW.CommandParsing.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PixelW.CommandParser;

namespace PixelW.CommandParsing.Command
{
    internal class SizeCommand:CommandProcessor
    {
        public SizeCommand(WallE robot, VariableManager variables,
                                   ExpressionEvaluator evaluator, LabelManager labelManager)
            : base(robot, variables, evaluator, labelManager) { }

        public override bool CanProcess(string command)
        {
            return command.TrimStart().StartsWith("Size(", StringComparison.OrdinalIgnoreCase);
        }

        public override void Process(string command, int lineNumber, ParseResult result)
        {
            try
            {
                var parts = command.TrimEnd(')').Split('(');
                if (parts.Length != 2)
                {
                    throw new Exception("Sintaxis incorrecta para Size. Uso: Size(3)");
                }

                int size = _evaluator.EvaluateNumericExpression(parts[1]);
                _robot.Size(size);
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
