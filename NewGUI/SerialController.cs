using System;
using System.Collections.Generic;

namespace NewGUI
{
    public class SerialController : IDisposable
    {
        private readonly SerialParser _parser = new SerialParser();
        private bool _attached = false;

        public event EventHandler<InitEventArgs> InitReceived;
        public event EventHandler<DataFrameEventArgs> DataFrameReceived;
        public event EventHandler<RawLineEventArgs> RawLineReceived;

        public SerialController()
        {
            _parser.InitReceived += OnParserInitReceived;
            _parser.DataFrameReceived += OnParserDataFrameReceived;
            _parser.RawLineReceived += OnParserRawLineReceived;
        }

        public bool IsOpen => SerialManager.Instance.IsOpen;

        public void ConfigurePort(
            string portName,
            int baudRate = 115200,
            System.IO.Ports.Parity parity = System.IO.Ports.Parity.None,
            int dataBits = 8,
            System.IO.Ports.StopBits stopBits = System.IO.Ports.StopBits.One,
            System.IO.Ports.Handshake handshake = System.IO.Ports.Handshake.None,
            string newLine = "\n")
        {
            SerialManager.Instance.ConfigurePort(portName, baudRate, parity, dataBits, stopBits, handshake, newLine);
        }

        public void Open()
        {
            SerialManager.Instance.Open();
            AttachIfNeeded();
        }

        public void Close()
        {
            DetachIfNeeded();
            try { SerialManager.Instance.DetachReceiver(); } catch { }
            try { SerialManager.Instance.Close(); } catch { }
        }

        public void WriteLine(string line)
        {
            SerialManager.Instance.WriteLine(line);
        }

        private void AttachIfNeeded()
        {
            if (_attached) return;
            try
            {
                SerialManager.Instance.LinesReceived += OnLinesReceived;
                _attached = true;
            }
            catch { }
        }

        private void DetachIfNeeded()
        {
            if (!_attached) return;
            try
            {
                SerialManager.Instance.LinesReceived -= OnLinesReceived;
            }
            catch { }
            _attached = false;
        }

        private void OnLinesReceived(object sender, LinesEventArgs e)
        {
            if (e?.Lines == null || e.Lines.Length == 0) return;
            foreach (var l in e.Lines)
            {
                try { _parser.ProcessLine(l); } catch { }
            }
        }

        private void OnParserInitReceived(object sender, InitEventArgs e) => InitReceived?.Invoke(this, e);
        private void OnParserDataFrameReceived(object sender, DataFrameEventArgs e) => DataFrameReceived?.Invoke(this, e);
        private void OnParserRawLineReceived(object sender, RawLineEventArgs e) => RawLineReceived?.Invoke(this, e);

        public void Dispose()
        {
            DetachIfNeeded();
            try { _parser.InitReceived -= OnParserInitReceived; } catch { }
            try { _parser.DataFrameReceived -= OnParserDataFrameReceived; } catch { }
            try { _parser.RawLineReceived -= OnParserRawLineReceived; } catch { }
        }
    }
}
