using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;


namespace NewGUI
{
    public partial class help : UserControl
    
    {
        private Form1 _rodic;
        private const string PdfFileNameOnly = "Dokumentace_senzory_EduBox.pdf";
        public help(Form1 rodic)
        {
            InitializeComponent();
            _rodic = rodic;

        }


        private void Document_button_click(object sender, EventArgs e)
        {
            _rodic.NahraditObsah(new Documentation(_rodic));
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
            var basePath = Directory.GetParent(Directory.GetParent(Directory.GetParent(Application.StartupPath).FullName).FullName).FullName;
            // Pokud PDF neleží v "Docs", tu část můžeš odstranit
            return Path.Combine(basePath,PdfFileNameOnly);
        }

    }
}
