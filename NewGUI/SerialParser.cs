using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewGUI
{
    public class InitEventArgs : EventArgs
    {
        public string Payload { get; }
        public InitEventArgs(string payload) { Payload = payload; }
    }

    public class DataFrameEventArgs : EventArgs
    {
        public string Line { get; }
        public DataFrameEventArgs(string line) { Line = line; }
    }

    public class RawLineEventArgs : EventArgs
    {
        public string Line { get; }
        public RawLineEventArgs(string line) { Line = line; }
    }

    /// <summary>
    /// SerialParser: rozpozná typ øádky (INIT seznam / mìøicí rámec / ostatní) a vyvolá patøièné události.
    /// UI potom mùže reagovat na události místo ruèního volání IsInitLine/ParseAndDisplayData.
    /// </summary>
    public class SerialParser
    {
        public event EventHandler<InitEventArgs> InitReceived;
        public event EventHandler<DataFrameEventArgs> DataFrameReceived;
        public event EventHandler<RawLineEventArgs> RawLineReceived;

        public void ProcessLine(string rawLine)
        {
            if (string.IsNullOrWhiteSpace(rawLine)) return;

            string line = rawLine.Trim();
            line = line.TrimStart('\uFEFF'); // odstranit BOM

            if (IsInitLine(line, out string payload))
            {
                InitReceived?.Invoke(this, new InitEventArgs(payload));
                return;
            }

            if (line.StartsWith("?id=", StringComparison.OrdinalIgnoreCase))
            {
                DataFrameReceived?.Invoke(this, new DataFrameEventArgs(line));
                return;
            }

            RawLineReceived?.Invoke(this, new RawLineEventArgs(line));
        }

        private static bool IsInitLine(string line, out string payload)
        {
            payload = null;
            if (string.IsNullOrWhiteSpace(line)) return false;

            // èistý "id:type,id:type"
            if (!line.StartsWith("?") && line.Contains(":") && line.Contains(","))
            {
                payload = line.Trim();
                return true;
            }

            // varianta s prefixem "?type=INIT&..."
            if (line.StartsWith("?type=INIT", StringComparison.OrdinalIgnoreCase))
            {
                int amp = line.IndexOf('&');
                payload = (amp >= 0) ? line.Substring(amp + 1).Trim() : line.Substring("?type=INIT".Length).TrimStart('&', ' ');
                return true;
            }

            return false;
        }

        /// <summary>
        /// Pomocná utilita: rozparsuje query string (s nebo bez poèáteèního '?') do slovníku.
        /// </summary>
        public static Dictionary<string, string> ParseQuery(string query)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(query)) return dict;

            string s = query;
            if (s.StartsWith("?")) s = s.Substring(1);
            var parts = s.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
            {
                var kv = p.Split(new[] { '=' }, 2);
                if (kv.Length == 2)
                {
                    var key = Uri.UnescapeDataString(kv[0]);
                    var val = Uri.UnescapeDataString(kv[1]);
                    dict[key] = val;
                }
            }

            return dict;
        }
    }
}
