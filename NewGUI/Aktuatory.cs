using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace NewGUI
{
    public partial class Aktuatory : UserControl
    {
        private Timer comPortWatcherTimer;                         // Kontrola přítomnosti COM zařízení
        private List<string> lastKnownPorts = new List<string>();
        private string BasePath = Directory.GetParent(Application.StartupPath).Parent.Parent.FullName;
        private Timer delayedSendTimer;                            // Timer pro jednorázové zpožděné odeslání

        // --- NOVĚ: JSON datový model místo CSV DataTable ---
        private List<Komponenty> aktuatoryData; // Načtené položky z aktuatory.json
                                               

        public Aktuatory(Form1 rodic)
        {
            InitializeComponent();
            LoadJsonData();                                        // ⟵ místo LoadCsvData()

            // Kontrola COM portů
            comPortWatcherTimer = new Timer();
            comPortWatcherTimer.Interval = 500;
            comPortWatcherTimer.Tick += ComPortWatcherTimer_Tick;
            comPortWatcherTimer.Start();

            // skrytí textboxů a labelů
            textBox1.Visible = false;
            textBox2.Visible = false;
            textBox3.Visible = false;

            label1.Visible = false;
            label2.Visible = false;
            label3.Visible = false;

            // Timer pro jednorázové odložené odeslání
            delayedSendTimer = new Timer();
            delayedSendTimer.Interval = 1000; // 1 s
            delayedSendTimer.Tick += DelayedSendTimer_Tick;

            // aby se UI pinů přepínalo při změně módu/aktuátoru/vstupu
            ModBox.SelectedIndexChanged += (s, e) => { UpdatePinInputsUi_Actuators(); UpdateStartEnabled_Actuators(); };
            AktBox.SelectedIndexChanged += (s, e) => { UpdatePinInputsUi_Actuators(); UpdateStartEnabled_Actuators(); };

            textBox1.TextChanged += (s, e) => UpdateStartEnabled_Actuators();
            textBox2.TextChanged += (s, e) => UpdateStartEnabled_Actuators();
            textBox3.TextChanged += (s, e) => UpdateStartEnabled_Actuators();
            textBox4.TextChanged += (s, e) => UpdateStartEnabled_Actuators();


            UpdatePinInputsUi_Actuators();
            UpdateStartEnabled_Actuators();


            // Default zobrazení
            SetControlButtonsEnabled(false);
        }

        // ---------- COM PORT WATCHER ----------
        private void ComPortWatcherTimer_Tick(object sender, EventArgs e)
        {
            var currentPorts = SerialPort.GetPortNames().ToList();
            if (!currentPorts.SequenceEqual(lastKnownPorts))
            {
                string selected = ComBox.SelectedItem as string;
                ComBox.Items.Clear();
                ComBox.Items.AddRange(currentPorts.ToArray());

                if (selected != null && currentPorts.Contains(selected))
                {
                    ComBox.SelectedItem = selected;
                }
                else if (currentPorts.Count > 0)
                {
                    ComBox.SelectedIndex = 0;
                }
                lastKnownPorts = currentPorts;
            }
        }
        // ---------- NOVĚ: NAČTENÍ JSON MÍSTO CSV ----------
        private void LoadJsonData()
        {
            try
            {
                string jsonPath = Path.Combine(Application.StartupPath,"Aktuatory.json");
                if (!File.Exists(jsonPath))
                {
                    MessageBox.Show($"Soubor aktuatory.json nebyl nalezen ve složce projektu: {BasePath}");
                    return;
                }

                string jsonText = File.ReadAllText(jsonPath);

                // Deserializace do List<Komponenty>
                aktuatoryData = JsonSerializer.Deserialize<List<Komponenty>>(jsonText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                }) ?? new List<Komponenty>();

                AktBox.Items.Clear();
                foreach (var a in aktuatoryData)
                {
                    if (!string.IsNullOrWhiteSpace(a.Alias))
                        AktBox.Items.Add(a.Alias);
                }

                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                AktBox.SelectedIndexChanged -= AktBox_UpdateImage;
                AktBox.TextChanged -= AktBox_UpdateImage;
                AktBox.SelectedIndexChanged += AktBox_UpdateImage;
                AktBox.TextChanged += AktBox_UpdateImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při načítání JSON: {ex.Message}");
            }
        }



        // Získá „zobrazovací alias“ – priorita: Alias (type) → Alias → Znackeni
        private static string GetDisplayAlias(Komponenty a)
        {
            return (a.Alias ?? string.Empty).Trim();
        }


        // Najde položku v JSONu podle jména zvoleného v AktBox (porovnává display alias)
        private Komponenty FindByDisplayAlias(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias) || aktuatoryData == null)
                return null;

            return aktuatoryData.FirstOrDefault(a =>
                string.Equals(a.Alias, alias, StringComparison.OrdinalIgnoreCase));
        }




        // ---------- OBRÁZEK PODLE VÝBĚRU ----------
        private void AktBox_UpdateImage(object sender, EventArgs e)
        {
            var path = Path.Combine(BasePath, "Aktuátory", $"{AktBox.Text}.png");

            if (!File.Exists(path))
            {
                pictureBox1.Image?.Dispose();
                pictureBox1.Image = null;
                return;
            }

            try
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var img = Image.FromStream(fs, useEmbeddedColorManagement: true, validateImageData: true))
                {
                    pictureBox1.Image = new Bitmap(img);
                }
            }
            catch
            {
                // ticho
            }
        }

        // Při zavření (pokud tuhle událost někde připojuješ)
        private void Aktuator_Closing(object sender, FormClosedEventArgs e)
        {
            SerialManager.Instance.Close();
        }

        // ---------- PŘIPOJENÍ/ODPOJENÍ ----------
        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!SerialManager.Instance.IsOpen)
            {
                if (ComBox.SelectedItem == null)
                {
                    MessageBox.Show("Vyberte COM port.");
                    return;
                }

                try
                {
                    SerialManager.Instance.ConfigurePort(
                        portName: ComBox.SelectedItem.ToString(),
                        baudRate: 115200,
                        parity: Parity.None,
                        dataBits: 8,
                        stopBits: StopBits.One,
                        handshake: Handshake.None,
                        newLine: "\n"
                    );

                    // V Aktuátorech RX nepotřebujeme – případně:
                    // SerialManager.Instance.AttachExclusiveReceiver(Aktuatory_DataReceived);

                    SerialManager.Instance.Open();

                    btnConnect.Text = "Odpojit";
                    SetControlButtonsEnabled(true);

                    badgeConn.Text = "Připojeno";
                    badgeConn.BackColor = Color.FromArgb(46, 125, 50);
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
                    if (delayedSendTimer.Enabled) delayedSendTimer.Stop(); // zruš odložené odeslání
                    SerialManager.Instance.Close();
                }
                finally
                {
                    btnConnect.Text = "Připojit";
                    badgeConn.Text = "Nepřipojeno";
                    badgeConn.BackColor = Color.FromArgb(107, 114, 128);

                    SetControlButtonsEnabled(false);
                }
            }
        }

        // ---------- UI pro CONFIG parametry ----------
        private void ShowTextBoxesForRequest(string request)
        {
            // reset UI
            textBox1.Visible = textBox2.Visible = textBox3.Visible = false;
            label1.Visible = label2.Visible = label3.Visible = false;
            textBox1.Text = textBox2.Text = textBox3.Text = string.Empty;

            // vše za "id="
            var match = Regex.Match(request, @"\bid=[^&]*&(.+)");
            if (!match.Success) return;

            var paramString = match.Groups[1].Value;
            var parameters = paramString.Split('&');

            for (int i = 0; i < parameters.Length && i < 3; i++)
            {
                var kv = parameters[i].Split(new[] { '=' }, 2);
                if (kv.Length < 1) continue;

                string key = kv[0].Trim();
                string val = kv.Length > 1 ? kv[1].Trim() : "";

                if (i == 0)
                {
                    label1.Text = key; label1.Visible = true;
                    textBox1.Text = val; textBox1.Visible = true;
                }
                else if (i == 1)
                {
                    label2.Text = key; label2.Visible = true;
                    textBox2.Text = val; textBox2.Visible = true;
                }
                else if (i == 2)
                {
                    label3.Text = key; label3.Visible = true;
                    textBox3.Text = val; textBox3.Visible = true;
                }
            }
            UpdateStartEnabled_Actuators();
        }


        // ---------- Dosazení hodnot z textboxů do requestu ----------
        private string UpdateRequestWithTextBoxValues(string originalRequest)
        {
            var pattern = @"(?<key>[^&=?]+)=(?<value>[^&]*)";
            var matches = Regex.Matches(originalRequest, pattern);

            var dict = new Dictionary<string, string>();
            foreach (Match match in matches)
                dict[match.Groups["key"].Value] = match.Groups["value"].Value;

            if (textBox1.Visible && label1.Visible)
                dict[label1.Text] = textBox1.Text;
            if (textBox2.Visible && label2.Visible)
                dict[label2.Text] = textBox2.Text;
            if (textBox3.Visible && label3.Visible)
                dict[label3.Text] = textBox3.Text;

            string basePart = originalRequest.Split('?')[0];
            string typeAndId = Regex.Match(originalRequest, @"\?[^&]+&[^&]+").Value;

            var newParams = dict
                .Where(kvp => !typeAndId.Contains($"{kvp.Key}="))
                .Select(kvp => $"{kvp.Key}={kvp.Value}");

            return basePart + typeAndId + (newParams.Any() ? "&" + string.Join("&", newParams) : "");
        }

        // ---------- START / STOP ----------
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (btnStart.Text == "Spustit")
            {
                if (!SerialManager.Instance.IsOpen)
                {
                    MessageBox.Show("Nejste připojen k žádnému COM portu.");
                    return;
                }

                string selectedMod = ModBox.SelectedItem?.ToString();
                string selectedAlias = AktBox.SelectedItem?.ToString();


                // --- CONNECT / DISCONNECT (one-shot) ---
                if (string.Equals(selectedMod, "CONNECT", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(selectedMod, "DISCONNECT", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(selectedAlias))
                    {
                        MessageBox.Show("Vyberte aktuátor.");
                        return;
                    }

                    var item = FindByDisplayAlias(selectedAlias);
                    if (item == null)
                    {
                        MessageBox.Show("Alias nebyl nalezen v JSONu.");
                        return;
                    }

                    // ID pro request – ideálně přímo z objektu (pokud máš string Id), jinak fallback z Request_CONFIG
                    string idValue = item.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    if (string.IsNullOrWhiteSpace(idValue))
                    {
                        var m = Regex.Match(item.Request_CONFIG ?? string.Empty, @"\bid=([^&]+)");
                        idValue = m.Success ? m.Groups[1].Value : null;
                    }
                    if (string.IsNullOrWhiteSpace(idValue))
                    {
                        MessageBox.Show("Nelze zjistit ID aktuátoru.");
                        return;
                    }

                    var pinQuery = BuildPinQueryActuator4(item);
                    if (string.IsNullOrWhiteSpace(pinQuery))
                    {
                        MessageBox.Show("Doplňte požadované piny.");
                        return;
                    }

                    // POZOR: formát s čárkami mezi pinX=... částmi (přesně jak požaduješ)
                    string request = $"?type={selectedMod.ToUpperInvariant()}&id={idValue}&{pinQuery}";

                    try
                    {
                        SerialManager.Instance.WriteLine(request);
                        MainTextBox.Clear();
                        MainTextBox.AppendText(request + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Chyba při odesílání: {ex.Message}");
                    }

                    // one-shot: NEpřepínáme tlačítko na „Zastavit“
                    return;
                }



                // CONFIG/UPDATE apod.
                if (string.IsNullOrEmpty(selectedAlias))
                {
                    MessageBox.Show("Vyberte aktuátor.");
                    return;
                }

                var it = FindByDisplayAlias(selectedAlias);
                if (it == null)
                {
                    MessageBox.Show("Alias nebyl nalezen v JSONu.");
                    return;
                }

                string requestOriginal = it.Request_CONFIG;
                if (string.IsNullOrWhiteSpace(requestOriginal))
                {
                    MessageBox.Show("V JSONu chybí Request pro vybraný aktuátor.");
                    return;
                }

                // Přepiš pouze type=..., id zůstává z JSONu
                string requestFinal = Regex.Replace(requestOriginal, @"type=[^&]+", $"type={selectedMod}");
                requestFinal = UpdateRequestWithTextBoxValues(requestFinal);

                try
                {
                    SerialManager.Instance.WriteLine(requestFinal);
                    MainTextBox.Clear();
                    MainTextBox.AppendText(requestFinal + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Chyba při odesílání: {ex.Message}");
                }

                btnStart.Text = "Zastavit";
                btnStart.ForeColor = Color.White;
                toolTip1.SetToolTip(this.btnStart, "Okamžitě zastaví běžící akci");
                btnStart.FlatAppearance.MouseDownBackColor = Color.FromArgb(183, 28, 28);
                btnStart.FlatAppearance.MouseOverBackColor = Color.FromArgb(153, 0, 0);
                btnStart.BackColor = Color.FromArgb(211, 47, 47);
                btnStart.FlatAppearance.BorderColor = Color.FromArgb(211, 47, 47);
            }
            else
            {
                btnStart.Text = "Spustit";
                this.btnStart.BackColor = Color.FromArgb(15, 108, 189);
                this.btnStart.FlatAppearance.BorderColor = Color.FromArgb(15, 108, 189);
                this.btnStart.FlatAppearance.MouseDownBackColor = Color.FromArgb(17, 94, 163);
                this.btnStart.FlatAppearance.MouseOverBackColor = Color.FromArgb(12, 83, 146);

                if (delayedSendTimer.Enabled)
                {
                    delayedSendTimer.Stop();
                    MainTextBox.AppendText("Odeslání bylo zrušeno tlačítkem STOP.\r\n");
                }
            }
            UpdateStartEnabled_Actuators();

        }


        // ---------- ODLOŽENÉ ODESLÁNÍ ----------
        private void DelayedSendTimer_Tick(object sender, EventArgs e)
        {
            delayedSendTimer.Stop(); // jednorázové odeslání

            if (!SerialManager.Instance.IsOpen)
                return;

            string selectedAlias = AktBox.SelectedItem?.ToString();
            string selectedMod = ModBox.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(selectedAlias) || string.IsNullOrEmpty(selectedMod))
                return;

            var item = FindByDisplayAlias(selectedAlias);
            if (item == null)
                return;

            string request = item.Request_CONFIG;
            if (string.IsNullOrWhiteSpace(request))
                return;

            request = Regex.Replace(request, @"type=[^&]+", $"type={selectedMod}");
            request = UpdateRequestWithTextBoxValues(request);

            try
            {
                SerialManager.Instance.WriteLine(request);

                MainTextBox.Clear();
                MainTextBox.AppendText($"Odesláno po zpoždění: {request}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                MainTextBox.Clear();
                MainTextBox.AppendText($"Chyba při odesílání: {ex.Message}{Environment.NewLine}");
            }
        }

        private void ModBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // reset viditelnosti – přepneme dle módu
            textBox1.Visible = textBox2.Visible = textBox3.Visible = textBox4.Visible = false;
            label1.Visible = label2.Visible = label3.Visible = label4.Visible = false;

            UpdatePinInputsUi_Actuators();
            UpdateStartEnabled_Actuators();
        }





        private void AktBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AktBox.SelectedItem == null || aktuatoryData == null)
                return;

            string selectedAlias = AktBox.SelectedItem.ToString();

            var item = FindByDisplayAlias(selectedAlias);
            if (item == null) return;

            string request = item.Request_CONFIG;
            if (!string.IsNullOrWhiteSpace(request) && ModBox.Text == "CONFIG")
            {
                ShowTextBoxesForRequest(request);
            }
        }

        private void SetControlButtonsEnabled(bool enabled)
        {
            btnStart.Enabled = enabled;
            ModBox.Enabled = enabled;
            AktBox.Enabled = enabled;
            textBox1.Enabled = enabled;
            textBox2.Enabled = enabled;
            textBox3.Enabled = enabled;
            textBox4.Enabled = enabled; 
        }

        // Najde vybraný aktuátor podle AktBox
        // Vyhledá vybraný aktuátor
        private Komponenty FindSelectedActuator()
        {
            var alias = AktBox.SelectedItem?.ToString();
            return FindByDisplayAlias(alias);
        }

        // Normalizace vstupu pinu (stejně jako u senzorů)
        private static string NormalizePinInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            input = input.Trim();
            var digits = new string(input.Where(char.IsDigit).ToArray());
            return string.IsNullOrEmpty(digits) ? input : digits;
        }

        // Sestaví výraz pro &pin=... podle požadavků a JSONu
        private string BuildPinExprActuator()
        {
            var item = FindSelectedActuator();
            if (item == null) return null;

            var p1 = NormalizePinInput(textBox1.Text);
            var hasSecond = !string.IsNullOrWhiteSpace(item.PIN2);
            var p2 = NormalizePinInput(textBox2.Text);

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


        // Sestaví "pin1=...,pin2=...,pin3=...,pin4=..." podle JSONu a textboxů.
        // Vrací null, pokud je vyžadovaný pin nevyplněn.
        private string BuildPinQueryActuator4(Komponenty item)
        {
            if (item == null) return null;

            // Podle JSONu zjistíme, kolik pinů daný aktuátor má
            bool has1 = !string.IsNullOrWhiteSpace(item.PIN1);
            bool has2 = !string.IsNullOrWhiteSpace(item.PIN2);
            bool has3 = !string.IsNullOrWhiteSpace(item.PIN3);
            bool has4 = !string.IsNullOrWhiteSpace(item.PIN4);

            string p1 = has1 ? NormalizePinInput(textBox1.Text) : null;
            string p2 = has2 ? NormalizePinInput(textBox2.Text) : null;
            string p3 = has3 ? NormalizePinInput(textBox3.Text) : null;
            string p4 = has4 ? NormalizePinInput(textBox4.Text) : null;

            if (has1 && string.IsNullOrWhiteSpace(p1)) return null;
            if (has2 && string.IsNullOrWhiteSpace(p2)) return null;
            if (has3 && string.IsNullOrWhiteSpace(p3)) return null;
            if (has4 && string.IsNullOrWhiteSpace(p4)) return null;

            var parts = new List<string>();
            if (has1) parts.Add($"pin1={p1}");
            if (has2) parts.Add($"pin2={p2}");
            if (has3) parts.Add($"pin3={p3}");
            if (has4) parts.Add($"pin4={p4}");

            // POZOR: požadoval jsi čárky mezi položkami
            return string.Join(",", parts);
        }

        // Přepíná UI pro CONNECT/DISCONNECT (zobrazení až 4 pinů podle JSONu)
        private void UpdatePinInputsUi_Actuators()
        {
            // výchozí skrytí
            textBox1.Visible = textBox2.Visible = textBox3.Visible = textBox4.Visible = false;
            label1.Visible = label2.Visible = label3.Visible = label4.Visible = false;

            var mode = ModBox.Text?.Trim();

            // CONFIG – ponecháváš existující ShowTextBoxesForRequest
            if (string.Equals(mode, "CONFIG", StringComparison.OrdinalIgnoreCase))
            {
                var itCfg = FindSelectedActuator();
                if (itCfg != null && !string.IsNullOrWhiteSpace(itCfg.Request_CONFIG))
                    ShowTextBoxesForRequest(itCfg.Request_CONFIG);
                return;
            }

            // CONNECT/DISCONNECT – ukaž piny dle JSONu
            bool isConn = string.Equals(mode, "CONNECT", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(mode, "DISCONNECT", StringComparison.OrdinalIgnoreCase);

            if (!isConn) return;

            var item = FindSelectedActuator();
            if (item == null) return;

            if (!string.IsNullOrWhiteSpace(item.PIN1))
            {
                label1.Text = item.PIN1;
                label1.Visible = true; textBox1.Visible = true;
            }
            if (!string.IsNullOrWhiteSpace(item.PIN2))
            {
                label2.Text = item.PIN2;
                label2.Visible = true; textBox2.Visible = true;
            }
            if (!string.IsNullOrWhiteSpace(item.PIN3))
            {
                label3.Text = item.PIN3;
                label3.Visible = true; textBox3.Visible = true;
            }
            if (!string.IsNullOrWhiteSpace(item.PIN4))
            {
                label4.Text = item.PIN4;
                label4.Visible = true; textBox4.Visible = true;
            }
        }

        // Povolení Start – hlídá vyžadované piny 1–4
        private void UpdateStartEnabled_Actuators()
        {
            bool connected = SerialManager.Instance.IsOpen;
            bool hasMode = !string.IsNullOrWhiteSpace(ModBox.Text);
            bool hasAct = AktBox.SelectedItem != null;

            bool ready = connected && hasMode;
            string m = ModBox.Text?.Trim() ?? string.Empty;

            if (m.Equals("CONNECT", StringComparison.OrdinalIgnoreCase) ||
                m.Equals("DISCONNECT", StringComparison.OrdinalIgnoreCase))
            {
                var item = FindSelectedActuator();
                if (item == null) { btnStart.Enabled = false; return; }

                bool need1 = !string.IsNullOrWhiteSpace(item.PIN1);
                bool need2 = !string.IsNullOrWhiteSpace(item.PIN2);
                bool need3 = !string.IsNullOrWhiteSpace(item.PIN3);
                bool need4 = !string.IsNullOrWhiteSpace(item.PIN4);

                bool p1ok = !need1 || !string.IsNullOrWhiteSpace(NormalizePinInput(textBox1.Text));
                bool p2ok = !need2 || !string.IsNullOrWhiteSpace(NormalizePinInput(textBox2.Text));
                bool p3ok = !need3 || !string.IsNullOrWhiteSpace(NormalizePinInput(textBox3.Text));
                bool p4ok = !need4 || !string.IsNullOrWhiteSpace(NormalizePinInput(textBox4.Text));

                ready = ready && hasAct && p1ok && p2ok && p3ok && p4ok;
            }
            else if (m.Equals("CONFIG", StringComparison.OrdinalIgnoreCase))
            {
                // všechny viditelné textboxy musí být vyplněné
                bool t1ok = !textBox1.Visible || !string.IsNullOrWhiteSpace(textBox1.Text?.Trim());
                bool t2ok = !textBox2.Visible || !string.IsNullOrWhiteSpace(textBox2.Text?.Trim());
                bool t3ok = !textBox3.Visible || !string.IsNullOrWhiteSpace(textBox3.Text?.Trim());
                bool t4ok = !textBox4.Visible || !string.IsNullOrWhiteSpace(textBox4.Text?.Trim());

                ready = ready && hasAct && t1ok && t2ok && t3ok && t4ok;
            }
            else
            {
                ready = ready && hasAct;
            }

            btnStart.Enabled = ready;
        }




    }
}
