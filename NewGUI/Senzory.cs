using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;

namespace NewGUI
{
    public partial class Senzory : UserControl
    {
        private bool isSendingRequest = false;              // Flag pro řízení posílání požadavků
        public string request;                              // Text aktuálního požadavku
        private int sampleCount = 0;                        // Počet načtených vzorků
        private string lastUsedID = null;                   // ID posledně použitého zařízení
        private Random rnd = new Random();                  // Generátor náhodných barev
        private Timer comPortWatcherTimer;
        private List<string> lastKnownPorts = new List<string>();
        private readonly Dictionary<string, string> sensorIdMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        // === Throttling příjmu/zobrazování ===
        private readonly object _rxLock = new object();
        private string _latestDataFrame;                // poslední přijatá věta; zobrazuje se periodicky
        private Timer displayTimer;                     // perioda vykreslování (řízena comboBoxTIMER)
        private System.Threading.CancellationTokenSource _sendCts; // rušení odesílací smyčky
        public Senzory(Form1 rodic)
        {
            InitializeComponent();
            InitializeChart(); // Inicializace grafu

            // Výchozí hodnoty pro comboBoxy
            comboBoxTIMER.SelectedIndex = 1;
            // comboBoxMode.SelectedIndex = 0; // ❌ nechceme nic předvyplněné
            // Timer pro periodické zobrazování přijatých dat (throttle)
            displayTimer = new Timer();
            displayTimer.Interval = 100;                // default; přepíše se dle comboBoxTIMER
            displayTimer.Tick += DisplayTimer_Tick;
            displayTimer.Start();

            // Když se změní perioda v UI, přenastav i zobrazovací timer
            comboBoxTIMER.SelectedIndexChanged += (s, e) => ApplyTimerIntervalFromUi();
            ApplyTimerIntervalFromUi();

            comPortWatcherTimer = new Timer();
            comPortWatcherTimer.Interval = 500; // kontrola každých 500 ms
            comPortWatcherTimer.Tick += ComPortWatcherTimer_Tick;
            comPortWatcherTimer.Start();

            SetUiForConnection(false);

            // jednorázově načti značení ze souboru
            LoadSensorsFromCsv();

            // nepředvybírat
            comboBoxSensor.SelectedIndex = -1;
            comboBoxMode.SelectedIndex = -1;

            // pro jistotu vhodné měřítko obrázku
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

            // handler na změnu výběru (vykreslí obrázek)
            comboBoxSensor.SelectedIndexChanged += comboBoxSensor_SelectedIndexChanged;

            // změna volby → přepočet requestu + (de)aktivace Start
            comboBoxSensor.SelectedIndexChanged += (s, e) => UpdateRequestFromUi();
            comboBoxMode.SelectedIndexChanged += (s, e) => UpdateRequestFromUi();

            // Když se změní perioda v UI, přenastav i zobrazovací timer
            comboBoxTIMER.SelectedIndexChanged += (s, e) => ApplyTimerIntervalFromUi();
            ApplyTimerIntervalFromUi();

        }

        // Nastaví výchozí podobu grafu.
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

        // Skládání requestu z UI + (de)aktivace Start tlačítka
        private void UpdateRequestFromUi()
        {
            bool hasSensor = comboBoxSensor.SelectedIndex >= 0 && !string.IsNullOrWhiteSpace(comboBoxSensor.Text);
            bool hasMode = comboBoxMode.SelectedIndex >= 0 && !string.IsNullOrWhiteSpace(comboBoxMode.Text);
            bool connected = SerialManager.Instance.IsOpen;

            if (hasSensor && hasMode)
            {
                string sensorLabel = comboBoxSensor.Text.Trim();
                string mode = comboBoxMode.Text.Trim(); // UPDATE/CONFIG/INIT/RESET...

                // vezmi id ze sloupce „id“ pro dané „Značení“
                if (!sensorIdMap.TryGetValue(sensorLabel, out string sensorId) || string.IsNullOrWhiteSpace(sensorId))
                    sensorId = sensorLabel;

                // >>> NOVĚ: převod na Sxx
                string formattedId = FormatSensorId(sensorId);

                request = mode.Equals("INIT", StringComparison.OrdinalIgnoreCase)
                          ? $"?type={mode}"
                          : $"?type={mode}&id={formattedId}";


                label8.Text = request; // náhled
            }
            else
            {
                request = null;
                label8.Text = string.Empty;
            }

            // Start je povolen jen když je připojeno a máme validní volby
            button1.Enabled = connected && hasSensor && hasMode;
        }

        private void ApplyTimerIntervalFromUi()
        {
            string txt = comboBoxTIMER.Text?.Trim();
            int delay;
            if (!int.TryParse(txt, out delay) || delay < 10) delay = 100; // min. 10 ms jako pojistka

            // 1) perioda zobrazování
            displayTimer.Interval = delay;
            // 2) perioda zápisu se čte v SendLoopAsync/SendRequest logice – bude stejná
        }


        private void SetUiForConnection(bool isConnected)
        {
            comboBoxCOM.Enabled = !isConnected;

            comboBoxSensor.Enabled = isConnected; // Senzor ID
            comboBoxMode.Enabled = isConnected;   // Příkaz
            comboBoxTIMER.Enabled = isConnected;  // Perioda

            button1.Enabled = false;

            if (ConnectBtn != null)
                ConnectBtn.Text = isConnected ? "Odpojit" : "Připojit";

            badgeConn.Text = isConnected ? "Připojeno" : "Nepřipojeno";
            badgeConn.BackColor = isConnected ? Color.FromArgb(46, 125, 50) : Color.FromArgb(107, 114, 128);

            UpdateRequestFromUi();
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
            // Pokud už je otevřeno, bereme to jako "Odpojit"
            if (SerialManager.Instance.IsOpen)
            {
                try
                {
                    StopSendingRequest(); // pro jistotu ukonči cyklus
                    SerialManager.Instance.DetachReceiver();
                    SerialManager.Instance.Close();
                    AppendTextBox("Odpojeno od portu.\r\n");
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

            // Jinak se pokusíme připojit
            string selectedPort = comboBoxCOM.Text?.Trim();
            if (string.IsNullOrWhiteSpace(selectedPort))
            {
                MessageBox.Show("Prosím vyber COM port.");
                return;
            }

            try
            {
                // Nastav + otevři
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
                AppendTextBox($"Připojeno k {selectedPort}.\r\n");
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

        private async void buttonStart_Click(object sender, EventArgs e)
        {
            UpdateRequestFromUi();

            string selectedPort = comboBoxCOM.Text?.Trim();
            string currentID = comboBoxSensor.Text?.Trim();
            string currentType = comboBoxMode.Text?.Trim();

            if (button1.Text == "Spustit")
            {
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

                // Reset grafu při změně ID
                if (currentID != lastUsedID)
                {
                    ResetChart();
                    lastUsedID = currentID;
                }

                // Spuštění komunikace
                StartSendingRequest();

                // Zablokování vstupů
                button1.Text = "Zastavit";
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

                StopSendingRequest();                  // ⟵ musí být hned tady
                textBox2.AppendText("Měření pozastaveno.\r\n");

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
                AppendTextBox("Požadavek není sestaven.\r\n");
                return;
            }

            // povol zobrazování (throttle timer) – pro jistotu
            displayTimer?.Start();

            // Nová session CTS – okamžité zrušení při Stop
            _sendCts?.Cancel();
            _sendCts?.Dispose();
            _sendCts = new System.Threading.CancellationTokenSource();

            // UPDATE = cyklické posílání, ostatní = jednorázově
            if (request.StartsWith("?type=update", StringComparison.OrdinalIgnoreCase))
            {
                isSendingRequest = true;
                _ = SendLoopAsync(_sendCts.Token); // fire-and-forget smyčka
            }
            else
            {
                try
                {
                    if (SerialManager.Instance.IsOpen)
                        SerialManager.Instance.WriteLine(request);
                    else
                        AppendTextBox("Port není otevřen – požadavek se neodešle.\r\n");
                }
                catch (Exception ex)
                {
                    AppendTextBox($"Chyba při zápisu: {ex.Message}\r\n");
                }
                isSendingRequest = false;
            }
        }

        private async Task SendLoopAsync(System.Threading.CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && SerialManager.Instance.IsOpen && isSendingRequest)
            {
                // Perioda z comboBoxTIMER
                int delay = 100;
                var txt = comboBoxTIMER?.Text?.Trim();
                if (!int.TryParse(txt, out delay) || delay < 1) delay = 100;

                try
                {
                    await Task.Delay(delay, ct);      // zrušitelné čekání
                    if (ct.IsCancellationRequested) break;

                    SerialManager.Instance.WriteLine(request);
                }
                catch (OperationCanceledException)
                {
                    break; // stop okamžitě
                }
                catch (Exception ex)
                {
                    AppendTextBox($"Chyba při zápisu: {ex.Message}\r\n");
                    break;
                }
            }
        }


        private void StopSendingRequest()
        {
            isSendingRequest = false;

            // 1) Zastav zobrazování hned
            displayTimer?.Stop();

            // 2) Zruš čekající delay/odeslání
            _sendCts?.Cancel();

            // 3) Zahodit poslední rámec (nic dalšího se nevykreslí)
            lock (_rxLock) _latestDataFrame = null;

            // 4) (volitelně) vyprázdni HW buffery
            try
            {
                // pokud si přidáš metodu do SerialManageru (viz krok 7), můžeš:
                // SerialManager.Instance.DiscardInOut();
            }
            catch { }
        }


        // Událost spuštěná při příjmu dat ze sériového portu.
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var port = sender as SerialPort;
                string data = port?.ReadExisting();
                if (string.IsNullOrEmpty(data)) return;

                // Ulož poslední rámec thread-safe; zobrazí se při ticku displayTimeru
                lock (_rxLock)
                {
                    _latestDataFrame = data;
                }
                // NEvolej ParseAndDisplayData/SendRequest hned – throttlujeme v DisplayTimer_Tick
            }
            catch
            {
                // ignoruj šum
            }

        }


        // Zpracuje přijatá data a zobrazí je v grafu a textovém výstupu.
        private void ParseAndDisplayData(string data)
        {
            data = data.Trim();
            if (data.StartsWith("?"))
                data = data.Substring(1);

            var parameters = data.Split('&')
                                 .Select(part => part.Split('='))
                                 .Where(pair => pair.Length == 2)
                                 .ToDictionary(pair => pair[0], pair => pair[1]);

            var dataForGraph = parameters
                                 .Where(kvp => kvp.Key != "type" && kvp.Key != "id")
                                 .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            List<string> valueTexts = new List<string>();

            foreach (var kvp in dataForGraph)
            {
                string variableName = kvp.Key;
                string stringValue = kvp.Value;

                if (double.TryParse(stringValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double numericValue))
                {
                    this.Invoke(new Action(() =>
                    {
                        if (!chart1.Series.IsUniqueName(variableName))
                        {
                            // Série již existuje
                        }
                        else
                        {
                            Series newSeries = new Series(variableName)
                            {
                                ChartType = SeriesChartType.Line,
                                BorderWidth = 2,
                                Color = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256))
                            };
                            chart1.Series.Add(newSeries);
                        }

                        if (chart1.Series[variableName].Points.Count > 50)
                        {
                            chart1.Series[variableName].Points.RemoveAt(0);
                        }

                        chart1.Series[variableName].Points.AddXY(sampleCount, numericValue);

                        chart1.ChartAreas[0].AxisX.Minimum = Math.Max(0, sampleCount - 10);
                        chart1.ChartAreas[0].AxisX.Maximum = sampleCount;
                        chart1.ChartAreas[0].RecalculateAxesScale();

                        chart1.ChartAreas[0].AxisY.Title = dataForGraph.Count > 1 ? "Values" : variableName.ToUpper();
                    }));

                    valueTexts.Add($"{variableName} = {numericValue}");
                }
                else
                {
                    valueTexts.Add($"{variableName}: {stringValue}");
                }
            }

            if (valueTexts.Count > 0)
            {
                AppendTextBox(string.Join(", ", valueTexts) + "\r\n");
                sampleCount++;
            }
        }
        private static string FormatSensorId(string rawId)
        {
            if (string.IsNullOrWhiteSpace(rawId)) return rawId;

            string t = rawId.Trim();

            // Když už je ve tvaru Sxx / sxx, ponech (normalizuj na velké S)
            if (t.StartsWith("S", StringComparison.OrdinalIgnoreCase))
                return "S" + t.Substring(1);

            // Zkus čistě číslo
            if (int.TryParse(t, out int n) && n >= 0)
                return "S" + n.ToString("D2");

            // Jinak vytáhni číslice z textu (např. "ID-3" -> "S03")
            var digits = new string(t.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out n) && n >= 0)
                return "S" + n.ToString("D2");

            // Fallback: prostě předřaď S
            return "S" + t;
        }



        private void ParseInitMessage(string data)
        {
            string rawData = data.Trim();

            if (rawData.StartsWith("?"))
            {
                rawData = rawData.Substring(1); // odstraní '?'
            }

            string[] sensorEntries = rawData.Split(',');

            StringBuilder result = new StringBuilder();

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

            AktivBox.Text = result.ToString();
        }

        /// Vlákna-bezpečný zápis do textového pole.
        private void AppendTextBox(string text)
        {
            if (textBox2.InvokeRequired)
            {
                textBox2.Invoke(new Action(() =>
                {
                    textBox2.AppendText(text);
                }));
            }
            else
            {
                textBox2.AppendText(text);
            }
        }

        /// Resetuje graf pro nové ID.
        private void ResetChart()
        {
            sampleCount = 0;
            chart1.Series.Clear();
            InitializeChart();
        }

        /// Ukončení – uzavření portu (pokud tahle událost používáš).
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SerialManager.Instance.Close();
        }

        private void LoadSensorsFromCsv()
        {
            try
            {
                string baseDir = Directory.GetParent(Application.StartupPath).Parent.Parent.FullName;
                string csvPath = Path.Combine(baseDir, "MTA_Senzory.csv");

                if (!File.Exists(csvPath))
                {
                    MessageBox.Show($"Soubor nebyl nalezen: {csvPath}");
                    return;
                }

                string[] lines;
                try
                {
                    lines = File.ReadAllLines(csvPath, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                }
                catch
                {
                    lines = File.ReadAllLines(csvPath, Encoding.Default);
                }

                if (lines.Length == 0)
                {
                    MessageBox.Show("Soubor je prázdný.");
                    return;
                }

                char sep = lines[0].Contains(";") ? ';' : ',';

                string[] headers = SplitCsvLine(lines[0], sep);
                int idxLabel = -1;
                int idxId = -1;

                for (int i = 0; i < headers.Length; i++)
                {
                    var h = headers[i].Trim();
                    if (idxLabel < 0 && h.Equals("Znaceni", StringComparison.OrdinalIgnoreCase)) idxLabel = i;
                    if (idxId < 0 && h.Equals("id", StringComparison.OrdinalIgnoreCase)) idxId = i;
                }

                if (idxLabel < 0)
                {
                    MessageBox.Show("V prvním řádku nebyl nalezen sloupec „Značení“.");
                    return;
                }
                if (idxId < 0)
                {
                    MessageBox.Show("V prvním řádku nebyl nalezen sloupec „id“.");
                    return;
                }

                sensorIdMap.Clear();
                var labelList = new List<string>();

                for (int r = 1; r < lines.Length; r++)
                {
                    if (string.IsNullOrWhiteSpace(lines[r])) break;

                    string[] cells = SplitCsvLine(lines[r], sep);
                    if (cells.Length <= Math.Max(idxLabel, idxId)) break;

                    string label = cells[idxLabel]?.Trim();
                    string idVal = cells[idxId]?.Trim();

                    if (string.IsNullOrWhiteSpace(label)) break; // stop na prázdném značení
                    if (string.IsNullOrWhiteSpace(idVal)) continue; // bez id přeskoč

                    if (!sensorIdMap.ContainsKey(label)) labelList.Add(label);
                    sensorIdMap[label] = idVal;
                }

                comboBoxSensor.BeginUpdate();
                comboBoxSensor.Items.Clear();
                foreach (var label in labelList)
                    comboBoxSensor.Items.Add(label);
                comboBoxSensor.EndUpdate();

                comboBoxSensor.SelectedIndex = -1; // nic nepředvybírat
                UpdateRequestFromUi();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při načítání CSV: {ex.Message}");
            }
        }

        // jednodušší parser: zvládá uvozovky a oddělovač ; nebo ,
        private static string[] SplitCsvLine(string line, char separator)
        {
            var list = new List<string>();
            if (line == null) return Array.Empty<string>();

            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '\"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                    {
                        sb.Append('\"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == separator && !inQuotes)
                {
                    list.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }
            list.Add(sb.ToString());

            return list.ToArray();
        }
        private void DisplayTimer_Tick(object sender, EventArgs e)
        {
            string snapshot = null;
            lock (_rxLock)
            {
                if (!string.IsNullOrEmpty(_latestDataFrame))
                {
                    snapshot = _latestDataFrame;
                    _latestDataFrame = null; // zobraz jen poslední známý rámec
                }
            }

            if (string.IsNullOrEmpty(snapshot)) return;

            // Jsme na UI vlákně (Timer.Tick) – můžeme volat rovnou
            if (snapshot.StartsWith("?type="))
                ParseAndDisplayData(snapshot);
            else
                ParseInitMessage(snapshot);
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
                    AppendTextBox($"Nenalezen obrázek pro „{label}“ ve složce {sensorsDir}.\r\n");
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
    }
}
