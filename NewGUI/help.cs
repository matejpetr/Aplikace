using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NewGUI
{
    public partial class help : UserControl
    {
        private Form1 _rodic;
        private const string Dokumentace = "Dokumentace_senzory.pdf";
        private const string Dokumentace2 = "Dokumentace_aktuatory.pdf";
        private const string Navod = "Dokumentace_Aplikace.pdf";

        public help(Form1 rodic)
        {
            InitializeComponent();
            _rodic = rodic;

            // Vytvoříme dlaždice (Tlacitka) přes původní tlačítka a zachytíme jejich aktivaci.
            try
            {
                AddTile(Document_panel,
                    title: "Dokumentace\n Senzory", // mírný posun druhé řádky doprava
                    normal: Properties.Resources.half_brain_mini3,
                    hover: Properties.Resources.half_brain_mini4,
                    detail: "Dokumentace shrnuje podrobné informace o všech senzorech systému Edubox – jejich zapojení, funkce, komunikační protokoly, konfiguraci i technické parametry.",
                    onActivate: (s, e) => Document_button_click(s,e));


                AddTile(Document2_panel,
                    title: "Dokumentace\nAktuátory",
                    normal: Properties.Resources.half_brain_mini3,
                    hover: Properties.Resources.half_brain_mini4,
                    detail: "Dokumentace shrnuje podrobné informace o všech aktuátorech systému Edubox – jejich zapojení, funkce, komunikační protokoly, konfiguraci i technické parametry.",
                    onActivate: (s, e) => Document2_button_Click(s, e));


                AddTile(Popis_panel,
                    title: "Manuál",
                    normal: Properties.Resources.half_brain_mini3,
                    hover: Properties.Resources.half_brain_mini4,
                    detail: "Dokumentace k aplikaci popisuje ovládací prvky, funkce jednotlivých tlačítek a postupy práce v uživatelském rozhraní systému.",
                    onActivate: (s, e) => Popis_button_Click(s,e));
            }
            catch (Exception ex)
            {
                // ladicí fallback
            }
        }

        public void AddTile(Panel host, string title, Image normal, Image hover, string detail, EventHandler onActivate)
        {
            host.Controls.Clear();

            var tile = new Tlacitka
            {
                Dock = DockStyle.Fill,
                Title = title,
                NormalImage = normal,
                HoverImage = hover,
                DetailText = detail,
                ExpandedHeight = 150
            };
            tile.Activated += onActivate;

            host.Controls.Add(tile);
        }


        private void Document_button_click(object sender, EventArgs e)
        {
            // Zůstat v panelu help – neprovádět navigaci
            var pdfPath = ResolvePdfPath(Dokumentace);

            if (!File.Exists(pdfPath))
            {
                MessageBox.Show("Soubor nebyl nalezen:\n" + pdfPath);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = pdfPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nepodařilo se otevřít PDF: " + ex.Message);
            }
        }

        private static string ResolvePdfPath(string Cesta)
        {
            var basePath = Directory.GetParent(Directory.GetParent(Directory.GetParent(Application.StartupPath).FullName).FullName).FullName;
            return Path.Combine(basePath, Cesta);
        }

        private void Popis_button_Click(object sender, EventArgs e)
        {
            // Zůstat v panelu help – neprovádět navigaci
            var pdfPath = ResolvePdfPath(Navod);

            if (!File.Exists(pdfPath))
            {
                MessageBox.Show("Soubor nebyl nalezen:\n" + pdfPath);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = pdfPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nepodařilo se otevřít PDF: " + ex.Message);
            }
        }

        private void Document2_button_Click(object sender, EventArgs e)
        {
            // Zůstat v panelu help – neprovádět navigaci
            var pdfPath = ResolvePdfPath(Dokumentace2);

            if (!File.Exists(pdfPath))
            {
                MessageBox.Show("Soubor nebyl nalezen:\n" + pdfPath);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = pdfPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nepodařilo se otevřít PDF: " + ex.Message);
            }

        }
    }
}
