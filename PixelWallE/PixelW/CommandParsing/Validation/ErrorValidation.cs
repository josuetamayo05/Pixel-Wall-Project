using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PixelW.CommandParser;

namespace PixelW.CommandParsing.Validation
{
    internal abstract class ErrorValidator
    {
        public abstract void Validate(string line, int lineNumber, ParseResult result);

        protected void AddError(ParseResult result, int lineNumber,
                              string message, ErrorType type, string snippet)
        {
            result.Errors.Add(new ErrorInfo
            {
                LineNumber = lineNumber,
                Message = message,
                Type = type,
                CodeSnippet = snippet
            });
        }
    }
}
