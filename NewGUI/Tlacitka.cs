using System;
using System.Drawing;
using System.Windows.Forms;

namespace NewGUI
{
    public partial class Tlacitka : UserControl
    {
        // Veøejné vlastnosti pro nastavení zvenèí
        public Image NormalImage { get; set; }
        public Image HoverImage { get; set; }
        public string Title
        {
            get => headerButton.Text;
            set => headerButton.Text = value;
        }
        public string DetailText
        {
            get => detailLabel.Text;
            set
            {
                detailLabel.Text = value ?? string.Empty;
                RecalculateDetailHeight();
            }
        }

        // Událost, když uživatel "aktivuje" dlaždici (klik)
        public event EventHandler Activated;

        // --- Vnitøek ---
        private readonly Button headerButton = new Button();
        private readonly Panel detailPanel = new Panel();
        private readonly Label detailLabel = new Label();
        private readonly Timer anim = new Timer { Interval = 15 };

        private int targetHeight = 0;   // cílová výška detailPanelu (0 nebo vypoètená)
        private int currentHeight = 0;  // aktuální výška pro animaci
        private int speed = 15;         // pixely na tick
        public int ExpandedHeight { get; set; } = 130;  // fallback, pokud nechceme dynamicky zvìtšit nad urèitou mez
        private readonly Color hoverOverlay = Color.FromArgb(243, 95, 0);
        private readonly Color textNormal = Color.White;

        // dynamická vypoètená výška dle obsahu
        private int _preferredDetailHeight = 0;

        public Tlacitka()
        {
            // základní vzhled
            BackColor = Color.White;
            Margin = new Padding(0);

            // HLAVIÈKA (tlaèítko)
            headerButton.Dock = DockStyle.Top;
            headerButton.BackColor = Color.FromArgb(220, 218, 215);
            headerButton.Height = 130;
            headerButton.TextAlign = ContentAlignment.TopCenter;
            headerButton.ImageAlign = ContentAlignment.MiddleLeft;
            headerButton.FlatStyle = FlatStyle.Flat;
            headerButton.FlatAppearance.BorderSize = 0;
            headerButton.Font = new Font("Bahnschrift", 20);
            headerButton.UseMnemonic = false;

            // DETAIL PANEL
            detailPanel.Dock = DockStyle.Top;
            detailPanel.Height = 0; // start schovaný
            detailPanel.BackColor = Color.White;

            // TEXT UVNITØ
            detailLabel.Dock = DockStyle.Fill;
            detailLabel.Padding = new Padding(12);
            detailLabel.Font = new Font("Bahnschrift", 10);
            detailLabel.AutoEllipsis = false; // nechceme tøi teèky, chceme celý text
            detailLabel.AutoSize = false;
            detailLabel.MaximumSize = new Size(0, 0); // budeme mìøit sami
            detailLabel.Text = string.Empty;
            detailLabel.UseCompatibleTextRendering = false; // TextRenderer.MeasureText používá GDI+

            detailPanel.Controls.Add(detailLabel);
            Controls.Add(detailPanel);
            Controls.Add(headerButton);

            // Malba hlavièky s overlay + ikonou
            headerButton.Paint += (s, e) =>
            {
                var g = e.Graphics;
                var rect = headerButton.ClientRectangle;

                // oranžový overlay odspodu podle currentHeight
                if (currentHeight > 0)
                {
                    int h = Math.Min(currentHeight, rect.Height);
                    var overlayRect = new Rectangle(0, rect.Height - h, rect.Width, h);
                    var b = new SolidBrush(hoverOverlay);
                    g.FillRectangle(b, overlayRect);
                }

                // 1) nejdøív obrázek vlevo
                var img = (currentHeight > rect.Width / 5 && HoverImage != null) ? HoverImage : NormalImage;
                if (img != null)
                {
                    int x = 0;
                    int y = 5;
                    g.DrawImage(img, x, y, img.Width, img.Height);
                }

                // 2) potom text (bude navrchu pøes obrázek)
                var textRect = new Rectangle(rect.Left, rect.Top + 4, rect.Width, rect.Height);
                var tff = TextFormatFlags.Top | TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis;

                // text mimo overlay èernì
                TextRenderer.DrawText(g, headerButton.Text, headerButton.Font, textRect, Color.Black, tff);

                // text uvnitø overlay bíle
                if (currentHeight > 0)
                {
                    int h = Math.Min(currentHeight, rect.Height);
                    var overlayRect = new Rectangle(0, rect.Height - h, rect.Width, h);
                    g.SetClip(overlayRect, System.Drawing.Drawing2D.CombineMode.Replace);
                    TextRenderer.DrawText(g, headerButton.Text, headerButton.Font, textRect, textNormal, tff);
                    g.ResetClip();
                }
            };

            // Hover/leave pro CELÝ control (vèetnì dìtí)
            WireHover(this);
            foreach (Control c in Controls) WireHover(c);

            headerButton.Click += (_, __) => Activated?.Invoke(this, EventArgs.Empty);

            anim.Tick += (_, __) =>
            {
                if (currentHeight < targetHeight) currentHeight = Math.Min(targetHeight, currentHeight + speed);
                else if (currentHeight > targetHeight) currentHeight = Math.Max(targetHeight, currentHeight - speed);

                detailPanel.Height = currentHeight;
                headerButton.Invalidate();

                if (currentHeight == targetHeight) anim.Stop();
            };

            // reagovat na zmìnu velikosti controlu: pøepoèítat výšku obsahu
            this.Resize += (s, e) =>
            {
                RecalculateDetailHeight();
                // pokud je teï rozbaleno, uprav okamžitì výšku
                if (targetHeight > 0)
                {
                    targetHeight = Math.Max(_preferredDetailHeight, 0);
                    detailPanel.Height = targetHeight;
                    currentHeight = targetHeight;
                    headerButton.Invalidate();
                }
            };
        }

        private void WireHover(Control c)
        {
            headerButton.MouseEnter += (_, __) => SetExpanded(true);
            headerButton.MouseLeave += (_, __) => SetExpanded(false);
        }

        public void SetExpanded(bool expanded)
        {
            // Pøed rozbalením pøepoèítáme preferovanou výšku dle textu
            RecalculateDetailHeight();

            // pokud je preferovaná výška nulová (žádný text), použij fallback ExpandedHeight
            int desired = Math.Max(_preferredDetailHeight, ExpandedHeight);
            // pokud chceš vždy zobrazit celý text, nastav desired = _preferredDetailHeight;

            targetHeight = expanded ? desired : 0;
            anim.Start();
        }

        private void RecalculateDetailHeight()
        {
            // Bez textu - preferovaná výška 0
            if (string.IsNullOrEmpty(detailLabel.Text))
            {
                _preferredDetailHeight = 0;
                return;
            }

            // Šíøka dostupná pro text (odeèteme padding)
            int availableWidth = Math.Max(10, this.Width - detailLabel.Padding.Left - detailLabel.Padding.Right - 8);

            // Mìøení textu s lomem slov (wordbreak)
            var size = TextRenderer.MeasureText(detailLabel.Text, detailLabel.Font, new Size(availableWidth, int.MaxValue),
                TextFormatFlags.WordBreak);

            // Pøidat vertikální padding
            _preferredDetailHeight = size.Height + detailLabel.Padding.Top + detailLabel.Padding.Bottom;

            // Nepøesáhnout pøimìøenou mez (pokud chceš omezit maximální rozbalení, uprav pole Max)
            int maxAllowed = 800; // bezpeènostní limit, uprav dle potøeby
            if (_preferredDetailHeight > maxAllowed) _preferredDetailHeight = maxAllowed;
        }
    }
}