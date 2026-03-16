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
            ScrollBars = ScrollBars.Both,
            Dock = DockStyle.Fill,
            WordWrap = false,
            TabStop = false,
            BorderStyle = BorderStyle.None,
            ShortcutsEnabled = true,
            HideSelection = false
        };

        public SerialPopupForm(string title = "Sériový výpis")
        {
            Text = title;
            Width = 300;
            Height = 400;
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.None;
            ControlBox = false;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            Text = string.Empty;
            Controls.Add(Output);

            // Necháme textbox interaktivní (výběr + scroll), ale zakážeme editaci (ReadOnly)
            // a minimalizujeme "caret" chování.
            Output.GotFocus += (s, e) =>
            {
                try { Output.SelectionLength = 0; } catch { }
            };
            Output.MouseDown += (s, e) =>
            {
                // při prvním kliku zruš caret selection (uživatel pak může tahat myší pro výběr)
                if (e.Button == MouseButtons.Left)
                {
                    // nic nedělej; necháme standardní select+drag
                }
            };

            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    e.Handled = true;
                    this.Hide();
                }
            };

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
            try
            {
                Output.SelectionLength = 0;
                Output.SelectionStart = Output.TextLength;
            }
            catch { }
        }
    }
}
