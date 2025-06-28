using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace PixelW.CommandParsing.Expressions
{
    internal class ExpressionEvaluator
    {
        private readonly VariableManager _variables;

        public ExpressionEvaluator(VariableManager variables)
        {
            _variables = variables;
        }

        public bool EvaluateBooleanExpression(string expr)
        {
            expr = EvaluateParentheses(expr.Trim());

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
        public int EvaluateParameter(string param)
        {
            if (_variables.Exists(param))
            {
                var value = _variables.GetValue(param);
                if (value is int intValue) return intValue;
                throw new Exception($"La variable {param} no es numérica");
            }

            if (int.TryParse(param, out int result)) return result;

            throw new Exception($"Parámetro no válido: {param}");
        }
        public int EvaluateNumericExpression(string expression)
        {
            expression = HandleParentheses(expression.Trim());
            string[] operators = { "**", "*", "/", "%", "+", "-" };

            foreach (string op in operators)
            {
                if (expression.Contains(op))
                {
                    var parts = expression.Split(new[] { op }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2) continue;

                    int left = EvaluateNumericExpression(parts[0].Trim());
                    int right = EvaluateNumericExpression(parts[1].Trim());

                    switch (op)
                    {
                        case "**": return (int)Math.Pow(left, right);
                        case "*": return left * right;
                        case "/":
                            if (right == 0) throw new Exception("No se puede dividir por cero");
                            return left / right;
                        case "%": return left % right;
                        case "+": return left + right;
                        case "-": return left - right;
                    }
                }
            }

            if (_variables.Exists(expression))
            {
                var result = _variables.GetValue(expression);
                if (result is int intValue) return intValue;
                throw new Exception($"La variable '{expression}' no es numérica");
            }

            if (int.TryParse(expression, out int value)) return value;

            throw new Exception($"Expresión numérica no válida: '{expression}'");
        }

        private string HandleParentheses(string expression)
        {
            while (expression.Contains("(") && expression.Contains(")"))
            {
                int open = expression.LastIndexOf('(');
                int close = expression.IndexOf(')', open);
                if (close == -1) throw new Exception("Paréntesis no balanceados");

                string inner = expression.Substring(open + 1, close - open - 1);
                object result = EvaluateBooleanExpression(inner);
                expression = expression.Remove(open, close - open + 1)
                                      .Insert(open, result.ToString());
            }
            return expression;
        }

        private string EvaluateParentheses(string expr)
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
}
