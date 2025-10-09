using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NewGUI
{
    public partial class Aktuatory : UserControl
    {
        private Timer comPortWatcherTimer;                 // Kontrola přítomnosti COM zařízení
        private DataTable excelTable;                      // CSV tabulka
        private List<string> lastKnownPorts = new List<string>();
        private string BasePath = Directory.GetParent(Application.StartupPath).Parent.Parent.FullName;
        private Timer delayedSendTimer;                    // Timer pro jednorázové zpožděné odeslání

        public Aktuatory(Form1 rodic)
        {
            InitializeComponent();
            LoadCsvData();

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

            // Default checkbox
            checkBox1.Visible = false;
            checkBox1.Checked = false;

            // Default zobrazení
            SetControlButtonsEnabled(false);
        }

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

        private void LoadCsvData()
        {
            try
            {
                string csvPath = Path.Combine(BasePath, "MTA_Aktuátory.csv");
                string PicPath = Path.Combine(BasePath, "Aktuátory");
                if (!File.Exists(csvPath))
                {
                    MessageBox.Show($"Soubor MTA_Aktuátory.csv nebyl nalezen ve složce projektu: {BasePath}");
                    return;
                }

                excelTable = new DataTable();
                AktBox.Items.Clear();
                int count = 0;

                try
                {
                    using (var reader = new StreamReader(csvPath, Encoding.Default))
                    {
                        bool headerRead = false;
                        string[] headers = null;

                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine()?.TrimEnd('\r', '\n');
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            // auto detekce oddělovače
                            string[] values;
                            if (line.Contains(';')) values = line.Split(';');
                            else if (line.Contains('\t')) values = line.Split('\t');
                            else values = line.Split(',');

                            if (!headerRead)
                            {
                                headers = values.Select(h => h.Trim().Trim('"')).ToArray();
                                foreach (var header in headers)
                                    excelTable.Columns.Add(header);
                                headerRead = true;
                                continue;
                            }

                            if (values.Length < headers.Length)
                                Array.Resize(ref values, headers.Length);

                            excelTable.Rows.Add(values);

                            // Alias: preferuj "Alias (type)", případně "Značení"
                            int aliasIdx = Array.FindIndex(headers, h =>
                                h.Equals("Alias (type)", StringComparison.OrdinalIgnoreCase) ||
                                h.Equals("Značení", StringComparison.OrdinalIgnoreCase));

                            if (aliasIdx >= 0)
                            {
                                string alias = values[aliasIdx]?.Trim().Trim('"');
                                if (!string.IsNullOrWhiteSpace(alias))
                                {
                                    AktBox.Items.Add(alias);
                                    count++;
                                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                                    AktBox.SelectedIndexChanged += AktBox_UpdateImage;
                                    AktBox.TextChanged += AktBox_UpdateImage;
                                }
                            }
                        }
                    }
                }
                catch (IOException)
                {
                    throw new ApplicationException("Soubor MTA_Aktuátory.csv je otevřený v jiném programu. Zavřete ho a zkuste to znovu.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při načítání CSV: {ex.Message}");
            }
        }

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
                    // Nastavení a otevření sdíleného seriáku
                    SerialManager.Instance.ConfigurePort(
                        portName: ComBox.SelectedItem.ToString(),
                        baudRate: 115200,
                        parity: Parity.None,
                        dataBits: 8,
                        stopBits: StopBits.One,
                        handshake: Handshake.None,
                        newLine: "\n"
                    );

                    // V Aktuátorech RX nepotřebujeme – pokud ano, můžeš připojit:
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
                    // zruš i případné odložené odeslání
                    if (delayedSendTimer.Enabled) delayedSendTimer.Stop();

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

        private void ShowTextBoxesForRequest(string request)
        {
            // Reset všech textboxů a labelů
            textBox1.Visible = false;
            textBox2.Visible = false;
            textBox3.Visible = false;

            label1.Visible = false;
            label2.Visible = false;
            label3.Visible = false;

            // parametry ZA "id="
            var match = Regex.Match(request, @"\bid=[^&]*&(.+)");
            if (!match.Success) return;

            var paramString = match.Groups[1].Value;
            var parameters = paramString.Split('&');

            for (int i = 0; i < parameters.Length && i < 3; i++)
            {
                string[] keyValue = parameters[i].Split('=');
                if (keyValue.Length < 2) continue;

                string key = keyValue[0].Trim();

                if (i == 0)
                {
                    label1.Text = key;
                    label1.Visible = true;
                    textBox1.Visible = true;
                }
                else if (i == 1)
                {
                    label2.Text = key;
                    label2.Visible = true;
                    textBox2.Visible = true;
                }
                else if (i == 2)
                {
                    label3.Text = key;
                    label3.Visible = true;
                    textBox3.Visible = true;
                }
            }
        }

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

                // RESET režim
                if (selectedMod == "RESET")
                {
                    // skryj textboxy
                    textBox1.Visible = false;
                    textBox2.Visible = false;
                    textBox3.Visible = false;
                    label1.Visible = false;
                    label2.Visible = false;
                    label3.Visible = false;

                    string idPart = checkBox1.Checked ? "*" : selectedAlias;
                    if (string.IsNullOrEmpty(idPart))
                    {
                        MessageBox.Show("Není vybrán aktuátor.");
                        return;
                    }

                    string request = $"?type=RESET&id={idPart}";

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

                    return; // jen RESET → konec
                }

                // CONFIG / UPDATE apod.
                if (string.IsNullOrEmpty(selectedAlias))
                {
                    MessageBox.Show("Vyberte aktuátor.");
                    return;
                }

                var row = excelTable.AsEnumerable()
                                    .FirstOrDefault(r => r.Table.Columns.Contains("Alias (type)")
                                                        ? r.Field<string>("Alias (type)") == selectedAlias
                                                        : (r.Table.Columns.Contains("Alias") && r.Field<string>("Alias") == selectedAlias));

                if (row == null)
                {
                    MessageBox.Show("Alias nebyl nalezen v CSV.");
                    return;
                }

                string requestOriginal = row.Field<string>("Request");
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
        }

        private void DelayedSendTimer_Tick(object sender, EventArgs e)
        {
            delayedSendTimer.Stop(); // jednorázové odeslání

            if (!SerialManager.Instance.IsOpen)
                return;

            string selectedAlias = AktBox.SelectedItem?.ToString();
            string selectedMod = ModBox.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(selectedAlias) || string.IsNullOrEmpty(selectedMod))
                return;

            var row = excelTable.AsEnumerable()
                                .FirstOrDefault(r => r.Table.Columns.Contains("Alias (type)")
                                                   ? r.Field<string>("Alias (type)") == selectedAlias
                                                   : (r.Table.Columns.Contains("Alias") && r.Field<string>("Alias") == selectedAlias));

            if (row == null)
                return;

            string request = row.Field<string>("Request");
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
            string selectedMod = ModBox.SelectedItem?.ToString();

            if (selectedMod == "RESET")
            {
                checkBox1.Visible = true;

                // schovat textboxy
                textBox1.Visible = false;
                textBox2.Visible = false;
                textBox3.Visible = false;

                label1.Visible = false;
                label2.Visible = false;
                label3.Visible = false;
            }
            else
            {
                checkBox1.Visible = false;
                checkBox1.Checked = false;
                AktBox.Enabled = true;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            AktBox.Enabled = !checkBox1.Checked;
        }

        private void AktBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AktBox.SelectedItem == null || excelTable == null)
                return;

            string selectedAlias = AktBox.SelectedItem.ToString();

            var row = excelTable.AsEnumerable()
                                .FirstOrDefault(r => r.Table.Columns.Contains("Alias (type)")
                                                   ? r.Field<string>("Alias (type)") == selectedAlias
                                                   : (r.Table.Columns.Contains("Alias") && r.Field<string>("Alias") == selectedAlias));

            if (row == null) return;

            string request = row.Field<string>("Request");
            if (ModBox.Text == "CONFIG")
            {
                ShowTextBoxesForRequest(request);
            }
        }

        private void SetControlButtonsEnabled(bool enabled)
        {
            btnStart.Enabled = enabled;
            ModBox.Enabled = enabled;
            AktBox.Enabled = enabled;
            checkBox1.Enabled = enabled;
            textBox1.Enabled = enabled;
            textBox2.Enabled = enabled;
            textBox3.Enabled = enabled;
        }

        // prázdné handlery – nechávám, pokud jsou navázány v Designeru
        private void badgeConn_Click(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
    }
}
