using System;
using System.IO.Ports;
using System.Text;

namespace NewGUI
{
    public sealed class SerialManager
    {
        private static readonly SerialManager _instance = new SerialManager();
        public static SerialManager Instance => _instance;

        private readonly SerialPort _port = new SerialPort();
        private readonly object _ioLock = new object();
        private SerialDataReceivedEventHandler _attachedHandler;

        private SerialManager()
        {
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
        }

        public void Close()
        {
            try
            {
                DetachReceiver();
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

    }
}
