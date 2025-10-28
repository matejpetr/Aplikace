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
        private string lastUsedID = null;

        private Timer comPortWatcherTimer;
        private List<string> lastKnownPorts = new List<string>();
        private readonly Dictionary<string, string> sensorIdMap
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // serial controller (encapsulates SerialManager + SerialParser)
        private SerialController _serialController;

        private System.Threading.CancellationTokenSource _sendCts;

        private List<Komponenty> SenzoryData;
        private string _lastSentMode = null;

        // Popup okna
        private SerialPopupForm _linkForm;   // vše mimo INIT
        private SerialPopupForm _initForm;   // jen INIT odpověď
        private PinsSelect _pinsForm;         // výběr pinů

        // INIT stav
        private bool _awaitingInitResponse = false; // čekám na odpověď INIT?
        private bool _initRequestSent = false;      // byl odeslán INIT request?
        private string _lastInitPayload = null;     // poslední přijatá INIT odpověď (řetězec)

        // do třídy Senzory:
        private readonly StringBuilder _linkBuffer = new StringBuilder();
        private readonly StringBuilder _initBuffer = new StringBuilder();   // jen INIT odpovědi

        // UI managers
        private ValueDisplayManager _valueDisplayManager;
        private ChartManager _chartManager;
        private ImageManager _imageManager; // NEW: replace old image-loading method

        public Senzory(Form1 rodic)
        {
            InitializeComponent();
            InitializeChart();

            comboBoxTIMER.SelectedIndex = 1;

            comboBoxTIMER.SelectedIndexChanged += (s, e) => ApplyTimerIntervalFromUi();
            ApplyTimerIntervalFromUi();

            comPortWatcherTimer = new Timer();
            comPortWatcherTimer.Interval = 500;
            comPortWatcherTimer.Tick += ComPortWatcherTimer_Tick;
            comPortWatcherTimer.Start();

            // serial controller must exist before UI queries IsOpen
            _serialController = new SerialController();
            _serialController.InitReceived += Parser_InitReceived;
            _serialController.DataFrameReceived += Parser_DataFrameReceived;
            _serialController.RawLineReceived += Parser_RawLineReceived;

            SetUiForConnection(false);

            LoadSensorsFromJson();

            comboBoxSensor.SelectedIndex = -1;
            comboBoxMode.SelectedIndex = -1;

            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

            // initialize ImageManager to handle image updates
            _imageManager = new ImageManager(pictureBox1);

            comboBoxSensor.SelectedIndexChanged += comboBoxSensor_SelectedIndexChanged;
            comboBoxSensor.SelectedIndexChanged += (s, e) => UpdateRequestFromUi();
            comboBoxMode.SelectedIndexChanged += (s, e) => UpdateRequestFromUi();


            // INIT stav reset při změně módu/senzoru
            comboBoxSensor.SelectedIndexChanged += (s, e) => { _initRequestSent = false; _lastInitPayload = null; UpdateInitBtnEnabled(); UpdatePinInputsUi(); };
            comboBoxMode.SelectedIndexChanged += (s, e) => { _initRequestSent = false; _lastInitPayload = null; UpdateInitBtnEnabled(); UpdatePinInputsUi(); };


            textPIN1.TextChanged += (s, e) => UpdateRequestFromUi();
            textPIN2.TextChanged += (s, e) => UpdateRequestFromUi();
            textPIN3.TextChanged += (s, e) => UpdateRequestFromUi();

            // value display manager (thread-safe updates)
            _valueDisplayManager = new ValueDisplayManager(valueText);

            // chart manager (will process frames and update chart on UI thread)
            _chartManager = new ChartManager(chart1, _valueDisplayManager, LogLink, intervalMs: 100, maxPoints: 50);
            ApplyTimerIntervalFromUi();

            // Note: SerialController already wires to SerialManager internally
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

            if (chart1.ChartAreas.Count == 0)
                chart1.ChartAreas.Add(new ChartArea());

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
            // Zapamatuj si, kde je focus a pozici kurzoru (pokud jsme v jednom z PIN textboxů)
            Control focused = this.FindForm()?.ActiveControl;
            TextBox focusedTb = null;
            int caret = 0;
            if (focused == textPIN1 || focused == textPIN2 || focused == textPIN3)
            {
                focusedTb = (TextBox)focused;
                caret = focusedTb.SelectionStart;
            }

            // Pomocná lokální funkce: nastav viditelnost jen když se mění
            void SetVis(Control c, bool vis)
            {
                if (c.Visible != vis) c.Visible = vis;
            }

            // NIC neschovávej hromadně. Nejdřív spočítej, co má být vidět:
            bool show1 = false, show2 = false, show3 = false;
            string mode = comboBoxMode.Text?.Trim();

            if (!string.IsNullOrWhiteSpace(mode) &&
                mode.Equals("CONFIG", StringComparison.OrdinalIgnoreCase))
            {
                var item = FindSelectedComponent();
                var configs = RequestBuilder.GetConfigNames(item);
                // note: UI label formatting kept local
                show1 = configs.Count >= 1;
                show2 = configs.Count >= 2;
                show3 = configs.Count >= 3;

                if (show1) PIN1.Text = (configs.Count >= 1) ? configs[0].Split(':')[0] + ":" : PIN1.Text;
                if (show2) PIN2.Text = (configs.Count >= 2) ? configs[1].Split(':')[0] + ":" : PIN2.Text;
                if (show3) PIN3.Text = (configs.Count >= 3) ? configs[2].Split(':')[0] + ":" : PIN3.Text;
            }
            else if (!string.IsNullOrWhiteSpace(mode) &&
                    (mode.Equals("CONNECT", StringComparison.OrdinalIgnoreCase) ||
                     mode.Equals("DISCONNECT", StringComparison.OrdinalIgnoreCase)))
            {
                var it2 = FindSelectedComponent();
                if (it2 != null)
                {
                    if (!string.IsNullOrWhiteSpace(it2.PIN1))
                    {
                        PIN1.Text = it2.PIN1;
                        show1 = true;
                    }
                    if (!string.IsNullOrWhiteSpace(it2.PIN2))
                    {
                        PIN2.Text = it2.PIN2;
                        show2 = true;

                        if (!show1)
                        {
                            PIN1.Text = "PIN1";
                            show1 = true;
                        }
                    }
                }
            }
            else
            {
                // jiné módy – PINy neukazuj
                show1 = show2 = show3 = false;
            }

            // A teď teprve „šetrně“ aplikuj viditelnost (jen když je změna)
            SetVis(PIN1, show1);
            SetVis(textPIN1, show1);
            SetVis(PIN2, show2);
            SetVis(textPIN2, show2);
            SetVis(PIN3, show3);
            SetVis(textPIN3, show3);

            // Vrať focus a caret, pokud to dává smysl
            if (focusedTb != null && focusedTb.Visible)
            {
                focusedTb.Focus();
                focusedTb.SelectionStart = Math.Min(caret, focusedTb.TextLength);
            }
        }

        private void UpdateRequestFromUi()
        {
            bool hasSensor = comboBoxSensor.SelectedIndex >= 0 && !string.IsNullOrWhiteSpace(comboBoxSensor.Text);
            bool hasMode = comboBoxMode.SelectedIndex >= 0 && !string.IsNullOrWhiteSpace(comboBoxMode.Text);
            bool connected = _serialController?.IsOpen == true;

            string m = comboBoxMode.Text?.Trim() ?? string.Empty;

            // delegate building request to RequestBuilder
            request = RequestBuilder.BuildRequest(m, comboBoxSensor.Text?.Trim(), sensorIdMap, FindSelectedComponent(), textPIN1.Text, textPIN2.Text, textPIN3.Text);

            label8.Text = request ?? string.Empty;

            // povolení Start
            bool ready = connected && hasMode;

            if (m.Equals("CONNECT", StringComparison.OrdinalIgnoreCase) || m.Equals("DISCONNECT", StringComparison.OrdinalIgnoreCase))
            {
                var item = FindSelectedComponent();
                bool needTwo = item != null && !string.IsNullOrWhiteSpace(item.PIN2);
                bool p1ok = !string.IsNullOrWhiteSpace(RequestBuilder.NormalizePinInput(textPIN1.Text));
                bool p2ok = !needTwo || !string.IsNullOrWhiteSpace(RequestBuilder.NormalizePinInput(textPIN2.Text));
                ready = ready && hasSensor && p1ok && p2ok;
            }
            else if (m.Equals("CONFIG", StringComparison.OrdinalIgnoreCase))
            {
                var item = FindSelectedComponent();
                var cfgs = RequestBuilder.GetConfigNames(item);
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
            _chartManager?.SetInterval(delay);
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
            if (_serialController.IsOpen)
            {
                try
                {
                    StopSendingRequest();
                    _serialController.Close();
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
                _serialController.ConfigurePort(
                    portName: selectedPort,
                    baudRate: 115200,
                    parity: Parity.None,
                    dataBits: 8,
                    stopBits: StopBits.One,
                    handshake: Handshake.None,
                    newLine: "\n"
                );

                // open and let controller attach
                _serialController.Open();

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
                if (!_serialController.IsOpen)
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

                        _chartManager?.Start();
                        _serialController.WriteLine(request);
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

            _chartManager?.Start();

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
                if (!_serialController.IsOpen)
                {
                    UiLog("Port není otevřen – požadavek se neodešle.");
                    return;
                }

                _serialController.WriteLine(request);

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
                   _serialController.IsOpen &&
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

                    _serialController.WriteLine(request);
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
            _chartManager?.Stop();
            _sendCts?.Cancel();

            try
            {
                // případně SerialManager.Instance.DiscardInOut();
            }
            catch { }
        }

        // Parser event handlers -> UI actions
        private void Parser_InitReceived(object sender, InitEventArgs e)
        {
            _lastInitPayload = e.Payload;
            _awaitingInitResponse = false;
            UpdateInitBtnEnabled();
            LogInit(e.Payload);
        }

        private void Parser_DataFrameReceived(object sender, DataFrameEventArgs e)
        {
            _chartManager.ParseAndEnqueue(e.Line);
            LogLink(e.Line);
        }

        private void Parser_RawLineReceived(object sender, RawLineEventArgs e)
        {
            LogLink(e.Line);
        }

        //----------------------------------------------------------------------

        private void ResetChart()
        {
            chart1.Series.Clear();
            InitializeChart();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { _serialController?.Close(); } catch { }
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

        private void comboBoxSensor_SelectedIndexChanged(object sender, EventArgs e)
        {
            string baseDir = Directory.GetParent(Application.StartupPath).Parent.Parent.FullName;
            try
            {
                string label = comboBoxSensor.SelectedItem as string;
                if (string.IsNullOrWhiteSpace(label)) return;

                // Use ImageManager to update pictureBox
                _imageManager.UpdateImageForLabel(label, "Senzory", baseDir);

                // If no image loaded, log like previous behavior
                if (pictureBox1.Image == null)
                {
                    
                    string sensorsDir = Path.Combine(baseDir, "Senzory");
                    UiLog($"Nenalezen obrázek pro „{label}“ ve složce {sensorsDir}.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při načítání obrázku: {ex.Message}");
            }
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
            bool connected = _serialController?.IsOpen == true;
            init_btn.Enabled = connected && !string.IsNullOrWhiteSpace(_lastInitPayload);
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

    }
}
