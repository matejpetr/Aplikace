using System;                                                   // Základní typy a události
using System.Collections.Generic;                               // Kolekce jako List<>, Dictionary<>
using System.Data;                                              // (aktuálně nepoužito, ale může se hodit pro DataTable)
using System.Drawing;                                           // Barvy a grafické typy (pro graf/obrázky)
using System.Linq;                                              // LINQ operace (Where, Select, ToDictionary apod.)
using System.Text;                                              // StringBuilder a textové utility
using System.Threading.Tasks;                                   // async/await Task
using System.Windows.Forms;                                     // WinForms UI
using System.IO.Ports;                                          // Sériová komunikace (SerialPort)
using System.Windows.Forms.DataVisualization.Charting;          // Ovládací prvek Chart
using System.IO;                                                // Práce se soubory a cestami
using System.Text.Json;                                         // JSON serializace/deserializace

namespace NewGUI                                                // Namespace projektu
{
    public partial class Senzory : UserControl                  // Uživatelský ovládací prvek Senzory
    {
        private bool isSendingRequest = false;                  // Flag: probíhá cyklické odesílání požadavků?
        public string request;                                  // Poslední sestavený požadavek (řetězec)
        private int sampleCount = 0;                            // Počet vykreslených vzorků do grafu
        private string lastUsedID = null;                       // Posledně použitý „ID“ (kvůli resetu grafu při změně)
        private Random rnd = new Random();                      // Generátor náhodných barev pro série v grafu
        private Timer comPortWatcherTimer;                      // Timer pro sledování změn dostupných COM portů
        private List<string> lastKnownPorts = new List<string>(); // Poslední známý seznam COM portů (na porovnání)
        private readonly Dictionary<string, string> sensorIdMap // Mapa „Znackeni“ -> „Id“ (string)
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly object _rxLock = new object();         // Zámek pro thread-safe přístup k přijatým datům
        private string _latestDataFrame;                        // Poslední přijatá věta (zobrazíme ji periodicky timerem)
        private Timer displayTimer;                             // Timer pro throttlované vykreslování přijatých dat
        private System.Threading.CancellationTokenSource _sendCts; // CTS pro zrušení cyklického odesílání
        private readonly StringBuilder _rxBuffer = new StringBuilder();

        private List<Komponenty> SenzoryData;                // Načtená data z Senzory.json (silně typovaná)
        private string _lastSentMode = null;
        public Senzory(Form1 rodic)                             // Konstruktor ovládacího prvku
        {
            InitializeComponent();                              // Inicializace WinForms komponent
            InitializeChart();                                  // Nastavení výchozího vzhledu grafu

            comboBoxTIMER.SelectedIndex = 1;                    // Výchozí položka v comboboxu pro periodu (např. 100 ms)

            displayTimer = new Timer();                         // Vytvoření timeru pro vykreslování přijatých dat
            displayTimer.Interval = 100;                        // Default perioda 100 ms (příp. se přepíše dle UI)
            displayTimer.Tick += DisplayTimer_Tick;             // Handler pro každé tiknutí timeru
            displayTimer.Start();                               // Start zobrazovacího timeru

            comboBoxTIMER.SelectedIndexChanged += (s, e) => ApplyTimerIntervalFromUi(); // Při změně periody v UI
            ApplyTimerIntervalFromUi();                         // Hned nastav periodu podle aktuální hodnoty UI

            comPortWatcherTimer = new Timer();                  // Timer pro sledování dostupných COM portů
            comPortWatcherTimer.Interval = 500;                 // Kontrola každých 500 ms
            comPortWatcherTimer.Tick += ComPortWatcherTimer_Tick; // Reakce na tik - aktualizace seznamu portů
            comPortWatcherTimer.Start();                        // Spusť sledování COM portů

            SetUiForConnection(false);                          // Inicializuj UI jako „nepřipojeno“

            LoadSensorsFromJson();                              // Načti seznam senzorů ze souboru Senzory.json

            comboBoxSensor.SelectedIndex = -1;                  // Nepředvybírej žádný senzor
            comboBoxMode.SelectedIndex = -1;                    // Nepředvybírej žádný mód

            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;     // Obrázky senzorů hezky „přizpůsobit“

            comboBoxSensor.SelectedIndexChanged += comboBoxSensor_SelectedIndexChanged; // Po změně senzoru nahraj jeho obrázek
            comboBoxSensor.SelectedIndexChanged += (s, e) => UpdateRequestFromUi();     // A přepočítej request
            comboBoxMode.SelectedIndexChanged += (s, e) => UpdateRequestFromUi();

            // Změna módu → přepočti request

            // když uživatel dopíše piny, hned se přepočítá request a povolí Start
            textPIN1.TextChanged += (s, e) => UpdateRequestFromUi();
            textPIN2.TextChanged += (s, e) => UpdateRequestFromUi();

        }

        private void InitializeChart()                          // Nastavení výchozí podoby grafu
        {
            chart1.Series.Clear();                              // Odstraň stávající série

            Series series = new Series("measuring")             // Vytvoř základní sérii pro měření
            {
                ChartType = SeriesChartType.Line,               // Čárový graf
                XValueType = ChartValueType.Int32,              // X je integer (pořadí vzorku)
                YValueType = ChartValueType.Double,             // Y je double (měřená hodnota)
                IsVisibleInLegend = false                       // Nechceme legendu pro tuto sérii
            };
            chart1.Series.Add(series);                          // Přidej sérii do grafu

            chart1.ChartAreas[0].AxisX.Title = "Počet vzorků";  // Popisek osy X
            chart1.ChartAreas[0].AxisY.LineWidth = 2;           // Tloušťka osy Y
            chart1.Series["measuring"].BorderWidth = 2;         // Tloušťka čáry série
            chart1.Series["measuring"].Color = Color.Black;     // Barva čáry (základní)
        }

        // Najde vybranou položku z načtených SenzoryData podle zobrazeného Znaceni
        private Komponenty FindSelectedComponent()
        {
            var label = comboBoxSensor.Text?.Trim();
            if (string.IsNullOrWhiteSpace(label) || SenzoryData == null) return null;
            return SenzoryData.FirstOrDefault(k =>
                string.Equals(k.Znaceni?.Trim(), label, StringComparison.OrdinalIgnoreCase));
        }

        // Podle módu a dat z JSONu ukáže/skrývá pin vstupy + nastaví popisky
        // Podle módu a dat z JSONu ukáže/skrývá pin vstupy + nastaví popisky
        private void UpdatePinInputsUi()
        {
            // 1) VŽDY nejdřív všechno schovat (a volitelně vyčistit)
            PIN1.Visible = PIN2.Visible = false;
            textPIN1.Visible = textPIN2.Visible = false;
            // případně chceš i mazat texty:
            // textPIN1.Clear();
            // textPIN2.Clear();
            // a popisky třeba do defaultu:
            // PIN1.Text = "PIN1";
            // PIN2.Text = "PIN2";

            // 2) Piny ukazujeme jen pro CONNECT/DISCONNECT
            string mode = comboBoxMode.Text?.Trim();
            bool isConnMode = mode.Equals("CONNECT", StringComparison.OrdinalIgnoreCase)
                           || mode.Equals("DISCONNECT", StringComparison.OrdinalIgnoreCase);
            if (!isConnMode) return; // jsme v jiném módu → zůstanou skryté

            // 3) Najdi položku z JSONu
            var item = FindSelectedComponent();
            if (item == null) return; // fallback necháme skrytý (nebo si můžeš zobrazit aspoň PIN1)

            // 4) Zobraz podle JSONu
            if (!string.IsNullOrWhiteSpace(item.PIN1))
            {
                PIN1.Text = item.PIN1;
                PIN1.Visible = true;
                textPIN1.Visible = true;
            }

            if (!string.IsNullOrWhiteSpace(item.PIN2))
            {
                PIN2.Text = item.PIN2;
                PIN2.Visible = true;
                textPIN2.Visible = true;

                // kdyby náhodou chyběl PIN1 v JSONu, ale je PIN2, ukaž aspoň PIN1 jako placeholder
                if (!PIN1.Visible)
                {
                    PIN1.Text = "PIN1";
                    PIN1.Visible = true;
                    textPIN1.Visible = true;
                }
            }
        }


        // vyčistí mezery a (volitelně) vyextrahuje číslice - "D2" -> "2", "GPIO 14" -> "14"
        private static string NormalizePinInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            input = input.Trim();

            var digits = new string(input.Where(char.IsDigit).ToArray());
            return string.IsNullOrEmpty(digits) ? input : digits;

            // pokud chceš zachovat přesně co uživatel zadal (třeba "D2"), vrať jen: return input.Trim();
        }

        // Vrátí text pro &pin=... podle JSONu a vyplněných textboxů
        // 1 pin  -> "\"13\""             // (včetně uvozovek)
        // 2 piny -> "\"5\",\"18\""
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
                    return null; // chybí některý pin
                return $"{p1},{p2}";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(p1))
                    return null; // chybí pin
                return $"{p1}";
            }
        }


        private void UpdateRequestFromUi()
        {
            bool hasSensor = comboBoxSensor.SelectedIndex >= 0 && !string.IsNullOrWhiteSpace(comboBoxSensor.Text);
            bool hasMode = comboBoxMode.SelectedIndex >= 0 && !string.IsNullOrWhiteSpace(comboBoxMode.Text);
            bool connected = SerialManager.Instance.IsOpen;

            // vždy nejdřív uprav UI pinů podle módu/JSONu
            UpdatePinInputsUi();

            if (hasSensor && hasMode)
            {
                string sensorLabel = comboBoxSensor.Text.Trim(); // "DS18B20"...
                string mode = comboBoxMode.Text.Trim();   // INIT/UPDATE/CONFIG/RESET/CONNECT/DISCONNECT...

                if (!sensorIdMap.TryGetValue(sensorLabel, out string sensorId) || string.IsNullOrWhiteSpace(sensorId))
                    sensorId = sensorLabel;

                string formattedId = FormatSensorId(sensorId);   // Sxx

                bool isConnMode = mode.Equals("CONNECT", StringComparison.OrdinalIgnoreCase)
                               || mode.Equals("DISCONNECT", StringComparison.OrdinalIgnoreCase);

                if (mode.Equals("INIT", StringComparison.OrdinalIgnoreCase))
                {
                    request = $"?type={mode}&app=APP_NAME&version=APP_VERSION&dbversion=DB_VERSION&api=API_VERSION";
                }
                else if (isConnMode)
                {
                    // připrav výraz pro pin
                    string pinExpr = BuildPinExpr(); // vrátí "\"13\"" nebo "\"5\",\"18\"" nebo null

                    // pokud ještě nemám vyplněné piny, zobraz aspoň náhled bez nich
                    if (string.IsNullOrWhiteSpace(pinExpr))
                    {
                        request = $"?type={mode}&id={formattedId}&pin="; // Start tlačítko stejně níže nepovolíme
                    }
                    else
                    {
                        // a) pokud používáš šablonu s 'PIN', nahradíme
                        string baseReq = $"?type={mode}&id={formattedId}&pin=PIN";
                        request = baseReq.Replace("PIN", pinExpr);

                        // b) kdybys nechtěl šablonu, tak prostě:
                        // request = $"?type={mode}&id={formattedId}&pin={pinExpr}";
                    }
                }
                else
                {
                    request = $"?type={mode}&id={formattedId}";
                }

                label8.Text = request;
            }
            else
            {
                request = null;
                label8.Text = string.Empty;
            }

            // --- povolení Start tlačítka ---
            bool ready = connected && hasSensor && hasMode;

            // CONNECT/DISCONNECT navíc vyžadují vyplněné piny podle JSONu
            string m = comboBoxMode.Text?.Trim() ?? "";
            bool isConn = m.Equals("CONNECT", StringComparison.OrdinalIgnoreCase) || m.Equals("DISCONNECT", StringComparison.OrdinalIgnoreCase);
            if (isConn)
            {
                var item = FindSelectedComponent();
                if (item != null)
                {
                    bool needTwo = !string.IsNullOrWhiteSpace(item.PIN2);
                    bool p1ok = !string.IsNullOrWhiteSpace(NormalizePinInput(textPIN1.Text));
                    bool p2ok = !needTwo || !string.IsNullOrWhiteSpace(NormalizePinInput(textPIN2.Text));
                    ready = ready && p1ok && p2ok;
                }
            }

            button1.Enabled = ready;
        }


        private void ApplyTimerIntervalFromUi()                 // Přenastaví periodu vykreslovacího timeru dle UI
        {
            string txt = comboBoxTIMER.Text?.Trim();            // Text z comboboxu
            int delay;                                          // Cílová perioda v ms
            if (!int.TryParse(txt, out delay) || delay < 10)    // Ošetření: minimálně 10 ms, jinak default
                delay = 100;

            displayTimer.Interval = delay;                      // Nastav periodu zobrazovacího timeru
            // Pozn.: perioda odesílání se čte v SendLoopAsync (bereme stejnou hodnotu z UI)
        }

        private void SetUiForConnection(bool isConnected)       // Přepne stavy ovládacích prvků podle připojení
        {
            comboBoxCOM.Enabled = !isConnected;                 // Při připojení zamknout výběr portu

            comboBoxSensor.Enabled = isConnected;               // Povolit senzory až po připojení
            comboBoxMode.Enabled = isConnected;                 // Povolit volbu módu až po připojení
            comboBoxTIMER.Enabled = isConnected;                // Povolit změny periody až po připojení

            button1.Enabled = false;                            // Start vypneme (zapne se po splnění podmínek)

            if (ConnectBtn != null)                             // Ochrana pokud designér generuje jinak
                ConnectBtn.Text = isConnected ? "Odpojit" : "Připojit"; // Text tlačítka připojení

            badgeConn.Text = isConnected ? "Připojeno" : "Nepřipojeno"; // Text „badge“ stavu
            badgeConn.BackColor = isConnected                   // Barva „badge“ dle stavu
                ? Color.FromArgb(46, 125, 50)
                : Color.FromArgb(107, 114, 128);

            UpdateRequestFromUi();                              // Po změně stavu přepočti požadavek
        }

        private void ComPortWatcherTimer_Tick(object sender, EventArgs e) // Každých 500 ms zkontroluj COM porty
        {
            var currentPorts = SerialPort.GetPortNames().ToList(); // Získej aktuální seznam COM portů

            if (!currentPorts.SequenceEqual(lastKnownPorts))    // Změnil se seznam?
            {
                string selected = comboBoxCOM.SelectedItem as string; // Původně vybraný port

                comboBoxCOM.Items.Clear();                      // Vyčisti combobox
                comboBoxCOM.Items.AddRange(currentPorts.ToArray()); // Naplň novými porty

                if (selected != null && currentPorts.Contains(selected)) // Pokud původní stále existuje
                {
                    comboBoxCOM.SelectedItem = selected;        // ponech ho
                }
                else if (currentPorts.Count > 0)                // Jinak když nějaké jsou
                {
                    comboBoxCOM.SelectedIndex = 0;              // vyber první
                }

                lastKnownPorts = currentPorts;                  // Ulož „last known“ seznam
            }
        }

        private void ConnectBtn_Click(object sender, EventArgs e) // Handler tlačítka Připojit/Odpojit
        {
            if (SerialManager.Instance.IsOpen)                  // Pokud už je otevřeno → odpojovat
            {
                try
                {
                    StopSendingRequest();                       // Ukonči případný odesílací cyklus
                    SerialManager.Instance.DetachReceiver();    // Odpoj handler pro příjem
                    SerialManager.Instance.Close();             // Zavři port
                    AppendTextBox("Odpojeno od portu.\r\n");    // Log do textBoxu
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Chyba při odpojování: {ex.Message}"); // Chybová hláška
                }
                finally
                {
                    SetUiForConnection(false);                  // UI → stav nepřipojeno
                    UpdateRequestFromUi();                      // Přepočítej požadavek
                }
                return;                                         // hotovo
            }

            string selectedPort = comboBoxCOM.Text?.Trim();     // Zvolený COM port z UI
            if (string.IsNullOrWhiteSpace(selectedPort))        // Nic nevybráno?
            {
                MessageBox.Show("Prosím vyber COM port.");      // Upozorni uživatele
                return;                                         // ukonči
            }

            try
            {
                SerialManager.Instance.ConfigurePort(           // Nastav parametry portu
                    portName: selectedPort,
                    baudRate: 115200,
                    parity: Parity.None,
                    dataBits: 8,
                    stopBits: StopBits.One,
                    handshake: Handshake.None,
                    newLine: "\n"
                );

                SerialManager.Instance.AttachExclusiveReceiver(SerialPort_DataReceived); // Připoj handler příjmu
                SerialManager.Instance.Open();                    // Otevři port

                SetUiForConnection(true);                        // UI → stav připojeno
                AppendTextBox($"Připojeno k {selectedPort}.\r\n"); // Log
                UpdateRequestFromUi();                           // Přepočítej požadavek
            }
            catch (Exception ex)
            {
                SetUiForConnection(false);                       // Při chybě vrať UI do nepřipojeno
                MessageBox.Show($"Chyba při otevírání portu: {ex.Message}"); // Hláška chyby
                badgeConn.Text = "Chyba";                        // Zobraz „Chyba“
                badgeConn.BackColor = Color.FromArgb(211, 47, 47); // Červená barva badge
                UpdateRequestFromUi();                           // Přepočítej požadavek
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            UpdateRequestFromUi();

            string selectedPort = comboBoxCOM.Text?.Trim();
            string currentID = comboBoxSensor.Text?.Trim();
            string currentType = comboBoxMode.Text?.Trim();

            if (button1.Text == "Spustit")
            {
                // Pokud jdu spustit CONNECT/DISCONNECT a ještě běží cyklus (Zastavit),
                // nejdřív cyklus ukončit a UI vrátit do "Spustit".
                var intendedMode = comboBoxMode.Text?.Trim() ?? "";
                bool wantsConn = intendedMode.Equals("CONNECT", StringComparison.OrdinalIgnoreCase)
                              || intendedMode.Equals("DISCONNECT", StringComparison.OrdinalIgnoreCase);

                if (wantsConn && button1.Text == "Zastavit")
                {
                    // simuluj ruční STOP
                    comboBoxSensor.Enabled = true;
                    comboBoxMode.Enabled = true;
                    comboBoxCOM.Enabled = true;
                    comboBoxTIMER.Enabled = true;
                    ConnectBtn.Enabled = true;
                    button1.Text = "Spustit";

                    StopSendingRequest();
                    textBox2.AppendText("Měření pozastaveno (přepnutí na CONNECT/DISCONNECT).\r\n");

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

                // Reset grafu při změně ID (jako dřív)
                if (currentID != lastUsedID)
                {
                    ResetChart();
                    lastUsedID = currentID;
                }

                

                // === ONE-SHOT PRO CONNECT/DISCONNECT ===
                bool isConnMode = currentType.Equals("CONNECT", StringComparison.OrdinalIgnoreCase)
                               || currentType.Equals("DISCONNECT", StringComparison.OrdinalIgnoreCase);

                if (isConnMode)
                {
                    try
                    {
                        if (request == null)
                        {
                            AppendTextBox("Požadavek není sestaven.\r\n");
                            return;
                        }

                        displayTimer?.Start();                 // ✅ přidej: zajistí zpracování odpovědi
                        SerialManager.Instance.WriteLine(request);
                        AppendTextBox($"Odesláno: {request}\r\n");
                        _lastSentMode = null;
                        return;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Chyba při odesílání: {ex.Message}");
                    }
                    return;
                }

                // === OSTATNÍ MÓDY (původní chování) ===
                _lastSentMode = null; // nový režim → nechceme „lepivý“ CONNECT/DISCONNECT stav
                StartSendingRequest();

                // zamknout UI a přepnout tlačítko na „Zastavit“ jen pro režimy, které to dávají smysl
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

                StopSendingRequest();
                textBox2.AppendText("Měření pozastaveno.\r\n");

                button1.BackColor = Color.FromArgb(15, 108, 189);
                button1.FlatAppearance.BorderColor = Color.FromArgb(15, 108, 189);
                button1.FlatAppearance.MouseDownBackColor = Color.FromArgb(17, 94, 163);
                button1.FlatAppearance.MouseOverBackColor = Color.FromArgb(12, 83, 146);

                UpdateRequestFromUi();
            }
        }


        private void StartSendingRequest()                        // Spuštění odesílání požadavku
        {
            if (request == null)                                  // Není co poslat?
            {
                AppendTextBox("Požadavek není sestaven.\r\n");    // Informuj
                return;                                           // Ukonči
            }

            displayTimer?.Start();                                // Zapni zobrazovací timer (pro jistotu)

            _sendCts?.Cancel();                                   // Zruš případné předchozí odesílání
            _sendCts?.Dispose();                                  // Uvolni zdroje
            _sendCts = new System.Threading.CancellationTokenSource(); // Nový cancellation token

            if (request.StartsWith("?type=update", StringComparison.OrdinalIgnoreCase)) // UPDATE = cyklické
            {
                isSendingRequest = true;                          // Nastav flag cyklení
                _ = SendLoopAsync(_sendCts.Token);                // Spusť smyčku na pozadí (fire-and-forget)
            }
            else                                                  // Ostatní módy = jednorázový zápis
            {
                try
                {
                    if (SerialManager.Instance.IsOpen)            // Jen když je port otevřený
                        SerialManager.Instance.WriteLine(request);// Odejdi požadavek
                    else
                        AppendTextBox("Port není otevřen – požadavek se neodešle.\r\n"); // Informuj
                }
                catch (Exception ex)
                {
                    AppendTextBox($"Chyba při zápisu: {ex.Message}\r\n"); // Zachyť chybu zápisu
                }
                isSendingRequest = false;                         // Neprobíhá cyklus
            }
        }

        private async Task SendLoopAsync(System.Threading.CancellationToken ct) // Smyčka cyklického odesílání
        {
            while (!ct.IsCancellationRequested &&                // Dokud není zrušeno
                   SerialManager.Instance.IsOpen &&               // a port je otevřen
                   isSendingRequest)                              // a máme cyklus povolen
            {
                int delay = 100;                                  // Default delay
                var txt = comboBoxTIMER?.Text?.Trim();            // Načti z UI
                if (!int.TryParse(txt, out delay) || delay < 1)   // Ošetření chybné hodnoty
                    delay = 100;

                try
                {
                    await Task.Delay(delay, ct);                  // Počkej daný interval (zrušitelné)
                    if (ct.IsCancellationRequested) break;        // Pokud zrušeno, vyskoč

                    SerialManager.Instance.WriteLine(request);    // Odeslat požadavek
                }
                catch (OperationCanceledException)                // Zrušeno čekání
                {
                    break;                                        // Ukonči smyčku
                }
                catch (Exception ex)                              // Jiná chyba
                {
                    AppendTextBox($"Chyba při zápisu: {ex.Message}\r\n"); // Napiš chybu
                    break;                                        // Ukonči smyčku
                }
            }
        }

        private void StopSendingRequest()                         // Zastavení cyklického odesílání
        {
            isSendingRequest = false;                             // Vypni flag

            displayTimer?.Stop();                                 // Okamžitě zastav zobrazování

            _sendCts?.Cancel();                                   // Zruš čekající delay/odeslání

            lock (_rxLock) _latestDataFrame = null;               // Vymaž poslední přijatý rámec

            try
            {
                // Případně lze doplnit SerialManager.Instance.DiscardInOut(); // Vyprázdnění HW bufferů
            }
            catch { }                                             // Tiché ignorování chyb
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e) // Příjem dat z portu
        {
            try
            {
                var port = sender as SerialPort;
                string data = port?.ReadExisting();
                if (string.IsNullOrEmpty(data)) return;

                // Hromadíme do bufferu – může přijít víc rámců najednou, nebo jen půl
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
            data = data.Trim();
            data = data.TrimStart('\uFEFF'); // kdyby se BOM dostal až sem
            if (data.StartsWith("?")) data = data.Substring(1);

            var parameters = data.Split('&')
                                 .Select(part => part.Split('='))
                                 .Where(pair => pair.Length == 2)
                                 .ToDictionary(pair => pair[0], pair => pair[1]);

            var skipKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"type", "id", "pin", "app", "version", "dbversion", "api", "status", "code" };


            var dataForGraph = parameters
                .Where(kvp => !skipKeys.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            List<string> valueTexts = new List<string>();

            foreach (var kvp in dataForGraph)
            {
                string variableName = kvp.Key;
                string raw = kvp.Value ?? string.Empty;

                string normalized = raw;
                if (normalized.IndexOf(',') >= 0 && normalized.IndexOf('.') < 0)
                    normalized = normalized.Replace(',', '.');

                var m = System.Text.RegularExpressions.Regex.Match(
                            normalized, @"[-+]?\d+(?:\.\d+)?(?:[eE][-+]?\d+)?");

                double numericValue = 0.0; // inicializace pro CS0165
                bool hasNumber = m.Success && double.TryParse(
                    m.Value,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out numericValue);

                if (hasNumber)
                {
                    // krátký debug – uvidíš v textBox2, co se opravdu vykreslilo
                    AppendTextBox($"[GRAPH] {variableName} -> {numericValue}\r\n");

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
                            chart1.ChartAreas.Add(new ChartArea()); // pojistka

                        chart1.ChartAreas[0].AxisX.Minimum = Math.Max(0, sampleCount - 10);
                        chart1.ChartAreas[0].AxisX.Maximum = sampleCount;
                        chart1.ChartAreas[0].RecalculateAxesScale();
                        chart1.ChartAreas[0].AxisY.Title = dataForGraph.Count > 1 ? "Values" : variableName.ToUpper();
                    }));
                }
                else
                {
                    valueTexts.Add($"{variableName}: {raw}");
                }
            }

            if (valueTexts.Count > 0)
                AppendTextBox(string.Join(", ", valueTexts) + "\r\n");

            // posun X po každém rámci (i když byl jen jeden klíč)
            sampleCount++;
            chart1.Invalidate();
        }



        private static string FormatSensorId(string rawId)        // Normalizace ID do tvaru Sxx
        {
            if (string.IsNullOrWhiteSpace(rawId)) return rawId;  // Prázdné → vrať jak je

            string t = rawId.Trim();                              // Ořízni

            if (t.StartsWith("S", StringComparison.OrdinalIgnoreCase)) // Pokud už začíná S/s
                return "S" + t.Substring(1);                     // Normalizuj velké „S“

            if (int.TryParse(t, out int n) && n >= 0)            // Pokud je to čisté číslo
                return "S" + n.ToString("D2");                   // Naformátuj Sxx

            var digits = new string(t.Where(char.IsDigit).ToArray()); // Vytáhni číslice z textu
            if (int.TryParse(digits, out n) && n >= 0)           // Zkus převést
                return "S" + n.ToString("D2");                   // Naformátuj

            return "S" + t;                                      // Fallback: prostě předřaď „S“
        }

        private void ParseInitMessage(string data)                // Parsování speciální INIT odpovědi (typ: id1:typ1,id2:typ2,…)
        {
            string rawData = data.Trim();                         // Ořízni

            if (rawData.StartsWith("?"))                          // Pokud začíná „?“
            {
                rawData = rawData.Substring(1);                   // Odstraň „?“
            }

            string[] sensorEntries = rawData.Split(',');          // Rozděl na položky dle čárky

            StringBuilder result = new StringBuilder();           // StringBuilder pro multiřádkový výstup

            foreach (string entry in sensorEntries)               // Pro každou položku „id:typ“
            {
                string[] parts = entry.Split(':');                // Rozděl na id a typ
                if (parts.Length == 2)                            // Očekáváme přesně 2 části
                {
                    string id = parts[0];                         // id
                    string type = parts[1];                       // typ
                    result.AppendLine($"{type} ({id})");          // Přidej řádek „typ (id)“
                }
            }

            AktivBox.Text = result.ToString();                    // Zapiš do textBoxu AktivBox
        }

        private void AppendTextBox(string text)                   // Thread-safe append do textBox2
        {
            if (textBox2.InvokeRequired)                          // Jsme mimo UI vlákno?
            {
                textBox2.Invoke(new Action(() =>                  // Přepošli na UI vlákno
                {
                    textBox2.AppendText(text);                    // Přidej text
                }));
            }
            else                                                  // Jsme na UI vlákně
            {
                textBox2.AppendText(text);                        // Přidej text přímo
            }
        }

        private void ResetChart()                                 // Reset grafu (při změně ID)
        {
            sampleCount = 0;                                      // Nuluj počitadlo
            chart1.Series.Clear();                                // Odeber všechny série
            InitializeChart();                                    // Vytvoř výchozí sérii znovu
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) // Při zavírání formuláře
        {
            SerialManager.Instance.Close();                       // Zavři sériový port
        }

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

                SenzoryData = data;                                            // Ulož načtený seznam do pole třídy (budeme s tím dál pracovat)

                sensorIdMap.Clear();                                              // Vyčisti mapu „Znackeni → Id“ (aby tam nezůstaly staré hodnoty)
                comboBoxSensor.BeginUpdate();                                     // Optimalizace vykreslování při hromadném plnění comboboxu
                comboBoxSensor.Items.Clear();                                     // Vyprázdni položky comboboxu

                foreach (var k in SenzoryData)                                 // Projdi všechny prvky z JSONu (každý „senzor“)
                {                                                                 // Začátek foreach
                    string label = (k.Znaceni ?? string.Empty).Trim();           // Vytáhni text „Znackeni“ (název/štítek do UI), ošetři null a ořízni
                    if (string.IsNullOrWhiteSpace(label))                         // Když je prázdný/whitespace,
                        continue;                                                 // ...tuhle položku přeskoč

                    if (!sensorIdMap.ContainsKey(label))                          // Je to nový label? (ještě není v mapě)
                        comboBoxSensor.Items.Add(label);                          // ...tak ho přidej do comboboxu (aby šel vybrat)

                    sensorIdMap[label] = k.Id.ToString();                         // Do mapy ulož dvojici Label → Id (Id převedeme na string)
                                                                                  // POZN: Pokud máš v modelu Id už jako "Sxx" string, napiš prostě: sensorIdMap[label] = k.Id;
                }                                                                 // Konec foreach

                comboBoxSensor.EndUpdate();                                       // Ukonči hromadnou aktualizaci (UI jednorázově překreslí změny)
                comboBoxSensor.SelectedIndex = -1;                                // Nic nepředvybírej (uživatel si vybere sám)
                UpdateRequestFromUi();                                            // Přepočítej náhled požadavku (label8, tlačítko Start apod.)
            }                                                                     // Konec try
            catch (Exception ex)                                                  // Zachytíme libovolnou výjimku (soubor, parsování…)
            {                                                                     // Začátek catch
                MessageBox.Show("Chyba při načítání Senzory.json: " + ex.Message);// Ukaž chybovou hlášku s důvodem
            }                                                                     // Konec catch
        }                                                                         // Konec metody


        private void DisplayTimer_Tick(object sender, EventArgs e)
        {
            string chunk;
            lock (_rxLock)
            {
                if (_rxBuffer.Length == 0) return;
                chunk = _rxBuffer.ToString();
                _rxBuffer.Clear();
            }

            // Normalizace EOL
            chunk = chunk.Replace("\r", "");
            var lines = chunk.Split('\n');

            foreach (var raw in lines)
            {
                if (string.IsNullOrEmpty(raw)) continue;

                // Odstranit BOM a neviditelné kontrolní znaky na začátku řádku
                var line = raw.Trim();
                line = line.TrimStart('\uFEFF'); // <— DŮLEŽITÉ (BOM)
                line = new string(line.Where(ch => !char.IsControl(ch) || ch == '?' || ch == '=' || ch == '&' || ch == '.' || ch == ',' || ch == '-' || char.IsLetterOrDigit(ch)).ToArray());

                if (string.IsNullOrEmpty(line)) continue;

                if (line.StartsWith("?id=", StringComparison.OrdinalIgnoreCase))
                {
                    ParseAndDisplayData(line);
                    continue;
                }

                if (LooksLikeInitList(line))
                {
                    ParseInitMessage(line);
                    continue;
                }

                AppendTextBox(line + "\r\n");
            }

            // jistota překreslení grafu po přidání bodů
            chart1.Invalidate();
        }



        private static bool LooksLikeInitList(string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return false;
                if (s.StartsWith("?")) return false;
                // hodně jednoduchý heuristický check pro "id:type,id:type"
                return s.Contains(":") && s.Contains(",");
            }


        private void comboBoxSensor_SelectedIndexChanged(object sender, EventArgs e) // Při změně senzoru načti jeho obrázek
        {
            try
            {
                string label = comboBoxSensor.SelectedItem as string; // Vybraný label (Znackeni)
                if (string.IsNullOrWhiteSpace(label)) return;     // Bez labelu → konec

                string baseDir = Directory.GetParent(Application.StartupPath).Parent.Parent.FullName; // Základní složka projektu
                string sensorsDir = Path.Combine(baseDir, "Senzory"); // Složka s obrázky senzorů

                string[] exts = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" }; // Povolené přípony

                string foundPath = null;                          // Sem uložíme první nalezený obrázek
                foreach (var ext in exts)                         // Zkus všechny přípony
                {
                    string p = Path.Combine(sensorsDir, label + ext); // Kandidátní cesta
                    if (File.Exists(p))                           // Existuje?
                    {
                        foundPath = p;                            // Ulož cestu
                        break;                                    // A konči hledání
                    }
                }

                if (foundPath == null)                            // Nenašli jsme obrázek?
                {
                    pictureBox1.Image = null;                     // Vymaž případný starý obrázek
                    AppendTextBox($"Nenalezen obrázek pro „{label}“ ve složce {sensorsDir}.\r\n"); // Logni info
                    return;                                       // A skonči
                }

                using (var fs = new FileStream(foundPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) // Otevři soubor
                {
                    var img = Image.FromStream(fs);               // Načti obrázek ze streamu
                    pictureBox1.Image = (Image)img.Clone();       // Naklonuj (aby šel později uvolnit soubor)
                }
            }
            catch (Exception ex)                                  // Při chybě
            {
                MessageBox.Show($"Chyba při načítání obrázku: {ex.Message}"); // Zobraz chybovou hlášku
            }
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }
    }
}
