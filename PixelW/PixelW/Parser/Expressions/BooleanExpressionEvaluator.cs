public class BooleanExpressionEvaluator : ExpressionEvaluator
{
    public BooleanExpressionEvaluator(VariableManager variables) : base(variables) { }
    public bool Evaluate(string expr)
    {
        expr = expr.Trim();
        expr = EvaluateParenthesis(expr);
        string[] operators = { "&&", "||" };
            foreach (var op in operators)
            {
                if (expr.Contains(op))
                {
                    var parts = expr.Split(new[] { op }, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        bool left = EvaluateBooleanExpression(parts[0]);
                        bool right = EvaluateBooleanExpression(parts[1]);
                        return op == "&&" ? left && right : left || right;
                    }
                }
            }

            return EvaluateSimpleComparison(expr);
    }
    public string EvaluateParentheses(string expr)
    {
        while (expr.Contains("(") && expr.Contains(")"))
        {
            int lastOpen = expr.LastIndexOf('(');
            int close = expr.IndexOf(')', lastOpen);

            if (close == -1) break;

            string inner = expr.Substring(lastOpen + 1, close - lastOpen - 1);
            bool innerResult = EvaluateBooleanExpression(inner);
            expr = expr.Remove(lastOpen, close - lastOpen + 1)
                      .Insert(lastOpen, innerResult ? "1" : "0");
        }
        return expr;
    }
    private bool EvaluateSimpleComparison(string expr)
        {
            string[] comparators = { "==", "!=", "<=", ">=", "<", ">" };
            foreach (var comp in comparators)
            {
                if (expr.Contains(comp))
                {
                    var parts = expr.Split(new[] { comp }, StringSplitOptions.None);
                    if (parts.Length != 2) continue;

                    int left = EvaluateNumericExpression(parts[0].Trim());
                    int right = EvaluateNumericExpression(parts[1].Trim());

                    switch (comp)
                    {
                        case "==": return left == right;
                        case "!=": return left != right;
                        case "<=": return left <= right;
                        case ">=": return left >= right;
                        case "<": return left < right;
                        case ">": return left > right;
                    }
                }
            }

            return EvaluateNumericExpression(expr) != 0;
        }
}