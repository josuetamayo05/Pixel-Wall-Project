public abstract class ExpressionEvaluator
{
    protected readonly VariableManager _variables;
    protected ExpressionEvaluator(VariableManager variables)
    {
        _variables = variables;
    }
    protected int EvaluateParameter(string param)
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
}