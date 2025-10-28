using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewGUI
{
    /// <summary>
    /// Jednoduchý thread-safe buffer + rámcovaè pro pøíchozí text ze sériového portu.
    /// - Volá se z DataReceived handleru: Append(rawChunk)
    /// - UI periodicnì zavolá DrainLines(), která vrátí kompletní, "vyèištìné" øádky a odstraní je z bufferu.
    /// Cíl: pøesunout framing/èištìní mimo UI kód a omezit šíøení lockù/voleb pøímo v UI.
    /// </summary>
    public class SerialFrameProcessor
    {
        private readonly StringBuilder _buffer = new StringBuilder();
        private readonly object _lock = new object();

        /// <summary>
        /// Pøidá fragment textu do interního bufferu. Mùže být voláno z libovolného vlákna.
        /// </summary>
        public void Append(string chunk)
        {
            if (string.IsNullOrEmpty(chunk)) return;
            lock (_lock)
            {
                _buffer.Append(chunk);
            }
        }

        /// <summary>
        /// Vrátí kompletní øádky (bez koncových CR/LF), které jsou aktuálnì v bufferu.
        /// Èásteèná poslední øádka zùstane v bufferu.
        /// Øádky jsou zároveò "vyèištìny" podobnì jako v pùvodním kódu (`DisplayTimer_Tick`).
        /// </summary>
        public string[] DrainLines()
        {
            string work;
            lock (_lock)
            {
                if (_buffer.Length == 0) return Array.Empty<string>();
                work = _buffer.ToString();
            }

            // Najdeme poslední '\n' - pouze èásti pøed tím jsou kompletní øádky
            int lastNewline = work.LastIndexOf('\n');
            if (lastNewline < 0)
            {
                // žádná kompletní øádka zatím
                return Array.Empty<string>();
            }

            string complete = work.Substring(0, lastNewline + 1); // vèetnì posledního \n
            string remaining = work.Substring(lastNewline + 1);

            // uložíme zpìt zbytek do bufferu (thread-safe)
            lock (_lock)
            {
                _buffer.Clear();
                if (!string.IsNullOrEmpty(remaining)) _buffer.Append(remaining);
            }

            // Rozsekáme kompletní èást na øádky, odstraníme CR a provedeme èištìní každé øádky.
            complete = complete.Replace("\r", "");
            var rawLines = complete.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new List<string>(rawLines.Length);

            foreach (var raw in rawLines)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                var line = raw.Trim();
                line = line.TrimStart('\uFEFF'); // odstranit BOM pokud pøítomen

                // odstranit kontrolní znaky, ponechat bìžné tisknutelné znaky (vèetnì interpunkce)
                var cleaned = new string(line.Where(ch => !char.IsControl(ch) || ch == '?' || ch == '=' || ch == '&' || ch == '.' || ch == ',' || ch == '-' || char.IsLetterOrDigit(ch)).ToArray());
                cleaned = cleaned.Trim();
                if (!string.IsNullOrEmpty(cleaned)) result.Add(cleaned);
            }

            return result.ToArray();
        }
    }
}
