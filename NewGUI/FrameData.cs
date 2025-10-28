using System.Collections.Generic;

namespace NewGUI
{
    public class FrameData
    {
        public int Index { get; }
        public IReadOnlyDictionary<string, double> Values { get; }
        public string ValueText { get; }

        public FrameData(int index, IDictionary<string, double> values, string valueText)
        {
            Index = index;
            Values = values == null ? new Dictionary<string,double>() : new Dictionary<string,double>(values);
            ValueText = valueText ?? string.Empty;
        }
    }
}
