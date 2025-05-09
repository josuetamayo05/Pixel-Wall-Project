using System;
using System.Collections.Generic;

namespace PixelW
{
    internal class VariableManager
    {
        private Dictionary<string, int> variables = new Dictionary<string, int>();
        public void Assign(string varName, int value)
        {
            variables[varName] = value;
        }
        public int GetValue(string varName)
        {
            if (variables.TryGetValue(varName, out var value))
                return value;
            throw new Exception($"Variable no definida: '{varName}'");
        }

        public bool Exists(string varName)
        {
            if (string.IsNullOrEmpty(varName))
                return false; // Caso borde

            return variables.ContainsKey(varName);
        }
    }
}