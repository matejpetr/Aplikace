using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace NewGUI
{
    public class ChartManager : IDisposable
    {
        private readonly Chart _chart;
        private readonly ValueDisplayManager _valueMgr;
        private readonly Action<string> _log;
        private readonly ConcurrentQueue<FrameData> _queue = new ConcurrentQueue<FrameData>();
        private readonly Timer _timer;
        private readonly Random _rnd = new Random();
        private readonly int _maxPoints;

        private int _sampleCount = 0;
        private bool _disposed;

        private readonly int _maxSamples;
        private bool _limitReached;

        // NEW: full-history buffer for export (independent of visible rolling window)
        // index -> (seriesName -> value)
        private readonly SortedDictionary<int, Dictionary<string, double>> _history
            = new SortedDictionary<int, Dictionary<string, double>>();
        private readonly object _historyLock = new object();

        public event EventHandler MaxSamplesReached;

        public ChartManager(Chart chart, ValueDisplayManager valueMgr, Action<string> log, int intervalMs = 100, int maxPoints = 50, int maxSamples = 10000)
        {
            _chart = chart ?? throw new ArgumentNullException(nameof(chart));
            _valueMgr = valueMgr;
            _log = log ?? (_ => { });
            _maxPoints = Math.Max(10, maxPoints);
            _maxSamples = Math.Max(1, maxSamples);

            _timer = new Timer();
            _timer.Interval = Math.Max(10, intervalMs);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            EnsureChartArea();
        }

        public void SetInterval(int ms)
        {
            if (_disposed) return;
            if (ms < 10) ms = 100;
            _timer.Interval = ms;
        }

        public void Start() { if (!_disposed) _timer.Start(); }
        public void Stop() { if (!_disposed) _timer.Stop(); }

        /// <summary>
        /// Vynuluje graf pro nové měření: smaže pending data, řady (a tím i legendu), body a počítadlo vzorků.
        /// </summary>
        public void Reset()
        {
            if (_disposed) return;

            // vyprázdnit čekající rámce
            while (_queue.TryDequeue(out _)) { }

            // vynulovat počítadlo vzorků
            System.Threading.Interlocked.Exchange(ref _sampleCount, 0);
            _limitReached = false;

            lock (_historyLock)
            {
                _history.Clear();
            }

            // vyčistit zobrazení hodnot
            try { _valueMgr?.UpdateValueText(string.Empty); } catch { }

            // vyčistit graf (řady/bodové série) -> zmizí i legenda
            try
            {
                if (_chart.InvokeRequired)
                {
                    _chart.BeginInvoke((Action)(() =>
                    {
                        _chart.Series.Clear();
                        _chart.Invalidate();
                    }));
                }
                else
                {
                    _chart.Series.Clear();
                    _chart.Invalidate();
                }
            }
            catch { }
        }

        // Přidání rámce (už naparsovaných čísel) do fronty pro vykreslení
        public void Enqueue(FrameData frame)
        {
            if (frame == null) return; // ochrana
            _queue.Enqueue(frame); // thread-safe
        }

        public void ParseAndEnqueue(string data) // Parsování surového textu a přidání do fronty
        {
            if (string.IsNullOrWhiteSpace(data)) return; // prázdné nic nedělá
            if (_limitReached) return; // pokud byl překročen limit vzorků, nic nedělat

            string s = data.Trim(); // osekat mezery
            s = s.TrimStart('\uFEFF'); // odstranit případný BOM (Byte Order Mark) značka pořadí bajtů
            if (s.StartsWith("?")) s = s.Substring(1); // tolerovat prefix „?“ (jako v URL query)

            // Rozdělit na dvojice klíč=hodnota (a=1&b=2 → {"a":"1","b":"2"})
            var parameters = s.Split('&')
                              .Select(part => part.Split('='))
                              .Where(pair => pair.Length == 2)
                              .ToDictionary(pair => pair[0], pair => pair[1]);

            // Klíče, které se do grafu NEKRESLÍ (meta-informace)
            var skipKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
             { "type", "id", "pin", "app", "version", "dbversion", "api", "status", "code" };

            // Filtrovat jen datové klíče (ostatní ignorovat)
            var dataForGraph = parameters
                .Where(kvp => !skipKeys.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var numericPairs = new List<string>(); // Pro sestavení textového přehledu
            var numericValues = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase); // Pro číselné hodnoty

            // Pro každý datový klíč se pokusí o vytažení čísla (toleruje „3,14“ i „3.14“, vědecký zápis)
            foreach (var kvp in dataForGraph)
            {
                string variableName = kvp.Key; // název veličiny
                string raw = kvp.Value ?? string.Empty; // původní textová hodnota

                // Normalizace desetinné čárky: „3,14“ → „3.14“ (jen pokud není tečka)
                string normalized = raw;
                if (normalized.IndexOf(',') >= 0 && normalized.IndexOf('.') < 0)
                    normalized = normalized.Replace(',', '.');

                // Regex najde první číslo v textu (podporuje znaménko, desetinnou tečku i exponent)
                var m = System.Text.RegularExpressions.Regex.Match(
                            normalized, @"[-+]?\d+(?:\.\d+)?(?:[eE][-+]?\d+)?");

                double numericValue = 0.0;
                bool hasNumber = m.Success && double.TryParse(
                    m.Value,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out numericValue);

                if (hasNumber) // Připraví textové „name=value“ a ulož číselnou hodnotu
                {
                    numericPairs.Add($"{variableName}={numericValue.ToString("G", System.Globalization.CultureInfo.InvariantCulture)}");
                    numericValues[variableName] = numericValue;
                }
                else
                {
                    // Nebylo číslo – jen zalogujeme původní text
                    _log?.Invoke($"{variableName}: {raw}");
                }
            }
            // Pokud máme aspoň jednu číselnou hodnotu, vytvoříme FrameData a pošleme do fronty
            if (numericValues.Count > 0)
            {
                var text = string.Join(", ", numericPairs); // textový přehled
                int idx = System.Threading.Interlocked.Increment(ref _sampleCount); // atomicky zvýší index vzorku
                var frame = new FrameData(idx, numericValues, text); // balíček dat
                _queue.Enqueue(frame); // zařadit ke kreslení
            }
        }

        // Tik timeru – přesune všechno z fronty do grafu a aktualizuje UI
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_disposed) return; // nic nedělat, pokud už jsou zlikvidované
            if (_limitReached) return; // pokud byl překročen limit vzorků, nic nedělat

            bool any = false; // indikátor, zda se něco přidalo
            FrameData last = null; // poslední zpracovaný rámec

            while (_queue.TryDequeue(out var frame)) // Vytahat všechny čekající rámce z fronty
            {
                last = frame;
                any = true;

                // store full history for export
                lock (_historyLock)
                {
                    if (!_history.TryGetValue(frame.Index, out var row))
                    {
                        row = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                        _history[frame.Index] = row;
                    }
                    foreach (var kv in frame.Values)
                        row[kv.Key] = kv.Value;
                }

                foreach (var kv in frame.Values) // Pro každou veličinu v rámci
                {
                    var variableName = kv.Key; // název řady/veličiny
                    var numericValue = kv.Value; // její hodnota

                    if (_chart.Series.IsUniqueName(variableName)) // Pokud pro tuto veličinu ještě neexistuje Series, vytvoříme ji
                    {
                        var s = new Series(variableName)
                        {
                            ChartType = SeriesChartType.Line, // kreslit čáru
                            BorderWidth = 2, // tloušťka
                            Color = Color.FromArgb(_rnd.Next(256), _rnd.Next(256), _rnd.Next(256)) // náhodná barva
                        };
                        _chart.Series.Add(s);
                    }

                    var series = _chart.Series[variableName]; // Vezmi existující řadu

                    if (series.Points.Count > _maxPoints) series.Points.RemoveAt(0); // Udržovat maximální počet bodů: když je jich moc, odstraň nejstarší

                    series.Points.AddXY(frame.Index, numericValue); // Přidat nový bod: X = index vzorku, Y = hodnota
                }

                // enforce max samples using the frame index (=sample counter)
                if (frame.Index >= _maxSamples)
                {
                    _limitReached = true;
                    try { _timer.Stop(); } catch { }
                    try { MaxSamplesReached?.Invoke(this, EventArgs.Empty); } catch { }
                    break;
                }
            }
            // Pokud jsme něco zpracovali – zaktualizuj osy, UI a překresli graf
            if (any)
            {
                // Graf musí mít ChartArea (pro jistotu)
                if (_chart.ChartAreas.Count == 0)
                    _chart.ChartAreas.Add(new ChartArea());

                var ca = _chart.ChartAreas[0];

                // Osa X: používej poslední index rámce (po Reset() začne zase od 1)
                int lastIndex = (last != null) ? last.Index : _sampleCount;
                ca.AxisX.Minimum = Math.Max(0, lastIndex - 10);
                ca.AxisX.Maximum = lastIndex;
                ca.RecalculateAxesScale(); // přepočti měřítko (zejména Y)

                // Do UI panelu s hodnotami pošli text posledního rámce (pokud existuje ValueDisplayManager)
                if (last != null && _valueMgr != null)
                {
                    _valueMgr.UpdateValueText(last.ValueText);
                }
                // Vyžádat překreslení grafu
                _chart.Invalidate();
            }
        }
        // Ujisti se, že graf má aspoň jednu ChartArea (jinak nejde kreslit)
        private void EnsureChartArea()
        {
            if (_chart.ChartAreas.Count == 0)
                _chart.ChartAreas.Add(new ChartArea());
        }
        // Uklid při likvidaci – odpojit timer a uvolnit zdroje
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try { _timer.Tick -= Timer_Tick; } catch { } // odhlásit handler
            try { _timer.Stop(); } catch { } // zastavit
            try { _timer.Dispose(); } catch { } // uvolnit
        }

        /// <summary>
        /// Exportuje všechna naměřená data (z interní historie, ne jen viditelné okno v grafu) do CSV.
        /// Format: Sample;Series1;Series2;...
        /// </summary>
        public string ExportCsv(char separator = ';', bool forceText = false, bool decimalComma = true)
        {
            if (_disposed) return string.Empty;

            // capture snapshot to avoid holding lock during formatting
            SortedDictionary<int, Dictionary<string, double>> snapshot;
            HashSet<string> allSeries;
            lock (_historyLock)
            {
                if (_history.Count == 0) return string.Empty;

                snapshot = new SortedDictionary<int, Dictionary<string, double>>();
                allSeries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var kv in _history)
                {
                    var rowCopy = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                    foreach (var v in kv.Value)
                    {
                        rowCopy[v.Key] = v.Value;
                        allSeries.Add(v.Key);
                    }
                    snapshot[kv.Key] = rowCopy;
                }
            }

            var seriesList = allSeries.OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();

            var sb = new StringBuilder();
            sb.Append("Sample");
            foreach (var s in seriesList)
                sb.Append(separator).Append(EscapeCsv(s, separator));
            sb.AppendLine();

            foreach (var row in snapshot)
            {
                string xText = row.Key.ToString(System.Globalization.CultureInfo.InvariantCulture);
                sb.Append(forceText ? ToExcelText(xText) : xText);

                for (int i = 0; i < seriesList.Count; i++)
                {
                    sb.Append(separator);
                    if (row.Value.TryGetValue(seriesList[i], out var y))
                    {
                        string yText = y.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        if (decimalComma) yText = yText.Replace('.', ',');
                        sb.Append(forceText ? ToExcelText(yText) : yText);
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string ToExcelText(string value)
        {
            if (value == null) value = string.Empty;
            value = value.Replace("\"", "\"");
            return "=\"" + value + "\"";
        }

        private static string EscapeCsv(string value, char separator)
        {
            if (value == null) return string.Empty;
            bool mustQuote = value.IndexOf(separator) >= 0 || value.IndexOf('"') >= 0 || value.IndexOf('\n') >= 0 || value.IndexOf('\r') >= 0;
            if (!mustQuote) return value;
            return "\"" + value.Replace("\"", "\"") + "\"";
        }
    }
}
