public interface ICommandProcessor
{
    bool CanProcess(string line);
    void Process(string line, ParseResult result, int currentLineNumber, WallE robot, VariableManager variables);
}