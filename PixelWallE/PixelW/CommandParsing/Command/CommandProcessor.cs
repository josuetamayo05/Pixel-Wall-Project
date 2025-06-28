using PixelW.CommandParsing.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PixelW.CommandParser;

namespace PixelW.CommandParsing.Command
{
    internal abstract class CommandProcessor
    {
        protected readonly WallE _robot;
        protected readonly VariableManager _variables;
        protected readonly ExpressionEvaluator _evaluator;
        protected readonly LabelManager _labelManager;

        protected CommandProcessor(WallE robot, VariableManager variables,ExpressionEvaluator evaluator, LabelManager labelManager)
        {
            _robot = robot;
            _variables = variables;
            _evaluator = evaluator;
            _labelManager = labelManager;
        }

        public abstract bool CanProcess(string command);
        public abstract void Process(string command, int lineNumber, ParseResult result);
    }
}
