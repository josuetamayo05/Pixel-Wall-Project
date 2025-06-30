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
            expression = expression.Trim();

            if (expression.Contains("(") && expression.Contains(")"))
            {
                expression = HandleParentheses(expression);
            }

            // manejar operadores unarios (-)
            if (expression.StartsWith("-"))
            {
                return -EvaluateNumericExpression(expression.Substring(1));
            }

            // eval operadores por precedencia
            string[][] operatorGroups = new[]
            {
                new[] { "+", "-" },    
                new[] { "*", "/", "%" }, 
                new[] { "**" }         
            };

            // evaluar en orden de precedencia 
            foreach (var operators in operatorGroups)
            {
                // buscar el operador más a la derecha para mantener asociatividad izquierda
                int opIndex = -1;
                string opFound = null;

                for (int i = expression.Length - 1; i >= 0; i--)
                {
                    foreach (var op in operators)
                    {
                        if (i >= op.Length - 1 && expression.Substring(i - op.Length + 1, op.Length) == op)
                        {
                            // verif que no es un operador unario
                            bool isUnary = op == "-" && (i == 0 || "+-*/%(".Contains(expression[i - 1]));

                            if (!isUnary)
                            {
                                opIndex = i - op.Length + 1;
                                opFound = op;
                                break;
                            }
                        }
                    }
                    if (opFound != null) break;
                }

                if (opIndex >= 0)
                {
                    string left = expression.Substring(0, opIndex).Trim();
                    string right = expression.Substring(opIndex + opFound.Length).Trim();

                    int leftVal = EvaluateNumericExpression(left);
                    int rightVal = EvaluateNumericExpression(right);

                    switch (opFound)
                    {
                        case "+": return leftVal + rightVal;
                        case "-": return leftVal - rightVal;
                        case "*": return leftVal * rightVal;
                        case "/":
                            if (rightVal == 0) throw new DivideByZeroException();
                            return leftVal / rightVal;
                        case "%": return leftVal % rightVal;
                        case "**": return (int)Math.Pow(leftVal, rightVal);
                    }
                }
            }

            if (_variables.Exists(expression))
                return _variables.GetNumericValue(expression);

            if (int.TryParse(expression, out int literal)) return literal;

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

            return EvaluateNumericExpression(expr) != 0;//valores numéricos como booleanos
        }
    }
}
