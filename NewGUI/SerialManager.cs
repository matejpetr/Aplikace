using System;
using System.IO.Ports;
using System.Text;
using System.Collections.Generic;

namespace NewGUI
{
    public sealed class SerialManager
    {
        private static readonly SerialManager _instance = new SerialManager();
        public static SerialManager Instance => _instance;

        private readonly SerialPort _port = new SerialPort();
        private readonly object _ioLock = new object();
        private SerialDataReceivedEventHandler _attachedHandler;

        // internal line buffering
        private readonly StringBuilder _lineBuffer = new StringBuilder();
        private readonly object _bufLock = new object();
        private readonly SerialDataReceivedEventHandler _internalDataReceivedHandler;
        private bool _internalHandlerAttached = false;

        public event EventHandler<LinesEventArgs> LinesReceived;

        public SerialManager()
        {
            _internalDataReceivedHandler = InternalDataReceived;
            _port.ReadTimeout = 500;
            _port.WriteTimeout = 500;
            _port.NewLine = "\r\n";
        }

        public bool IsOpen => _port.IsOpen;
        public string PortName => _port.PortName;
        public int BaudRate => _port.BaudRate;

        public void ConfigurePort(
            string portName,
            int baudRate = 115200,
            Parity parity = Parity.None,
            int dataBits = 8,
            StopBits stopBits = StopBits.One,
            Handshake handshake = Handshake.None,
            string newLine = "\n")
        {
            if (IsOpen) throw new InvalidOperationException("Nejdřív zavři port (Close), pak měň konfiguraci.");

            _port.PortName = portName;
            _port.BaudRate = baudRate;
            _port.Parity = parity;
            _port.DataBits = dataBits;
            _port.StopBits = stopBits;
            _port.Handshake = handshake;
            _port.NewLine = newLine;
        }

        public void Open()
        {
            if (!IsOpen) _port.Open();

            // ensure our internal handler is attached once
            if (!_internalHandlerAttached)
            {
                _port.DataReceived += _internalDataReceivedHandler;
                _internalHandlerAttached = true;
            }
        }

        public void Close()
        {
            try
            {
                DetachReceiver();
                if (_internalHandlerAttached)
                {
                    try { _port.DataReceived -= _internalDataReceivedHandler; } catch { }
                    _internalHandlerAttached = false;
                }

                if (IsOpen) _port.Close();
            }
            catch { /* log/ignore */ }
        }

        public void AttachExclusiveReceiver(SerialDataReceivedEventHandler handler)
        {
            DetachReceiver();
            if (handler != null)
            {
                _port.DataReceived += handler;
                _attachedHandler = handler;
            }
        }

        public void DetachReceiver()
        {
            if (_attachedHandler != null)
            {
                try { _port.DataReceived -= _attachedHandler; } catch { }
                _attachedHandler = null;
            }
        }

        public void WriteLine(string line)
        {
            if (!IsOpen) throw new InvalidOperationException("Port není otevřen.");
            lock (_ioLock) _port.WriteLine(line);
        }

        public void Write(string text)
        {
            if (!IsOpen) throw new InvalidOperationException("Port není otevřen.");
            lock (_ioLock) _port.Write(text);
        }

        public void DiscardInOut()
        {
            try
            {
                if (!_port.IsOpen) return;
                _port.DiscardInBuffer();
                _port.DiscardOutBuffer();
            }
            catch { /* ignore */ }
        }

        private void InternalDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = _port.ReadExisting();
                if (string.IsNullOrEmpty(data)) return;

                List<string> completeLines = null;

                lock (_bufLock)
                {
                    _lineBuffer.Append(data);
                    var buf = _lineBuffer.ToString();
                    int lastNewline = buf.LastIndexOf('\n');
                    if (lastNewline >= 0)
                    {
                        string complete = buf.Substring(0, lastNewline + 1);
                        string remaining = buf.Substring(lastNewline + 1);
                        _lineBuffer.Clear();
                        if (!string.IsNullOrEmpty(remaining)) _lineBuffer.Append(remaining);

                        // normalize and split lines
                        complete = complete.Replace("\r", "");
                        var rawLines = complete.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        completeLines = new List<string>(rawLines.Length);
                        foreach (var raw in rawLines)
                        {
                            if (string.IsNullOrWhiteSpace(raw)) continue;
                            completeLines.Add(raw.Trim());
                        }
                    }
                }

                if (completeLines != null && completeLines.Count > 0)
                {
                    try
                    {
                        LinesReceived?.Invoke(this, new LinesEventArgs(completeLines.ToArray()));
                    }
                    catch { /* subscriber exceptions should not crash serial thread */ }
                }
            }
            catch { /* ignore read errors */ }
        }
    }

    public class LinesEventArgs : EventArgs
    {
        public string[] Lines { get; }
        public LinesEventArgs(string[] lines) { Lines = lines ?? Array.Empty<string>(); }
    }
}
