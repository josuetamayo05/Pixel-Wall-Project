public class ErrorInfo
{
    public int LineNumber { get; set; }
    public string Message { get; set; }
    public ErrorType Type { get; set; }
    public string CodeSnippet {  get; set; }
}