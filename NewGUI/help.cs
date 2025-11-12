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
        private const string Dokumentace = "Dokumentace_senzory_EduBox.pdf";
        private const string Navod = "Dokumentace_Aplikace.pdf";

        public help(Form1 rodic)
        {
            InitializeComponent();
            _rodic = rodic;

            // Vytvoříme dlaždice (Tlacitka) přes původní tlačítka a zachytíme jejich aktivaci.
            try
            {
                AddTile(Document_panel,
                    title: "Dokumentace",
                    normal: Properties.Resources.half_brain_mini3,
                    hover: Properties.Resources.half_brain_mini4,
                    detail: "Dokumentace shrnuje podrobné informace o všech senzorech a aktuátorech systému Edubox – jejich zapojení, funkce, komunikační protokoly, konfiguraci i technické parametry.",
                    onActivate: (s, e) => Document_button_click(s,e));

                AddTile(Popis_panel,
                    title: "Návod k Aplikaci",
                    normal: Properties.Resources.half_brain_mini3,
                    hover: Properties.Resources.half_brain_mini4,
                    detail: "Dokumentace k aplikaci popisuje ovládací prvky, funkce jednotlivých tlačítek a postupy práce v uživatelském rozhraní systému.",
                    onActivate: (s, e) => Popis_button_Click(s,e));
            }
            catch (Exception ex)
            {
                // Pokud něco selže (např. chybějící resource), při ladění zobraďte chybu:
                // MessageBox.Show("Chyba při vytváření dlaždic: " + ex.Message);
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
                ExpandedHeight = 150 // výšku můžeš doladit
            };
            tile.Activated += onActivate;

            host.Controls.Add(tile);
        }


        private void Document_button_click(object sender, EventArgs e)
        {
            _rodic.NahraditObsah(new Documentation(_rodic));
            // Vypočítá cestu stejně jako dřív
            var pdfPath = ResolvePdfPath(Dokumentace);

            if (!File.Exists(pdfPath))
            {
                MessageBox.Show("Soubor nebyl nalezen:\n" + pdfPath);
                return;
            }

            try
            {
                // Otevře PDF ve výchozím programu (prohlížeč, Adobe, Edge, …)
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

        // Tvoje původní cesta zachována
        private static string ResolvePdfPath(string Cesta)
        {
            var basePath = Directory.GetParent(Directory.GetParent(Directory.GetParent(Application.StartupPath).FullName).FullName).FullName;
            // Pokud PDF neleží v "Docs", tu část můžeš odstranit
            return Path.Combine(basePath, Cesta);
        }

        private void Popis_button_Click(object sender, EventArgs e)
        {
            _rodic.NahraditObsah(new Documentation(_rodic));
            // Vypočítá cestu stejně jako dřív
            var pdfPath = ResolvePdfPath(Navod);

            if (!File.Exists(pdfPath))
            {
                MessageBox.Show("Soubor nebyl nalezen:\n" + pdfPath);
                return;
            }

            try
            {
                // Otevře PDF ve výchozím programu (prohlížeč, Adobe, Edge, …)
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
