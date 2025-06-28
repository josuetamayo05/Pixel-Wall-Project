using PixelW.CommandParsing.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PixelW.CommandParser;

namespace PixelW.CommandParsing.Command
{
    internal class SpawnCommand : CommandProcessor
    {
        public SpawnCommand(WallE robot, VariableManager variables, ExpressionEvaluator evaluator, LabelManager labelManager) : base(robot, variables, evaluator, labelManager) { }
        public override bool CanProcess(string line) => line.TrimStart().StartsWith("Spawn(");
        public override void Process(string line, int currentLineNumber, ParseResult result)
        {
            try
            {
                var parts = line.Split(new[] { '(', ',', ')' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 3)
                {
                    throw new Exception("Sintaxis incorrecta para Spawn. Uso: Spawn(x, y)");
                }

                int x = _evaluator.EvaluateNumericExpression(parts[1].Trim());
                int y = _evaluator.EvaluateNumericExpression(parts[2].Trim());
                _robot.Spawn(x, y);
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ErrorInfo
                {
                    LineNumber = currentLineNumber + 1,
                    Message = ex.Message,
                    Type = ErrorType.Syntactic,
                    CodeSnippet = line
                });
            }
        }
    }
}
