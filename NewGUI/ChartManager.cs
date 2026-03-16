using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace NewGUI
{
    public class ChartManager : IDisposable
    {
        private readonly Chart _chart; // Odkaz na WinForms Chart, do kterého se kreslí
        private readonly ValueDisplayManager _valueMgr; // Pomocník pro zobrazení textu posledních hodnot (může být null)
        private readonly Action<string> _log; // Delegát pro logování zpráv
        private readonly ConcurrentQueue<FrameData> _queue = new ConcurrentQueue<FrameData>(); // Fronta rámců k vykreslení (thread-safe)
        private readonly Timer _timer; // Periodické vykreslování (UI vlákno)
        private readonly Random _rnd = new Random(); // Náhodná barva pro nové řady
        private readonly int _maxPoints; // Max počet bodů na řadě (rolling okno)
        private int _sampleCount = 0; // Počítadlo vzorků (X osa)
        private bool _disposed; // Indikátor, zda byl objekt zlikvidován


        // Konstruktor – nastaví timer, interval, limit bodů a připraví Chart
        public ChartManager(Chart chart, ValueDisplayManager valueMgr, Action<string> log, int intervalMs = 100, int maxPoints = 50)
        {
            _chart = chart ?? throw new ArgumentNullException(nameof(chart)); // Chart je povinný
            _valueMgr = valueMgr; // může být null – hodnoty do UI se pak neukazují
            _log = log ?? (_ => { }); // pokud není log, použijeme prázdný delegát
            _maxPoints = Math.Max(10, maxPoints); // minimálně 10 bodů, i kdyby někdo poslal menší číslo

            _timer = new Timer(); // WinForms timer – tiká na UI vlákně
            _timer.Interval = Math.Max(10, intervalMs); // ochrana – moc malý interval zvýšíme
            _timer.Tick += Timer_Tick; // co dělat při každém tiknutí (vykreslování fronty)
            _timer.Start(); // spustíme periodu

            EnsureChartArea(); // zajistíme, že graf má aspoň jednu ChartArea
        }

        public void SetInterval(int ms)
        {
            if (_disposed) return; // nic nedělat, pokud už jsou zlikvidované
            if (ms < 10) ms = 100; // ochrana proti extrémně malým intervalům
            _timer.Interval = ms; // nastavíme nový interval
        }

        // Ovládání timeru – start/stop
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
            catch
            {
                // ignore reset errors
            }
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
                    _log?.Invoke($"[GRAPH] {variableName} -> {numericValue}"); // log pro debugging
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

            bool any = false; // indikátor, zda se něco přidalo
            FrameData last = null; // poslední zpracovaný rámec

            while (_queue.TryDequeue(out var frame)) // Vytahat všechny čekající rámce z fronty
            {
                last = frame;
                any = true;

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
    }
}
