using PixelW.CommandParsing.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PixelW.CommandParser;

namespace PixelW.CommandParsing.Command
{
    internal class FillCommand:CommandProcessor
    {
        public FillCommand(WallE robot, VariableManager variables,
                                   ExpressionEvaluator evaluator, LabelManager labelManager)
            : base(robot, variables, evaluator, labelManager) { }

        public override bool CanProcess(string command)
        {
            return command.Trim().Equals("Fill", StringComparison.OrdinalIgnoreCase) ||
                   command.TrimStart().StartsWith("Fill(", StringComparison.OrdinalIgnoreCase);
        }

        public override void Process(string command, int lineNumber, ParseResult result)
        {
            try
            {
                _robot.Fill();
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
