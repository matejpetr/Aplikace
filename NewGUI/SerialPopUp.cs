using System;
using System.Drawing;
using System.Windows.Forms;

namespace NewGUI
{
    public sealed partial class SerialPopupForm : Form
    {
        public TextBox Output { get; } = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
            WordWrap = false,
            TabStop = false,          // ať se do něj neskáče tabem (spolu s ReadOnly to eliminuje blikání caret)
            BorderStyle = BorderStyle.None
        };

        public SerialPopupForm(string title = "Sériový výpis")
        {
            Text = title;
            Width = 600;
            Height = 400;
            StartPosition = FormStartPosition.Manual;

            Controls.Add(Output);

            // --- nastavení fontu: Cascadia Code 12, fallback na Consolas ---
            const string desired = "Cascadia Code";
            Output.Font = new Font(desired, 12f, FontStyle.Regular, GraphicsUnit.Point);
            if (!Output.Font.Name.Equals(desired, StringComparison.OrdinalIgnoreCase))
            {
                // fallback, když Cascadia Code není k dispozici
                Output.Font = new Font("Consolas", 12f, FontStyle.Regular, GraphicsUnit.Point);
            }
        }

        public void SetText(string text)
        {
            if (IsDisposed) return;
            if (InvokeRequired) { BeginInvoke(new Action<string>(SetText), text); return; }
            Output.SuspendLayout();
            Output.Text = text ?? string.Empty;
            Output.SelectionStart = Output.TextLength;
            Output.SelectionLength = 0;
            Output.ScrollToCaret();
            Output.ResumeLayout();
        }

        public void AppendLine(string line)
        {
            if (IsDisposed) return;
            if (InvokeRequired) { BeginInvoke(new Action<string>(AppendLine), line); return; }
            if (!string.IsNullOrEmpty(line))
            {
                if (!line.EndsWith("\r\n")) line += "\r\n";
                Output.AppendText(line);
                Output.SelectionStart = Output.TextLength;
                Output.SelectionLength = 0;
                Output.ScrollToCaret();
            }
        }

        // (volitelné) ještě jeden trik proti blikajícímu caret:
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Output.SelectionLength = 0;
            Output.SelectionStart = Output.TextLength;
        }
    }
}
