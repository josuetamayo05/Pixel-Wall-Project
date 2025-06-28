using PixelW.CommandParsing.Command;
using PixelW.CommandParsing.Expressions;
using PixelW.CommandParsing.Validation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace PixelW
{
    internal partial class CommandParser
    {
        private readonly LabelManager _labelManager;
        private readonly List<ErrorValidator> _validators;
        private readonly List<CommandProcessor> _commandProcessors;
        private readonly WallE _robot;
        private readonly ExpressionEvaluator _evaluator;
        private readonly VariableManager _variables;
        private int currentLineNumber;
        public CommandParser(WallE robot, VariableManager variables)
        {
            _robot = robot ?? throw new ArgumentNullException(nameof(robot));
            _variables = variables ?? throw new ArgumentNullException(nameof(variables));
            _evaluator = new ExpressionEvaluator(variables);
            _labelManager = new LabelManager();
            _validators = new List<ErrorValidator>
            {
                new SyntaxValidator(),
                new SemanticValidator(_labelManager,variables,robot),
                new RuntimeValidator(robot)
            };
            _commandProcessors = new List<CommandProcessor>
            {
                new GoToCommand(robot,variables, _evaluator,_labelManager),
                new SpawnCommand(robot, variables, _evaluator, _labelManager),
                new ColorCommand(robot, variables, _evaluator, _labelManager),
                new DrawLineCommand(robot, variables, _evaluator, _labelManager),
                new VariableAssignment(robot, variables, _evaluator, _labelManager),
                new FillCommand(robot, variables, _evaluator, _labelManager),
                new SizeCommand(robot, variables, _evaluator, _labelManager),
                new DrawCircleCommand(robot, variables, _evaluator, _labelManager),
                new GetColorCountCommand(robot,variables,_evaluator,_labelManager),
                new DrawRectangleCommand(robot,variables,_evaluator,_labelManager),

            };
        }
        
        public class ParseResult
        {
            public bool Success { get; set; }
            public List<ErrorInfo> Errors { get; } = new List<ErrorInfo>();
            public int? JumpToLine { get; set; }
            public int LastReturnValue { get; set; }
        }
        public class ErrorInfo
        {
            public int LineNumber { get; set; }
            public string Message { get; set; }
            public ErrorType Type { get; set; }
            public string CodeSnippet {  get; set; }
        }

        public enum ErrorType
        {
            Syntactic,
            Semantic,
            Runtime
        }

        public ParseResult Execute(string code)
        {
            var result = new ParseResult();
            var lines = code.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            //idd todas las etiq
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                
                if (_labelManager.IsLabelLine(line))
                {
                    try
                    {
                        _labelManager.AddLabel(line, i);
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new ErrorInfo
                        {
                            LineNumber = i + 1,
                            Message = ex.Message,
                            Type = ErrorType.Semantic,
                            CodeSnippet = line
                        });
                    }
                }
            }
            //ejec comandos
            for (currentLineNumber = 0; currentLineNumber < lines.Length; currentLineNumber++)
            {
                string line = lines[currentLineNumber].Trim();
                if (string.IsNullOrWhiteSpace(line) || _labelManager.IsLabelLine(line)) continue;
                foreach (var validator in _validators)
                {
                    validator.Validate(line, currentLineNumber + 1, result);
                }
                ProcessLine(line, currentLineNumber, result);
                if (result.JumpToLine.HasValue)
                {
                    currentLineNumber = result.JumpToLine.Value - 1;
                    result.JumpToLine = null;
                }
            }

            result.Success = result.Errors.Count == 0;
            return result;
        }
                          
        private void ProcessLine(string line, int lineNumber,ParseResult result)
        {
            try
            {
                var processor=_commandProcessors.FirstOrDefault(p => p.CanProcess(line));
                if (processor != null) {
                    processor.Process(line, lineNumber+1, result);
                    return;
                }
                result.Errors.Add(new ErrorInfo
                {
                    LineNumber = lineNumber + 1,
                    Message = $"Comando no reconocido: {line}",
                    Type = ErrorType.Syntactic,
                    CodeSnippet = line
                });
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ErrorInfo
                {
                    LineNumber = lineNumber + 1,
                    Message = ex.Message,
                    Type = ErrorType.Runtime,
                    CodeSnippet = line
                });
            }
        }                                        
    }
}