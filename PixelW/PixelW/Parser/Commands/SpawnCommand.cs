public class SpawnCommand : ICommandProcessor
{
    public bool CanProcess(string line) => line.TrimStart().StartsWith("Spawn(");
    public void Process(string line, ParseResult result, int currentLineNumber, WallE robot, VariableManager variables)
    {
        try
        {
            var parts = line.Split(new[] { '(', ',', ')' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
            {
                throw new Exception("Sintaxis incorrecta para Spawn. Uso: Spawn(x, y)");
            }

            var evaluator = new NumericExpressionEvaluator(variables);
            int x = evaluator.Evaluate(parts[1].Trim());
            int y = evaluator.Evaluate(parts[2].Trim());
            robot.Spawn(x, y);
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