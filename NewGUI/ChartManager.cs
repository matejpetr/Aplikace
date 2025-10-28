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
        private readonly Chart _chart;
        private readonly ValueDisplayManager _valueMgr;
        private readonly Action<string> _log;
        private readonly ConcurrentQueue<FrameData> _queue = new ConcurrentQueue<FrameData>();
        private readonly Timer _timer;
        private readonly Random _rnd = new Random();
        private readonly int _maxPoints;
        private int _sampleCount = 0;
        private bool _disposed;

        public ChartManager(Chart chart, ValueDisplayManager valueMgr, Action<string> log, int intervalMs = 100, int maxPoints = 50)
        {
            _chart = chart ?? throw new ArgumentNullException(nameof(chart));
            _valueMgr = valueMgr; // may be null
            _log = log ?? (_ => { });
            _maxPoints = Math.Max(10, maxPoints);

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

        public void Enqueue(FrameData frame)
        {
            if (frame == null) return;
            _queue.Enqueue(frame);
        }

        public void ParseAndEnqueue(string data)
        {
            if (string.IsNullOrWhiteSpace(data)) return;
            string s = data.Trim();
            s = s.TrimStart('\uFEFF');
            if (s.StartsWith("?")) s = s.Substring(1);

            var parameters = s.Split('&')
                              .Select(part => part.Split('='))
                              .Where(pair => pair.Length == 2)
                              .ToDictionary(pair => pair[0], pair => pair[1]);

            var skipKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
             { "type", "id", "pin", "app", "version", "dbversion", "api", "status", "code" };

            var dataForGraph = parameters
                .Where(kvp => !skipKeys.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var numericPairs = new List<string>();
            var numericValues = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in dataForGraph)
            {
                string variableName = kvp.Key;
                string raw = kvp.Value ?? string.Empty;

                string normalized = raw;
                if (normalized.IndexOf(',') >= 0 && normalized.IndexOf('.') < 0)
                    normalized = normalized.Replace(',', '.');

                var m = System.Text.RegularExpressions.Regex.Match(
                            normalized, @"[-+]?\d+(?:\.\d+)?(?:[eE][-+]?\d+)?");

                double numericValue = 0.0;
                bool hasNumber = m.Success && double.TryParse(
                    m.Value,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out numericValue);

                if (hasNumber)
                {
                    numericPairs.Add($"{variableName}={numericValue.ToString("G", System.Globalization.CultureInfo.InvariantCulture)}");
                    numericValues[variableName] = numericValue;
                    _log?.Invoke($"[GRAPH] {variableName} -> {numericValue}");
                }
                else
                {
                    _log?.Invoke($"{variableName}: {raw}");
                }
            }

            if (numericValues.Count > 0)
            {
                var text = string.Join(", ", numericPairs);
                int idx = System.Threading.Interlocked.Increment(ref _sampleCount);
                var frame = new FrameData(idx, numericValues, text);
                _queue.Enqueue(frame);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_disposed) return;

            bool any = false;
            FrameData last = null;

            while (_queue.TryDequeue(out var frame))
            {
                last = frame;
                any = true;

                foreach (var kv in frame.Values)
                {
                    var variableName = kv.Key;
                    var numericValue = kv.Value;

                    if (_chart.Series.IsUniqueName(variableName))
                    {
                        var s = new Series(variableName)
                        {
                            ChartType = SeriesChartType.Line,
                            BorderWidth = 2,
                            Color = Color.FromArgb(_rnd.Next(256), _rnd.Next(256), _rnd.Next(256))
                        };
                        _chart.Series.Add(s);
                    }

                    var series = _chart.Series[variableName];

                    if (series.Points.Count > _maxPoints) series.Points.RemoveAt(0);

                    series.Points.AddXY(frame.Index, numericValue);
                }
            }

            if (any)
            {
                if (_chart.ChartAreas.Count == 0)
                    _chart.ChartAreas.Add(new ChartArea());

                var ca = _chart.ChartAreas[0];
                ca.AxisX.Minimum = Math.Max(0, _sampleCount - 10);
                ca.AxisX.Maximum = _sampleCount;
                ca.RecalculateAxesScale();

                if (last != null && _valueMgr != null)
                {
                    _valueMgr.UpdateValueText(last.ValueText);
                }

                _chart.Invalidate();
            }
        }

        private void EnsureChartArea()
        {
            if (_chart.ChartAreas.Count == 0)
                _chart.ChartAreas.Add(new ChartArea());
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try { _timer.Tick -= Timer_Tick; } catch { }
            try { _timer.Stop(); } catch { }
            try { _timer.Dispose(); } catch { }
        }
    }
}
