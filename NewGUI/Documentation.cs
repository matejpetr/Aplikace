// HelpForm.cs
using System;
using System.IO;
using System.Windows.Forms;
using PdfiumViewer;

namespace NewGUI
{
    public partial class Documentation : UserControl
    {
        private PdfViewer _viewer;
        private PdfDocument _doc;
        public Documentation(Form1 rodic)
        {
            InitializeComponent();

            _viewer = new PdfViewer { Dock = DockStyle.Fill };
            Controls.Add(_viewer);

            if (!File.Exists(pdfPath))
            {
                MessageBox.Show("Dokumentace nenalezena: " + pdfPath);
                Close();
                return;
            }

            _doc = PdfDocument.Load(pdfPath);  // umí i Stream, kdybys chtěl embed resource
            _viewer.Document = _doc;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _viewer?.Dispose();
            _doc?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
