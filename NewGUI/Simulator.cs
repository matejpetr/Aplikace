using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace NewGUI
{
    public partial class Simulator : UserControl
    {
        
        private Timer simulationTimer;
        private string selectedSensor = "";
        private DataTable excelTable;
        private Random random = new Random();
        private Timer comPortWatcherTimer;
        private List<string> lastKnownPorts = new List<string>();
        private bool simulationRunning = false;

        private string BasePath = Directory.GetParent(Application.StartupPath).Parent.Parent.FullName;

        // NEW: režim podle TypeBox (true = Senzory, false = Aktuátory)
        private bool sensorsMode = true;

        public Simulator(Form1 rodic)
        {
            InitializeComponent();

            // Handlery UI (přidat jednou)
            sensorBox.SelectedIndexChanged += sensorBox_UpdateImage;
            sensorBox.TextChanged += sensorBox_UpdateImage;
            TypeBox.SelectedIndexChanged += TypeBox_SelectedIndexChanged;

            // ❌ Inicializace SerialPort – nahrazeno SerialManagerem

            // Timer pro simulaci
            simulationTimer = new Timer();
            simulationTimer.Interval = 1000; // 1 sekunda
            simulationTimer.Tick += SimulationTimer_Tick;

            // Inicializace UI prvků (COM porty)
            Load += Form1_Load;

            // Watcher COM portů
            comPortWatcherTimer = new Timer();
            comPortWatcherTimer.Interval = 500;
            comPortWatcherTimer.Tick += ComPortWatcherTimer_Tick;
            comPortWatcherTimer.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // COM porty
            comBox.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports) comBox.Items.Add(port);
            if (comBox.Items.Count > 0) comBox.SelectedIndex = 0;

            // Aplikuj režim (zavolá naplnění CSV dle volby)
            TypeBox_SelectedIndexChanged(TypeBox, EventArgs.Empty);
        }

        // === Přepínání Senzory / Aktuátory ====================================
        private void TypeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TypeBox.SelectedItem == null) return;

            var choice = TypeBox.SelectedItem.ToString();
            sensorsMode = string.Equals(choice, "Senzory", StringComparison.OrdinalIgnoreCase);

            // Viditelnost a zobrazení vstupu
            lblSensor.Visible = sensorsMode;
            lblAktuator.Visible = !sensorsMode;
            sensorBox.Visible = true;

            // Naplnění bez auto-výběru
            LoadItemsFromCsv(sensorsMode ? "MTA_Senzory.csv" : "MTA_Aktuátory.csv");
            sensorBox.SelectedIndex = -1; // důležité
            selectedSensor = "";

            // Vyčisti náhled, dokud si uživatel nevybere položku
            component_pic.Image?.Dispose();
            component_pic.Image = null;
        }

        // === Načtení položek z CSV do excelTable + sensorBox ==================
        private void LoadItemsFromCsv(string csvFileName)
        {
            try
            {
                string csvPath = Path.Combine(BasePath, csvFileName);
                if (!File.Exists(csvPath))
                {
                    MessageBox.Show($"Soubor {csvFileName} nebyl nalezen v: {BasePath}");
                    excelTable = new DataTable();
                    sensorBox.Items.Clear();
                    return;
                }

                excelTable = new DataTable();
                sensorBox.BeginUpdate();
                sensorBox.Items.Clear();

                using (var reader = new StreamReader(csvPath, Encoding.Default))
                {
                    bool headerRead = false;
                    string[] headers = null;

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        string[] values;
                        if (line.Contains(';')) values = line.Split(';');
                        else if (line.Contains('\t')) values = line.Split('\t');
                        else values = line.Split(',');

                        if (!headerRead)
                        {
                            headers = values.Select(h => h.Trim().Trim('"')).ToArray();
                            foreach (var h in headers) excelTable.Columns.Add(h);
                            headerRead = true;
                            continue;
                        }

                        if (values.Length < headers.Length)
                            Array.Resize(ref values, headers.Length);

                        excelTable.Rows.Add(values);

                        // alias sloupec: "Alias (type)" nebo "Alias"
                        int aliasIdx = Array.FindIndex(headers, h =>
                            h.Equals("Alias (type)", StringComparison.OrdinalIgnoreCase) ||
                            h.Equals("Značení", StringComparison.OrdinalIgnoreCase));

                        if (aliasIdx >= 0)
                        {
                            string alias = values[aliasIdx]?.Trim().Trim('"');
                            if (!string.IsNullOrWhiteSpace(alias))
                                sensorBox.Items.Add(alias);
                        }
                    }
                }

                sensorBox.EndUpdate();

            }
            catch (IOException)
            {
                MessageBox.Show("CSV je pravděpodobně otevřené v jiném programu. Zavři ho a zkus to znovu.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při načítání {csvFileName}: {ex.Message}");
            }
        }

        // === Vyhledání řádku podle aliasu =====================================
        private DataRow NajdiSensor(string sensorAlias)
        {
            if (excelTable == null) return null;
            return excelTable.AsEnumerable()
                .FirstOrDefault(row => row.Table.Columns.Contains("Alias (type)")
                    ? row["Alias (type)"].ToString() == sensorAlias
                    : (row.Table.Columns.Contains("Alias") && row["Alias"].ToString() == sensorAlias));
        }

        // === Generování hodnot =================================================
        private string VygenerujHodnotu(string type)
        {
            var typ = type.Contains(":") ? type.Split(':')[1].Trim().ToLowerInvariant() : type.Trim().ToLowerInvariant();

            if (typ.Contains("bool"))
            {
                return random.Next(0, 2).ToString();
            }
            else if (typ.Contains("float"))
            {
                double value = 20 + random.NextDouble() * 10;
                return value.ToString("F2");
            }
            else if (typ.Contains("int"))
            {
                int value = random.Next(0, 100);
                return value.ToString();
            }
            else if (typ.Contains("string"))
            {
                if (typ.Contains("polarity"))
                {
                    string[] options = { "North", "South", "East", "West" };
                    return options[random.Next(options.Length)];
                }
                else if (typ.Contains("direction"))
                {
                    string[] options = { "Up", "Down", "Left", "Right" };
                    return options[random.Next(options.Length)];
                }
                else
                {
                    string[] defaultOptions = { "ON", "OFF" };
                    return defaultOptions[random.Next(defaultOptions.Length)];
                }
            }
            return "N/A";
        }

        // === Vytvoření response podle šablony/keywords =======================
        private string VytvorResponse(string sensorAlias)
        {
            DataRow sensorRow = NajdiSensor(sensorAlias);
            if (sensorRow == null) return "";

            string responseTemplate = sensorRow.Table.Columns.Contains("Response") ? sensorRow["Response"].ToString() : "";
            string keywords = sensorRow.Table.Columns.Contains("Keywords - values") ? sensorRow["Keywords - values"].ToString() : "";

            if (string.IsNullOrEmpty(responseTemplate) || string.IsNullOrEmpty(keywords))
                return "";

            string result = responseTemplate;

            // Mapování key -> typ
            Dictionary<string, string> keyTypeMap = new Dictionary<string, string>();
            string[] keywordPairs = keywords.Split(',');
            foreach (var pair in keywordPairs)
            {
                string[] parts = pair.Split(':');
                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string type = parts[1].Trim();
                    keyTypeMap[key] = type;
                }
            }

            // Nahrazení hodnot v response
            var matches = Regex.Matches(responseTemplate, @"(\w+)=([^&]*)");
            foreach (Match match in matches)
            {
                string keyInResponse = match.Groups[1].Value; // např. temp
                if (keyTypeMap.TryGetValue(keyInResponse, out string type))
                {
                    string newValue = VygenerujHodnotu(type);
                    string pattern = $@"({keyInResponse}=)[^&]*";
                    result = Regex.Replace(result, pattern, m => m.Groups[1].Value + newValue);
                }
            }

            return result;
        }

        private void SimulationTimer_Tick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedSensor))
                return;

            string responseToSend = VytvorResponse(selectedSensor);

            textBox.AppendText(responseToSend + Environment.NewLine);

            // ⤵ posíláme přes SerialManager
            if (SerialManager.Instance.IsOpen)
            {
                try
                {
                    SerialManager.Instance.WriteLine(responseToSend);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Chyba při odesílání dat: {ex.Message}");
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Globálně zavře (a odpojí případné handlery, i když v Simulatoru RX nepoužíváme)
            SerialManager.Instance.Close();
        }

        private void ComPortWatcherTimer_Tick(object sender, EventArgs e)
        {
            var currentPorts = SerialPort.GetPortNames().ToList();

            if (!currentPorts.SequenceEqual(lastKnownPorts))
            {
                string selected = comBox.SelectedItem as string;

                comBox.Items.Clear();
                comBox.Items.AddRange(currentPorts.ToArray());

                if (selected != null && currentPorts.Contains(selected))
                {
                    comBox.SelectedItem = selected;
                }
                else if (currentPorts.Count > 0)
                {
                    comBox.SelectedIndex = 0;
                }

                lastKnownPorts = currentPorts;
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (SerialManager.Instance.IsOpen == false)
            {
                if (comBox.SelectedItem == null)
                {
                    MessageBox.Show("Vyberte COM port.");
                    return;
                }

                try
                {
                    // Nastavení a otevření sdíleného seriáku
                    SerialManager.Instance.ConfigurePort(
                        portName: comBox.SelectedItem.ToString(),
                        baudRate: 115200,
                        parity: Parity.None,
                        dataBits: 8,
                        stopBits: StopBits.One,
                        handshake: Handshake.None,
                        newLine: "\n"
                    );

                    // V Simulatoru příjem nepotřebujeme → nepřipojujeme žádný DataReceived handler
                    // SerialManager.Instance.AttachExclusiveReceiver(SomeRxHandler);

                    SerialManager.Instance.Open();

                    btnConnect.Text = "Odpojit";
                    comBox.Enabled = false;
                    badgeConn.Text = "Připojeno";
                    badgeConn.BackColor = Color.FromArgb(46, 125, 50);
                    btnStartStop.Enabled = true;
                    TypeBox.Enabled = true;
                    btnStartStop.Cursor = Cursors.Hand;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Chyba při připojení: {ex.Message}");
                    badgeConn.Text = "Chyba";
                    badgeConn.BackColor = Color.FromArgb(211, 47, 47);
                }
            }
            else
            {
                try
                {
                    // Odpoj a ukliď
                    simulationRunning = false;
                    simulationTimer.Stop();
                    SerialManager.Instance.Close();
                }
                finally
                {
                    btnConnect.Text = "Připojit";
                    comBox.Enabled = true;
                    badgeConn.Text = "Nepřipojeno";
                    badgeConn.BackColor = Color.FromArgb(107, 114, 128);
                    btnStartStop.Enabled = false;
                    TypeBox.Enabled = false;
                    btnStartStop.Cursor = Cursors.Arrow;
                }
            }
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (!simulationRunning)
            {
                if (!SerialManager.Instance.IsOpen)
                {
                    MessageBox.Show("Nejprve připojte zařízení.");
                    return;
                }
                if (sensorBox.SelectedItem == null)
                {
                    MessageBox.Show("Vyberte položku.");
                    return;
                }

                selectedSensor = sensorBox.SelectedItem.ToString();
                simulationTimer.Start();
                simulationRunning = true;
                btnStartStop.Text = ("Zastavit");
                btnStartStop.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(183)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
                btnStartStop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
                btnStartStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(47)))), ((int)(((byte)(47)))));
                btnStartStop.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(47)))), ((int)(((byte)(47)))));
            }
            else
            {
                simulationRunning = false;
                simulationTimer.Stop();
                btnStartStop.Text = ("Spustit");
                selectedSensor = "";
                textBox.Text += "Simulace byla zastavena" + Environment.NewLine;
                this.btnStartStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(108)))), ((int)(((byte)(189)))));
                this.btnStartStop.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(108)))), ((int)(((byte)(189)))));
                this.btnStartStop.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(94)))), ((int)(((byte)(163)))));
                this.btnStartStop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(83)))), ((int)(((byte)(146)))));
            }
        }

        // === Náhled obrázku podle režimu (Senzory/Aktuátory) ==================
        private void sensorBox_UpdateImage(object sender, EventArgs e)
        {
            var folder = sensorsMode ? "Senzory" : "Aktuátory";
            var path = Path.Combine(BasePath, folder, $"{sensorBox.Text}.png");

            if (!File.Exists(path))
            {
                component_pic.Image?.Dispose();
                component_pic.Image = null;
                return;
            }

            try
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var img = Image.FromStream(fs, useEmbeddedColorManagement: true, validateImageData: true))
                {
                    component_pic.SizeMode = PictureBoxSizeMode.Zoom;
                    component_pic.Image = new Bitmap(img);
                }
            }
            catch
            {
                // ticho – soubor mohl zmizet / nebýt validní obrázek
            }
        }
    }
}
