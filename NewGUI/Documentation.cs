using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace NewGUI
{
    public partial class Documentation : UserControl
    {
        private const string PdfFileNameOnly = "Dokumentace_senzory_EduBox.pdf";

        public Documentation(Form1 rodic)
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Vypočítá cestu stejně jako dřív
            var pdfPath = ResolvePdfPath();

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
        private static string ResolvePdfPath()
        {
            var basePath = Directory.GetParent(Directory.GetParent(Application.StartupPath).FullName).FullName;
            // Pokud PDF neleží v "Docs", tu část můžeš odstranit
            return Path.Combine(basePath, "Docs", PdfFileNameOnly);
        }
    }
}
