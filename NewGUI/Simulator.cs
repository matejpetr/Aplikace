using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NewGUI
{
    public partial class Simulator : UserControl
    {
        
        private Timer simulationTimer;
        private string selectedSensor = "";
        private Random random = new Random();
        private Timer comPortWatcherTimer;
        private List<string> lastKnownPorts = new List<string>();
        private bool simulationRunning = false;
        private List<Komponenty> SenzoryList;
        private string BasePath = Directory.GetParent(Application.StartupPath).Parent.Parent.FullName;
        private readonly Dictionary<string, string> sensorIdMap // Mapa „Znackeni“ -> „Id“ (string)
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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
            LoadSensorsFromJson();
            sensorBox.SelectedIndex = -1; // důležité
            selectedSensor = "";

            // Vyčisti náhled, dokud si uživatel nevybere položku
            component_pic.Image?.Dispose();
            component_pic.Image = null;
        }

        // === Načtení položek z CSV do excelTable + sensorBox ==================
        private void LoadSensorsFromJson()                                        // Metoda bez vstupů – načte JSON a naplní UI + mapu
        {                                                                         // Začátek bloku metody
            try                                                                   // Try-catch: kdyby cokoliv spadlo (soubor, JSON), zobrazíme hlášku
            {                                                                     // Začátek try
                                                                                  // JSON musí být vedle .exe (Properties: Content + Copy if newer)
                string jsonPath = Path.Combine(Application.StartupPath,           // Poskládáme absolutní cestu...
                                                "Senzory.json");                   // ...k souboru Senzory.json ve stejné složce jako .exe

                if (!File.Exists(jsonPath))                                       // Ověříme, že soubor opravdu existuje
                {                                                                 // Pokud neexistuje:
                    MessageBox.Show("Soubor Senzory.json nebyl nalezen v "        // ...ukaž informaci uživateli
                                     + Application.StartupPath + ".");            // ...a doplň, kde jsme hledali
                    return;                                                        // A ukonči metodu (není co načítat)
                }                                                                 // Konec if

                string jsonText = File.ReadAllText(jsonPath);                     // Načti celý obsah souboru jako text (string)

                var data = JsonSerializer.Deserialize<List<Komponenty>>(          //Převeď JSON text na List<Komponenty>
                    jsonText,                                                     //Vstup: načtený JSON řetězec
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true } //Nastavení: názvy vlastností nerozlišuj podle velikosti písmen
                );                                                                //Konec volání Deserialize

                if (data == null || data.Count == 0)                              //Ověření: máme nějaká data?
                {                                                                 //Pokud ne:
                    MessageBox.Show("Senzory.json je prázdný nebo ve špatném formátu."); //...informuj uživatele
                    return;                                                       // ...a skonči
                }                                                                 // Konec if

                SenzoryList = data;                                            // Ulož načtený seznam do pole třídy (budeme s tím dál pracovat)

                sensorIdMap.Clear();                                              // Vyčisti mapu „Znaceni → Id“ (aby tam nezůstaly staré hodnoty)
                sensorBox.BeginUpdate();                                     // Optimalizace vykreslování při hromadném plnění comboboxu
                sensorBox.Items.Clear();                                     // Vyprázdni položky comboboxu

                foreach (var k in SenzoryList)                                 // Projdi všechny prvky z JSONu (každý „senzor“)
                {                                                                 // Začátek foreach
                    string label = (k.Znaceni ?? string.Empty).Trim();           // Vytáhni text „Znackeni“ (název/štítek do UI), ošetři null a ořízni
                    if (string.IsNullOrWhiteSpace(label))                         // Když je prázdný/whitespace,
                        continue;                                                 // ...tuhle položku přeskoč

                    if (!sensorIdMap.ContainsKey(label))                          // Je to nový label? (ještě není v mapě)
                        sensorBox.Items.Add(label);                          // ...tak ho přidej do comboboxu (aby šel vybrat)

                    sensorIdMap[label] = k.Id.ToString();                         // Do mapy ulož dvojici Label → Id (Id převedeme na string)
                                                                                  // POZN: Pokud máš v modelu Id už jako "Sxx" string, napiš prostě: sensorIdMap[label] = k.Id;
                }                                                                 // Konec foreach

                sensorBox.EndUpdate();                                       // Ukonči hromadnou aktualizaci (UI jednorázově překreslí změny)
                sensorBox.SelectedIndex = -1;                                // Nic nepředvybírej (uživatel si vybere sám)
                //UpdateRequestFromUi();                                            // Přepočítej náhled požadavku (label8, tlačítko Start apod.)
            }                                                                     // Konec try
            catch (Exception ex)                                                  // Zachytíme libovolnou výjimku (soubor, parsování…)
            {                                                                     // Začátek catch
                MessageBox.Show("Chyba při načítání Senzory.json: " + ex.Message);// Ukaž chybovou hlášku s důvodem
            }                                                                     // Konec catch
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
        public string VytvorResponse(Komponenty sensor)
        {
            if (sensor == null) return "";

            string type = sensor.Znaceni ?? "UNKNOWN";
            string id = $"S{sensor.Id}";
            var sb = new StringBuilder();

            sb.Append("?type=").Append(Uri.EscapeDataString(type));
            sb.Append("&id=").Append(id);

            if (sensor.Keywords_values != null && sensor.Keywords_values.Count > 0)
            {
                foreach (var kv in sensor.Keywords_values)
                {
                    string key = kv.Key;
                    string valType = kv.Value;
                    string val = VygenerujHodnotu(valType);
                    sb.Append("&").Append(key).Append("=").Append(val);
                }
            }

            return sb.ToString();
        }

        private void SimulationTimer_Tick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedSensor))
                return;

            // Najdi objekt podle jména (v JSONu je Znaceni = stejný text jako v sensorBox)
            var sensorObj = SenzoryList.FirstOrDefault(s =>
                string.Equals(s.Znaceni, selectedSensor, StringComparison.OrdinalIgnoreCase));

            if (sensorObj == null)
                return;

            string responseToSend = VytvorResponse(sensorObj);

            AppendLineToTextBox(responseToSend);


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
                btnConnect.Enabled = false;
            }
            else
            {
                simulationRunning = false;
                simulationTimer.Stop();
                btnStartStop.Text = ("Spustit");
                selectedSensor = "";
                AppendLineToTextBox("Simulace byla zastavena");
                this.btnStartStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(108)))), ((int)(((byte)(189)))));
                this.btnStartStop.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(108)))), ((int)(((byte)(189)))));
                this.btnStartStop.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(94)))), ((int)(((byte)(163)))));
                this.btnStartStop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(83)))), ((int)(((byte)(146)))));
                btnConnect.Enabled = true;
            }
        }

        private void AppendLineToTextBox(string text)
        {
            textBox.AppendText(text + Environment.NewLine);
            textBox.SelectionStart = textBox.Text.Length;
            textBox.ScrollToCaret();
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
