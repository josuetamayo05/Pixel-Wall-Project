using PixelW.CommandParsing.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PixelW.CommandParser;

namespace PixelW.CommandParsing.Command
{
    internal class ColorCommand:CommandProcessor
    {
        public ColorCommand(WallE robot, VariableManager variables,ExpressionEvaluator evaluator, LabelManager labelManager)
            : base(robot, variables, evaluator, labelManager) { }
        public override bool CanProcess(string command)
        {
            return command.TrimStart().StartsWith("Color(", StringComparison.OrdinalIgnoreCase);
        }
        public override void Process(string command, int lineNumber, ParseResult result)
        {
            try
            {
                int start = command.IndexOf('(') + 1;
                int end = command.IndexOf(')');
                if (start <= 0 || end <= 0)
                {
                    throw new Exception("Sintaxis incorrecta para Color. Uso: Color(\"NombreColor\")");
                }

                string colorName = command.Substring(start, end - start).Trim('"', ' ', '\'');
                _robot.SetColor(colorName);
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ErrorInfo
                {
                    LineNumber = lineNumber+1,
                    Message = ex.Message,
                    Type = ErrorType.Syntactic,
                    CodeSnippet = command
                });
            }
        }
    }
}
