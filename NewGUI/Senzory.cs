using System;                                                   // Základní typy a události
using System.Collections.Generic;                               // Kolekce jako List<>, Dictionary<>
using System.Data;                                              // (aktuálně nepoužito)
using System.Drawing;                                           // Barvy a grafické typy (pro graf/obrázky)
using System.Linq;                                              // LINQ operace
using System.Text;                                              // StringBuilder a textové utility
using System.Threading.Tasks;                                   // async/await Task
using System.Windows.Forms;                                     // WinForms UI
using System.IO.Ports;                                          // Sériová komunikace (SerialPort)
using System.Windows.Forms.DataVisualization.Charting;          // Ovládací prvek Chart
using System.IO;                                                // Práce se soubory a cestami
using System.Text.Json;                                         // JSON serializace/deserializace

namespace NewGUI
{
    public partial class Senzory : UserControl
    {
        private bool isSendingRequest = false;
        public string request;
        private int sampleCount = 0;
        private string lastUsedID = null;
        private Random rnd = new Random();
        private Timer comPortWatcherTimer;
        private List<string> lastKnownPorts = new List<string>();
        private readonly Dictionary<string, string> sensorIdMap
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly object _rxLock = new object();
        private string _latestDataFrame;
        private Timer displayTimer;
        private System.Threading.CancellationTokenSource _sendCts;
        private readonly StringBuilder _rxBuffer = new StringBuilder();

        private List<Komponenty> SenzoryData;
        private string _lastSentMode = null;

        // Popup okna
        private SerialPopupForm _linkForm;   // vše mimo INIT
        private SerialPopupForm _initForm;   // jen INIT odpověď

        // INIT stav
        private bool _awaitingInitResponse = false; // čekám na odpověď INIT?
        private bool _initRequestSent = false;      // byl odeslán INIT request?
        private string _lastInitPayload = null;     // poslední přijatá INIT odpověď (řetězec)

        // do třídy Senzory:
        private readonly StringBuilder _linkBuffer = new StringBuilder();
        private readonly StringBuilder _initBuffer = new StringBuilder();   // jen INIT odpovědi


        public Senzory(Form1 rodic)
        {
            InitializeComponent();
            InitializeChart();

            comboBoxTIMER.SelectedIndex = 1;

            displayTimer = new Timer();
            displayTimer.Interval = 100;
            displayTimer.Tick += DisplayTimer_Tick;
            displayTimer.Start();

            comboBoxTIMER.SelectedIndexChanged += (s, e) => ApplyTimerIntervalFromUi();
            ApplyTimerIntervalFromUi();

            comPortWatcherTimer = new Timer();
            comPortWatcherTimer.Interval = 500;
            comPortWatcherTimer.Tick += ComPortWatcherTimer_Tick;
            comPortWatcherTimer.Start();

            SetUiForConnection(false);

            LoadSensorsFromJson();

            comboBoxSensor.SelectedIndex = -1;
            comboBoxMode.SelectedIndex = -1;

            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

            comboBoxSensor.SelectedIndexChanged += comboBoxSensor_SelectedIndexChanged;
            comboBoxSensor.SelectedIndexChanged += (s, e) => UpdateRequestFromUi();
            comboBoxMode.SelectedIndexChanged += (s, e) => UpdateRequestFromUi();

            // INIT stav reset při změně módu/senzoru
            comboBoxMode.SelectedIndexChanged += (s, e) => { _initRequestSent = false; _lastInitPayload = null; UpdateInitBtnEnabled(); };
            comboBoxSensor.SelectedIndexChanged += (s, e) => { _initRequestSent = false; _lastInitPayload = null; UpdateInitBtnEnabled(); };

            textPIN1.TextChanged += (s, e) => UpdateRequestFromUi();
            textPIN2.TextChanged += (s, e) => UpdateRequestFromUi();
            textPIN3.TextChanged += (s, e) => UpdateRequestFromUi();

  

        }

        private void InitializeChart()
        {
            chart1.Series.Clear();

            Series series = new Series("measuring")
            {
                ChartType = SeriesChartType.Line,
                XValueType = ChartValueType.Int32,
                YValueType = ChartValueType.Double,
                IsVisibleInLegend = false
            };
            chart1.Series.Add(series);

            chart1.ChartAreas[0].AxisX.Title = "Počet vzorků";
            chart1.ChartAreas[0].AxisY.LineWidth = 2;
            chart1.Series["measuring"].BorderWidth = 2;
            chart1.Series["measuring"].Color = Color.Black;
        }

        private Komponenty FindSelectedComponent()
        {
            var label = comboBoxSensor.Text?.Trim();
            if (string.IsNullOrWhiteSpace(label) || SenzoryData == null) return null;
            return SenzoryData.FirstOrDefault(k =>
                string.Equals(k.Znaceni?.Trim(), label, StringComparison.OrdinalIgnoreCase));
        }

        private void UpdatePinInputsUi()
        {
            // vše skryj
            PIN1.Visible = PIN2.Visible = PIN3.Visible = false;
            textPIN1.Visible = textPIN2.Visible = textPIN3.Visible = false;

            string mode = comboBoxMode.Text?.Trim();

            // CONFIG – ukaž key/labely dle JSONu (Configs/Config1..3)
            if (mode != null && mode.Equals("CONFIG", StringComparison.OrdinalIgnoreCase))
            {
                var item = FindSelectedComponent();
                if (item == null) return;

                var configs = GetConfigNames(item);
                if (configs.Count >= 1)
                {
                    PIN1.Text = CleanConfigLabel(configs[0]);
                    PIN1.Visible = true;
                    textPIN1.Visible = true;
                }
                if (configs.Count >= 2)
                {
                    PIN2.Text = CleanConfigLabel(configs[1]);
                    PIN2.Visible = true;
                    textPIN2.Visible = true;
                }
                if (configs.Count >= 3)
                {
                    PIN3.Text = CleanConfigLabel(configs[2]);
                    PIN3.Visible = true;
                    textPIN3.Visible = true;
                }
                return;
            }

            // CONNECT/DISCONNECT – ukaž piny dle PIN1/PIN2 z JSONu
            bool isConnMode = mode != null && (
                mode.Equals("CONNECT", StringComparison.OrdinalIgnoreCase) ||
                mode.Equals("DISCONNECT", StringComparison.OrdinalIgnoreCase));

            if (!isConnMode) return;

            var it2 = FindSelectedComponent();
            if (it2 == null) return;

            if (!string.IsNullOrWhiteSpace(it2.PIN1))
            {
                PIN1.Text = it2.PIN1;
                PIN1.Visible = true;
                textPIN1.Visible = true;
            }
            if (!string.IsNullOrWhiteSpace(it2.PIN2))
            {
                PIN2.Text = it2.PIN2;
                PIN2.Visible = true;
                textPIN2.Visible = true;

                if (!PIN1.Visible)
                {
                    PIN1.Text = "PIN1";
                    PIN1.Visible = true;
                    textPIN1.Visible = true;
                }
            }
        }

        private static List<string> GetConfigNames(Komponenty item)
        {
            var result = new List<string>();
            if (item == null) return result;

            var propArr = item.GetType().GetProperty("Configs");
            if (propArr != null)
            {
                var val = propArr.GetValue(item) as System.Collections.IEnumerable;
                if (val != null)
                {
                    foreach (var it in val)
                    {
                        var s = (it ?? "").ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(s)) result.Add(s);
                    }
                }
                if (result.Count > 0) return result;
            }

            string[] names = { "Config1", "Config2", "Config3", "CONFIG1", "CONFIG2", "CONFIG3" };
            foreach (var n in names)
            {
                var p = item.GetType().GetProperty(n);
                if (p != null)
                {
                    var s = (p.GetValue(item)?.ToString() ?? "").Trim();
                    if (!string.IsNullOrWhiteSpace(s)) result.Add(s);
                }
            }
            return result;
        }

        private static string CleanConfigLabel(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return raw;
            var s = raw.Trim();
            int colon = s.IndexOf(':');
            if (colon >= 0) s = s.Substring(0, colon);
            s = System.Text.RegularExpressions.Regex.Replace(s, @"\s*\(.*?\)\s*$", "");
            s = System.Text.RegularExpressions.Regex.Replace(s, @"\s+", " ");
            return s + ":";
        }

        private static string ConfigKey(string raw)
        {
            var s = CleanConfigLabel(raw);
            return s?.TrimEnd(':').Trim();
        }

        private static string NormalizePinInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            input = input.Trim();
            var digits = new string(input.Where(char.IsDigit).ToArray());
            return string.IsNullOrEmpty(digits) ? input : digits;
        }

        private string BuildPinExpr()
        {
            var item = FindSelectedComponent();
            if (item == null) return null;

            var p1 = NormalizePinInput(textPIN1.Text);
            var hasSecond = !string.IsNullOrWhiteSpace(item.PIN2);
            var p2 = NormalizePinInput(textPIN2.Text);

            if (hasSecond)
            {
                if (string.IsNullOrWhiteSpace(p1) || string.IsNullOrWhiteSpace(p2))
                    return null;
                return $"{p1},{p2}";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(p1))
                    return null;
                return $"{p1}";
            }
        }

        private void UpdateRequestFromUi()
        {
            bool hasSensor = comboBoxSensor.SelectedIndex >= 0 && !string.IsNullOrWhiteSpace(comboBoxSensor.Text);
            bool hasMode = comboBoxMode.SelectedIndex >= 0 && !string.IsNullOrWhiteSpace(comboBoxMode.Text);
            bool connected = SerialManager.Instance.IsOpen;

            UpdatePinInputsUi();

            string m = comboBoxMode.Text?.Trim() ?? string.Empty;

            request = null;

            string formattedId = null;
            if (hasSensor)
            {
                string sensorLabel = comboBoxSensor.Text.Trim();
                if (!sensorIdMap.TryGetValue(sensorLabel, out string sensorId) || string.IsNullOrWhiteSpace(sensorId))
                    sensorId = sensorLabel;
                formattedId = FormatSensorId(sensorId);
            }

            if (hasMode)
            {
                if (m.Equals("INIT", StringComparison.OrdinalIgnoreCase))
                {
                    request = "?type=INIT&api=1.0";
                }
                else if (m.Equals("CONNECT", StringComparison.OrdinalIgnoreCase) || m.Equals("DISCONNECT", StringComparison.OrdinalIgnoreCase))
                {
                    if (hasSensor)
                    {
                        var item = FindSelectedComponent();
                        string pinExpr = BuildPinExpr(); // "13" nebo "5,18"

                        if (string.IsNullOrWhiteSpace(pinExpr))
                        {
                            bool needTwo = item != null && !string.IsNullOrWhiteSpace(item.PIN2);
                            string keyWhenEmpty = needTwo ? "pins" : "pin";
                            request = $"?type={m}&id={formattedId}&{keyWhenEmpty}=";
                        }
                        else
                        {
                            bool multiple = pinExpr.Contains(",");
                            string key = multiple ? "pins" : "pin";
                            request = $"?type={m}&id={formattedId}&{key}={pinExpr}";
                        }
                    }
                }
                else if (m.Equals("CONFIG", StringComparison.OrdinalIgnoreCase))
                {
                    if (hasSensor)
                    {
                        var itemForCfg = FindSelectedComponent();
                        string cfgQuery = BuildConfigQuery(itemForCfg);
                        request = string.IsNullOrEmpty(cfgQuery)
                            ? $"?type={m}&id={formattedId}"
                            : $"?type={m}&id={formattedId}&{cfgQuery}";
                    }
                }
                else if (m.Equals("RESET", StringComparison.OrdinalIgnoreCase))
                {
                    if (hasSensor)
                        request = $"?type={m}&id={formattedId}";
                }
                else
                {
                    if (hasSensor)
                        request = $"?type={m}&id={formattedId}";
                }
            }

            label8.Text = request ?? string.Empty;

            // povolení Start
            bool ready = connected && hasMode;

            if (m.Equals("CONNECT", StringComparison.OrdinalIgnoreCase) || m.Equals("DISCONNECT", StringComparison.OrdinalIgnoreCase))
            {
                var item = FindSelectedComponent();
                bool needTwo = item != null && !string.IsNullOrWhiteSpace(item.PIN2);
                bool p1ok = !string.IsNullOrWhiteSpace(NormalizePinInput(textPIN1.Text));
                bool p2ok = !needTwo || !string.IsNullOrWhiteSpace(NormalizePinInput(textPIN2.Text));
                ready = ready && hasSensor && p1ok && p2ok;
            }
            else if (m.Equals("CONFIG", StringComparison.OrdinalIgnoreCase))
            {
                var item = FindSelectedComponent();
                var cfgs = GetConfigNames(item);
                bool c1 = cfgs.Count < 1 || !string.IsNullOrWhiteSpace(textPIN1.Text?.Trim());
                bool c2 = cfgs.Count < 2 || !string.IsNullOrWhiteSpace(textPIN2.Text?.Trim());
                bool c3 = cfgs.Count < 3 || !string.IsNullOrWhiteSpace(textPIN3.Text?.Trim());
                ready = ready && hasSensor && c1 && c2 && c3;
            }
            else if (m.Equals("RESET", StringComparison.OrdinalIgnoreCase))
            {
                ready = ready && hasSensor;
            }
            else if (!m.Equals("INIT", StringComparison.OrdinalIgnoreCase))
            {
                ready = ready && hasSensor;
            }

            if (string.IsNullOrWhiteSpace(request))
                ready = false;

            button1.Enabled = ready;

            UpdateInitBtnEnabled();
            UpdateAcceptButton();
        }

        private void ApplyTimerIntervalFromUi()
        {
            string txt = comboBoxTIMER.Text?.Trim();
            int delay;
            if (!int.TryParse(txt, out delay) || delay < 10)
                delay = 100;
            displayTimer.Interval = delay;
        }

        private void SetUiForConnection(bool isConnected)
        {
            comboBoxCOM.Enabled = !isConnected;

            comboBoxSensor.Enabled = isConnected;
            comboBoxMode.Enabled = isConnected;
            comboBoxTIMER.Enabled = isConnected;

            button1.Enabled = false;

            if (ConnectBtn != null)
                ConnectBtn.Text = isConnected ? "Odpojit" : "Připojit";

            badgeConn.Text = isConnected ? "Připojeno" : "Nepřipojeno";
            badgeConn.BackColor = isConnected
                ? Color.FromArgb(46, 125, 50)
                : Color.FromArgb(107, 114, 128);

            UpdateRequestFromUi();
            if (!isConnected) { _initRequestSent = false; _lastInitPayload = null; }
            UpdateInitBtnEnabled();
            UpdateAcceptButton();  

        }

        private void ComPortWatcherTimer_Tick(object sender, EventArgs e)
        {
            var currentPorts = SerialPort.GetPortNames().ToList();

            if (!currentPorts.SequenceEqual(lastKnownPorts))
            {
                string selected = comboBoxCOM.SelectedItem as string;

                comboBoxCOM.Items.Clear();
                comboBoxCOM.Items.AddRange(currentPorts.ToArray());

                if (selected != null && currentPorts.Contains(selected))
                {
                    comboBoxCOM.SelectedItem = selected;
                }
                else if (currentPorts.Count > 0)
                {
                    comboBoxCOM.SelectedIndex = 0;
                }

                lastKnownPorts = currentPorts;
            }
        }

        private void ConnectBtn_Click(object sender, EventArgs e)
        {
            if (SerialManager.Instance.IsOpen)
            {
                try
                {
                    StopSendingRequest();
                    SerialManager.Instance.DetachReceiver();
                    SerialManager.Instance.Close();
                    UiLog("Odpojeno od portu.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Chyba při odpojování: {ex.Message}");
                }
                finally
                {
                    SetUiForConnection(false);
                    UpdateRequestFromUi();
                }
                return;
            }

            string selectedPort = comboBoxCOM.Text?.Trim();
            if (string.IsNullOrWhiteSpace(selectedPort))
            {
                MessageBox.Show("Prosím vyber COM port.");
                return;
            }

            try
            {
                SerialManager.Instance.ConfigurePort(
                    portName: selectedPort,
                    baudRate: 115200,
                    parity: Parity.None,
                    dataBits: 8,
                    stopBits: StopBits.One,
                    handshake: Handshake.None,
                    newLine: "\n"
                );

                SerialManager.Instance.AttachExclusiveReceiver(SerialPort_DataReceived);
                SerialManager.Instance.Open();

                SetUiForConnection(true);
                UiLog($"Připojeno k {selectedPort}.");
                UpdateRequestFromUi();
            }
            catch (Exception ex)
            {
                SetUiForConnection(false);
                MessageBox.Show($"Chyba při otevírání portu: {ex.Message}");
                badgeConn.Text = "Chyba";
                badgeConn.BackColor = Color.FromArgb(211, 47, 47);
                UpdateRequestFromUi();
            }
        }



        private void UiLog(string msg)
        {
            LogLink(msg);
        }


        private void buttonStart_Click(object sender, EventArgs e)
        {
            UpdateRequestFromUi();

            string selectedPort = comboBoxCOM.Text?.Trim();
            string currentID = comboBoxSensor.Text?.Trim();
            string currentType = comboBoxMode.Text?.Trim();

            if (button1.Text == "Spustit")
            {
                var intendedMode = comboBoxMode.Text?.Trim() ?? "";
                bool wantsConn = intendedMode.Equals("CONNECT", StringComparison.OrdinalIgnoreCase)
                              || intendedMode.Equals("DISCONNECT", StringComparison.OrdinalIgnoreCase);

                if (wantsConn && button1.Text == "Zastavit")
                {
                    comboBoxSensor.Enabled = true;
                    comboBoxMode.Enabled = true;
                    comboBoxCOM.Enabled = true;
                    comboBoxTIMER.Enabled = true;
                    ConnectBtn.Enabled = true;
                    button1.Text = "Spustit";
                    UpdateAcceptButton();

                    StopSendingRequest();
                    UiLog("Měření pozastaveno (přepnutí na CONNECT/DISCONNECT).");

                    button1.BackColor = Color.FromArgb(15, 108, 189);
                    button1.FlatAppearance.BorderColor = Color.FromArgb(15, 108, 189);
                    button1.FlatAppearance.MouseDownBackColor = Color.FromArgb(17, 94, 163);
                    button1.FlatAppearance.MouseOverBackColor = Color.FromArgb(12, 83, 146);
                }

                if (string.IsNullOrWhiteSpace(selectedPort))
                {
                    MessageBox.Show("Prosím vyber COM port.");
                    return;
                }
                if (!SerialManager.Instance.IsOpen)
                {
                    MessageBox.Show("Nejprve se připoj k sériovému portu.");
                    return;
                }
                if (string.IsNullOrWhiteSpace(currentType))
                {
                    MessageBox.Show("Prosím vyber typ měření.");
                    return;
                }
                if (!currentType.Equals("INIT", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(currentID))
                    {
                        MessageBox.Show("Prosím zadej nebo vyber ID zařízení.");
                        return;
                    }
                }

                if (currentID != lastUsedID)
                {
                    ResetChart();
                    lastUsedID = currentID;
                }

                bool isConnMode = currentType.Equals("CONNECT", StringComparison.OrdinalIgnoreCase)
                               || currentType.Equals("DISCONNECT", StringComparison.OrdinalIgnoreCase);

                if (isConnMode)
                {
                    try
                    {
                        if (request == null)
                        {
                            UiLog("Požadavek není sestaven.");
                            return;
                        }

                        displayTimer?.Start();
                        SerialManager.Instance.WriteLine(request);
                        // Zde do textBox2 nelogujeme request, necháme jen UI hlášky
                        _lastSentMode = null;
                        UiLog("Měření spuštěno."); // UI hláška
                        return;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Chyba při odesílání: {ex.Message}");
                    }
                    return;
                }

                // ostatní módy (včetně INIT)
                _lastSentMode = null;
                StartSendingRequest();

                // INIT speciál: UI hláška
                if (currentType.Equals("INIT", StringComparison.OrdinalIgnoreCase))
                    UiLog("INIT odesláno.");

                button1.Text = "Zastavit";
                UpdateAcceptButton();
                comboBoxSensor.Enabled = false;
                comboBoxMode.Enabled = false;
                comboBoxCOM.Enabled = false;
                comboBoxTIMER.Enabled = false;
                ConnectBtn.Enabled = false;
                button1.FlatAppearance.MouseDownBackColor = Color.FromArgb(183, 28, 28);
                button1.FlatAppearance.MouseOverBackColor = Color.FromArgb(153, 0, 0);
                button1.BackColor = Color.FromArgb(211, 47, 47);
                button1.FlatAppearance.BorderColor = Color.FromArgb(211, 47, 47);
            }
            else
            {
                comboBoxSensor.Enabled = true;
                comboBoxMode.Enabled = true;
                comboBoxCOM.Enabled = true;
                comboBoxTIMER.Enabled = true;
                ConnectBtn.Enabled = true;
                button1.Text = "Spustit";
                UpdateAcceptButton();
                StopSendingRequest();
                UiLog("Měření pozastaveno.");

                button1.BackColor = Color.FromArgb(15, 108, 189);
                button1.FlatAppearance.BorderColor = Color.FromArgb(15, 108, 189);
                button1.FlatAppearance.MouseDownBackColor = Color.FromArgb(17, 94, 163);
                button1.FlatAppearance.MouseOverBackColor = Color.FromArgb(12, 83, 146);

                UpdateRequestFromUi();
            }
        }

        private void StartSendingRequest()
        {
            if (request == null)
            {
                UiLog("Požadavek není sestaven.");
                return;
            }

            displayTimer?.Start();

            // reset/obnova CTS
            _sendCts?.Cancel();
            _sendCts?.Dispose();
            _sendCts = new System.Threading.CancellationTokenSource();

            // 1) UPDATE = cyklické posílání
            if (request.StartsWith("?type=update", StringComparison.OrdinalIgnoreCase))
            {
                isSendingRequest = true;
                _ = SendLoopAsync(_sendCts.Token);
                return;
            }

            // 2) Ostatní (INIT / CONFIG / RESET / CONNECT / atd.) = jednorázově
            try
            {
                if (!SerialManager.Instance.IsOpen)
                {
                    UiLog("Port není otevřen – požadavek se neodešle.");
                    return;
                }

                SerialManager.Instance.WriteLine(request);

                // INIT: začínáme nový INIT cyklus – smaž starý INIT log a čekej odpověď
                if (request.StartsWith("?type=INIT", StringComparison.OrdinalIgnoreCase))
                {
                    _initRequestSent = true;
                    _awaitingInitResponse = true;
                    _lastInitPayload = null;
                    _initBuffer.Clear();
                    UpdateInitBtnEnabled();
                }
            }
            catch (Exception ex)
            {
                UiLog($"Chyba při zápisu: {ex.Message}");
            }
            finally
            {
                isSendingRequest = false;
            }
        }


        private async Task SendLoopAsync(System.Threading.CancellationToken ct)
        {
            while (!ct.IsCancellationRequested &&
                   SerialManager.Instance.IsOpen &&
                   isSendingRequest)
            {
                int delay = 100;
                var txt = comboBoxTIMER?.Text?.Trim();
                if (!int.TryParse(txt, out delay) || delay < 1)
                    delay = 100;

                try
                {
                    await Task.Delay(delay, ct);
                    if (ct.IsCancellationRequested) break;

                    SerialManager.Instance.WriteLine(request);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    UiLog($"Chyba při zápisu: {ex.Message}");
                    break;
                }
            }
        }

        private void StopSendingRequest()
        {
            isSendingRequest = false;
            displayTimer?.Stop();
            _sendCts?.Cancel();

            lock (_rxLock) _latestDataFrame = null;

            try
            {
                // případně SerialManager.Instance.DiscardInOut();
            }
            catch { }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var port = sender as SerialPort;
                string data = port?.ReadExisting();
                if (string.IsNullOrEmpty(data)) return;

                lock (_rxLock)
                {
                    _rxBuffer.Append(data);
                }
            }
            catch
            {
                // ticho
            }
        }

        private void ParseAndDisplayData(string data)
        {
            var numericPairs = new List<string>();

            data = data.Trim();
            data = data.TrimStart('\uFEFF');
            if (data.StartsWith("?")) data = data.Substring(1);

            var parameters = data.Split('&')
                                 .Select(part => part.Split('='))
                                 .Where(pair => pair.Length == 2)
                                 .ToDictionary(pair => pair[0], pair => pair[1]);

            var skipKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
             { "type", "id", "pin", "app", "version", "dbversion", "api", "status", "code" };

            var dataForGraph = parameters
                .Where(kvp => !skipKeys.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

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
                    // přidej do přehledu pro valueText
                    numericPairs.Add($"{variableName}={numericValue.ToString("G", System.Globalization.CultureInfo.InvariantCulture)}");

                    LogLink($"[GRAPH] {variableName} -> {numericValue}");

                    this.Invoke(new Action(() =>
                    {
                        if (chart1.Series.IsUniqueName(variableName))
                        {
                            var s = new Series(variableName)
                            {
                                ChartType = SeriesChartType.Line,
                                BorderWidth = 2,
                                Color = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256))
                            };
                            chart1.Series.Add(s);
                        }

                        var series = chart1.Series[variableName];
                        if (series.Points.Count > 50) series.Points.RemoveAt(0);

                        series.Points.AddXY(sampleCount, numericValue);

                        if (chart1.ChartAreas.Count == 0)
                            chart1.ChartAreas.Add(new ChartArea());

                        chart1.ChartAreas[0].AxisX.Minimum = Math.Max(0, sampleCount - 10);
                        chart1.ChartAreas[0].AxisX.Maximum = sampleCount;
                        chart1.ChartAreas[0].RecalculateAxesScale();
                        chart1.ChartAreas[0].AxisY.Title = dataForGraph.Count > 1 ? "Values" : variableName.ToUpper();
                    }));
                }
                else // jen když to NENÍ číslo
                {
                    // textové hodnoty -> do logu, bez kreslení grafu
                    LogLink($"{variableName}: {raw}");
                }
            }

            // až po zpracování všech klíčů z rámce aktualizuj valueText jedním řádkem
            if (numericPairs.Count > 0)
            {
                var text = string.Join(", ", numericPairs); // např. "temp=23.5, hum=41.2"
                if (valueText.InvokeRequired)
                    BeginInvoke((Action)(() => valueText.Text = text));
                else
                    valueText.Text = text;
            }

            sampleCount++;
            chart1.Invalidate();
        }


        private static string FormatSensorId(string rawId)
        {
            if (string.IsNullOrWhiteSpace(rawId)) return rawId;

            string t = rawId.Trim();

            if (t.StartsWith("S", StringComparison.OrdinalIgnoreCase))
                return "S" + t.Substring(1);

            if (int.TryParse(t, out int n) && n >= 0)
                return "S" + n.ToString("D2");

            var digits = new string(t.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out n) && n >= 0)
                return "S" + n.ToString("D2");

            return "S" + t;
        }

        private void ParseInitMessage(string data)
        {
            string rawData = data.Trim();

            if (rawData.StartsWith("?"))
            {
                rawData = rawData.Substring(1);
            }

            string[] sensorEntries = rawData.Split(',');
            var result = new StringBuilder();

            foreach (string entry in sensorEntries)
            {
                string[] parts = entry.Split(':');
                if (parts.Length == 2)
                {
                    string id = parts[0];
                    string type = parts[1];
                    result.AppendLine($"{type} ({id})");
                }
            }
            // výstup pro INIT teď posíláme do popup okna (_initForm) v DisplayTimer_Tick
        }

        private void ResetChart()
        {
            sampleCount = 0;
            chart1.Series.Clear();
            InitializeChart();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SerialManager.Instance.Close();
        }

        private void LoadSensorsFromJson()
        {
            try
            {
                string jsonPath = Path.Combine(Application.StartupPath, "Senzory.json");
                if (!File.Exists(jsonPath))
                {
                    MessageBox.Show("Soubor Senzory.json nebyl nalezen v " + Application.StartupPath + ".");
                    return;
                }

                string jsonText = File.ReadAllText(jsonPath);

                var data = JsonSerializer.Deserialize<List<Komponenty>>(
                    jsonText,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (data == null || data.Count == 0)
                {
                    MessageBox.Show("Senzory.json je prázdný nebo ve špatném formátu.");
                    return;
                }

                SenzoryData = data;

                sensorIdMap.Clear();
                comboBoxSensor.BeginUpdate();
                comboBoxSensor.Items.Clear();

                foreach (var k in SenzoryData)
                {
                    string label = (k.Znaceni ?? string.Empty).Trim();
                    if (string.IsNullOrWhiteSpace(label)) continue;

                    if (!sensorIdMap.ContainsKey(label))
                        comboBoxSensor.Items.Add(label);

                    sensorIdMap[label] = k.Id.ToString();
                }

                comboBoxSensor.EndUpdate();
                comboBoxSensor.SelectedIndex = -1;
                UpdateRequestFromUi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při načítání Senzory.json: " + ex.Message);
            }
        }

        private void DisplayTimer_Tick(object sender, EventArgs e)
        {
            string chunk;
            lock (_rxLock)
            {
                if (_rxBuffer.Length == 0) return;
                chunk = _rxBuffer.ToString();
                _rxBuffer.Clear();
            }

            chunk = chunk.Replace("\r", "");
            var lines = chunk.Split('\n');

            foreach (var raw in lines)
            {
                if (string.IsNullOrEmpty(raw)) continue;

                var line = raw.Trim();
                line = line.TrimStart('\uFEFF');
                line = new string(line.Where(ch => !char.IsControl(ch)
                                                   || ch == '?' || ch == '=' || ch == '&'
                                                   || ch == '.' || ch == ',' || ch == '-'
                                                   || char.IsLetterOrDigit(ch)).ToArray());
                if (string.IsNullOrEmpty(line)) continue;

                // 1) INIT seznam (id:type,id:type,...) — loguj do INIT a povol tlačítko
                if (LooksLikeInitList(line))
                {
                    ParseInitMessage(line);

                    _lastInitPayload = line;
                    _awaitingInitResponse = false;
                    UpdateInitBtnEnabled();

                    LogInit(line);
                    continue;
                }

                // 2) měřicí rámce "?id=..." – graf + běžný log
                if (line.StartsWith("?id=", StringComparison.OrdinalIgnoreCase))
                {
                    ParseAndDisplayData(line);
                    LogLink(line);
                    continue;
                }

                // 3) ostatní text → běžný log
                LogLink(line);
            }

            chart1.Invalidate();
        }


        private static bool LooksLikeInitList(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            if (s.StartsWith("?")) return false;
            return s.Contains(":") && s.Contains(",");
        }

        private void comboBoxSensor_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string label = comboBoxSensor.SelectedItem as string;
                if (string.IsNullOrWhiteSpace(label)) return;

                string baseDir = Directory.GetParent(Application.StartupPath).Parent.Parent.FullName;
                string sensorsDir = Path.Combine(baseDir, "Senzory");

                string[] exts = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };

                string foundPath = null;
                foreach (var ext in exts)
                {
                    string p = Path.Combine(sensorsDir, label + ext);
                    if (File.Exists(p))
                    {
                        foundPath = p;
                        break;
                    }
                }

                if (foundPath == null)
                {
                    pictureBox1.Image = null;
                    UiLog($"Nenalezen obrázek pro „{label}“ ve složce {sensorsDir}.");
                    return;
                }

                using (var fs = new FileStream(foundPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var img = Image.FromStream(fs);
                    pictureBox1.Image = (Image)img.Clone();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při načítání obrázku: {ex.Message}");
            }
        }

        private string BuildConfigQuery(Komponenty item)
        {
            if (item == null) return string.Empty;

            var cfgs = GetConfigNames(item);
            var values = new[] { textPIN1.Text?.Trim(), textPIN2.Text?.Trim(), textPIN3.Text?.Trim() };

            var parts = new List<string>();
            for (int i = 0; i < Math.Min(3, cfgs.Count); i++)
            {
                string key = ConfigKey(cfgs[i]);
                string val = values[i];

                if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(val))
                {
                    parts.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(val)}");
                }
            }

            return string.Join("&", parts);
        }

        private void link_btn_Click(object sender, EventArgs e)
        {
            if (_linkForm == null || _linkForm.IsDisposed)
            {
                _linkForm = new SerialPopupForm("Sériový výpis");
                _linkForm.FormClosed += (_, __) => _linkForm = null;

                // vždy nasyp aktuální buffer:
                if (_linkBuffer.Length > 0)
                    _linkForm.SetText(_linkBuffer.ToString());

                PositionNextToHost(_linkForm);
                _linkForm.Show();
                _linkForm.BringToFront();
                return;
            }

            if (_linkForm.Visible)
                _linkForm.Hide();
            else
            {
                // dorovnat stav (kdyby se v mezidobí buffer zvětšil):
                _linkForm.SetText(_linkBuffer.ToString());
                PositionNextToHost(_linkForm);
                _linkForm.Show();
                _linkForm.BringToFront();
            }
        }





        private void init_btn_Click(object sender, EventArgs e)
        {
            if (_initForm == null || _initForm.IsDisposed)
            {
                _initForm = new SerialPopupForm("INIT výpis");
                _initForm.FormClosed += (_, __) => _initForm = null;

                // vždy nasyp aktuální INIT buffer:
                if (_initBuffer.Length > 0)
                    _initForm.SetText(_initBuffer.ToString());

                PositionNextToHost(_initForm);
                _initForm.Show();
                _initForm.BringToFront();
                return;
            }

            if (_initForm.Visible)
                _initForm.Hide();
            else
            {
                // dorovnat stav:
                _initForm.SetText(_initBuffer.ToString());
                PositionNextToHost(_initForm);
                _initForm.Show();
                _initForm.BringToFront();
            }
        }


        private void UpdateAcceptButton()
        {
            var form = this.FindForm();
            if (form == null) return;

            // Enter bude spouštět jen když je tlačítko připravené a je ve stavu "Spustit"
            if (button1.Enabled)
                form.AcceptButton = button1;
            else
                form.AcceptButton = null;
        }

        private void UpdateInitBtnEnabled()
        {
            string m = comboBoxMode.Text?.Trim() ?? string.Empty;
            bool connected = SerialManager.Instance.IsOpen;
            bool isInit = m.Equals("INIT", StringComparison.OrdinalIgnoreCase);

            // povol až když: mód = INIT, připojeno a už opravdu nějaká INIT odpověď dorazila
            init_btn.Enabled = isInit && connected && !string.IsNullOrWhiteSpace(_lastInitPayload);
        }

        private void PositionNextToHost(Form form, int offsetX = 10)
        {
            var host = this.FindForm();
            if (host != null)
            {
                form.Left = host.Right + offsetX;
                form.Top = host.Top;
            }
        }

        // Bezpečně přidá řádek do LINK bufferu a do otevřeného LINK popupu
        private void LogLink(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return;
            if (!line.EndsWith("\r\n")) line += "\r\n";

            void Write()
            {
                _linkBuffer.Append(line); // vždy do bufferu
                if (_linkForm != null && !_linkForm.IsDisposed) // a když je okno otevřené, tak i do něj
                    _linkForm.AppendLine(line);
            }

            if (InvokeRequired) BeginInvoke((Action)Write);
            else Write();
        }

        // Bezpečně přidá řádek do INIT bufferu a do otevřeného INIT popupu
        private void LogInit(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return;
            if (!line.EndsWith("\r\n")) line += "\r\n";

            void Write()
            {
                _initBuffer.Append(line); // vždy do bufferu
                if (_initForm != null && !_initForm.IsDisposed) // a když je okno otevřené, tak i do něj
                    _initForm.AppendLine(line);
            }

            if (InvokeRequired) BeginInvoke((Action)Write);
            else Write();
        }


        private void Senzory_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
