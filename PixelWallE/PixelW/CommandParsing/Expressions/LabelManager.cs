using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PixelW.CommandParsing.Expressions
{
    internal class LabelManager
    {
        private readonly Dictionary<string, int> _labels = new Dictionary<string, int>();
        private readonly HashSet<string> _allLabels = new HashSet<string>();

        public void AddLabel(string labelName, int lineNumber)
        {
            if (!IsValidLabelName(labelName))
                throw new Exception($"Nombre de etiqueta inválido: '{labelName}'");

            if (_labels.ContainsKey(labelName))
                throw new Exception($"Etiqueta duplicada: '{labelName}'");

            _labels[labelName] = lineNumber;
            _allLabels.Add(labelName);
        }
        public bool LabelExists(string labelName)
        {
            return _allLabels.Contains(labelName);
        }

        public bool TryGetLabelLine(string labelName, out int lineNumber)
        {
            return _labels.TryGetValue(labelName, out lineNumber);
        }

        public bool IsLabelLine(string line)
        {
            return !string.IsNullOrWhiteSpace(line) &&
                   !line.StartsWith("Spawn(") &&
                   !line.StartsWith("Color(") &&
                   !line.StartsWith("GoTo") &&
                   Regex.IsMatch(line, @"^[a-zA-Z][a-zA-Z0-9_-]*$");
        }

        private bool IsValidLabelName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (char.IsDigit(name[0]) || name[0] == '-')
                return false;

            return name.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-');
        }
    }
}
