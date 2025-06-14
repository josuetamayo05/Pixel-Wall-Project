using System;
using System.Collections.Generic;
using System.Linq;

namespace PixelW
{
    internal class VariableManager
    {
        private Dictionary<string, int> numericVars = new Dictionary<string, int>();
        private Dictionary<string, bool> booleanVars = new Dictionary<string, bool>();

        public bool Exists(string varName)
        {
            if (string.IsNullOrWhiteSpace(varName))
                return false;

            return numericVars.ContainsKey(varName) || booleanVars.ContainsKey(varName);
        }
        public void Assign(string varName, object value)
        {
            if (!IsValidVariableName(varName))
                throw new Exception($"Nombre de variable inválido: '{varName}'");

            if (value is int intValue)
            {
                numericVars[varName] = intValue;
            }
            else if (value is bool boolValue)
            {
                booleanVars[varName] = boolValue;
            }
            else
            {
                throw new Exception($"Tipo de valor no soportado para variable '{varName}'");
            }
        }

        public object GetValue(string varName)
        {
            if (numericVars.TryGetValue(varName, out int numValue))
                return numValue;
            if (booleanVars.TryGetValue(varName, out bool boolValue))
                return boolValue;

            throw new Exception($"Variable no definida: '{varName}'");
        }

        public bool IsValidVariableName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            // Primer carácter debe ser letra
            if (!char.IsLetter(name[0])) return false;

            // Caracteres permitidos: letras, números, guiones bajos
            foreach (char c in name)
            {
                if (!char.IsLetterOrDigit(c) && c != '_')
                    return false;
            }

            // Palabras reservadas (opcional)
            string[] reserved = { "and", "or", "not", "true", "false" };
            if (reserved.Contains(name.ToLower())) return false;

            return true;
        }
    }
}