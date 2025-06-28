using PixelW.CommandParsing.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static PixelW.CommandParser;

namespace PixelW.CommandParsing.Command
{
    internal class GoToCommand : CommandProcessor
    {
        public GoToCommand(WallE robot,VariableManager variables,ExpressionEvaluator evaluator,LabelManager labelManager) :base(robot,variables,evaluator, labelManager) { }
        public override bool CanProcess(string command)
        {
            return command.TrimStart().StartsWith("GoTo",StringComparison.OrdinalIgnoreCase);
        }
        public override void Process(string command, int lineNumber, CommandParser.ParseResult result)
        {
            try
            {
                var match = Regex.Match(command,
                    @"GoTo\s*\[\s*([a-zA-Z][a-zA-Z0-9_-]*)\s*\]\s*\(\s*(.*)\s*\)",
                    RegexOptions.IgnoreCase);

                if (!match.Success || match.Groups.Count != 3)
                    throw new Exception("Formato incorrecto para GoTo. Uso: GoTo [etiqueta] (condición)");

                string label = match.Groups[1].Value;
                string condition = match.Groups[2].Value;

                if (!_labelManager.TryGetLabelLine(label, out int targetLine))
                    throw new Exception($"Etiqueta no encontrada: '{label}'");

                if (_evaluator.EvaluateBooleanExpression(condition))
                {
                    result.JumpToLine = targetLine;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ErrorInfo
                {
                    LineNumber = lineNumber,
                    Message = $"Error en GoTo: {ex.Message}",
                    Type = ErrorType.Runtime,
                    CodeSnippet = command
                });
            }
        }
    }   
}
