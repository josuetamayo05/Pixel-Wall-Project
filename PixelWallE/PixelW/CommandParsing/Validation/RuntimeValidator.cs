using PixelW.CommandParsing.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PixelW.CommandParser;

namespace PixelW.CommandParsing.Expressions
{
    internal class RuntimeValidator : ErrorValidator
    {
        private readonly WallE _robot;

        public RuntimeValidator(WallE robot)
        {
            _robot = robot;
        }

        public override void Validate(string line, int lineNumber, ParseResult result)
        {
            if (line.StartsWith("Spawn("))
            {
                if (!IsWithinCanvasBounds(/* parámetros */))
                {
                    AddError(result, lineNumber,
                            "Posición inicial fuera del canvas",
                            ErrorType.Runtime, line);
                }
            }
        }

        private bool IsWithinCanvasBounds(/* parámetros */)
        {
            // Implementación de la validación
            return true;
        }
    }
}
