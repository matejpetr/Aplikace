using System;
using System.Drawing;
using System.Windows.Forms;

namespace NewGUI
{
    public partial class Tlacitka : UserControl
    {
        // Veřejné vlastnosti pro nastavení zvenčí
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
            set => detailLabel.Text = value;
        }

        // Událost, když uživatel "aktivuje" dlaždici (klik)
        public event EventHandler Activated;

        // --- Vnitřek ---
        private readonly Button headerButton = new Button();
        private readonly Panel detailPanel = new Panel();
        private readonly Label detailLabel = new Label();
        private readonly Timer anim = new Timer { Interval = 15 };

        private int targetHeight = 0;   // cílová výška detailPanelu (0 nebo ExpandedHeight)
        private int currentHeight = 0;  // aktuální výška pro animaci
        private int speed = 15;         // pixely na tick
        public int ExpandedHeight { get; set; } = 130;  // kolik se má vysunout
        private readonly Color hoverOverlay = Color.FromArgb(243, 95, 0);
        private readonly Color textNormal = Color.White;

        public Tlacitka()
        {
            // základní vzhled
            BackColor = Color.White;
            Margin = new Padding(0);
            //BackColor = Color.FromArgb(220, 218, 215);
            // HLAVIČKA (tlačítko)
            headerButton.Dock = DockStyle.Top;
            headerButton.BackColor = Color.FromArgb(220, 218, 215); // šedé tlačítko
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

            // TEXT UVNITŘ
            detailLabel.Dock = DockStyle.Fill;
            detailLabel.Padding = new Padding(12);
            detailLabel.Font = new Font("Bahnschrift", 10);
            detailLabel.AutoEllipsis = true;

            detailPanel.Controls.Add(detailLabel);
            Controls.Add(detailPanel);
            Controls.Add(headerButton);

            // Malba hlavičky s overlay + ikonou, velmi podobné tvému kódu
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

                // 1) nejdřív obrázek vlevo
                var img = (currentHeight > rect.Width / 5 && HoverImage != null) ? HoverImage : NormalImage;
                if (img != null)
                {
                    int x = 0;
                    int y = 5;
                    g.DrawImage(img, x, y, img.Width, img.Height);
                }

                // 2) potom text (bude navrchu přes obrázek)
                var textRect = new Rectangle(rect.Left, rect.Top + 4, rect.Width, rect.Height);
                var tff = TextFormatFlags.Top | TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis;

                // text mimo overlay černě
                TextRenderer.DrawText(g, headerButton.Text, headerButton.Font, textRect, Color.Black, tff);

                // text uvnitř overlay bíle
                if (currentHeight > 0)
                {
                    int h = Math.Min(currentHeight, rect.Height);
                    var overlayRect = new Rectangle(0, rect.Height - h, rect.Width, h);
                    g.SetClip(overlayRect, System.Drawing.Drawing2D.CombineMode.Replace);
                    TextRenderer.DrawText(g, headerButton.Text, headerButton.Font, textRect, textNormal, tff);
                    g.ResetClip();
                }
            };

            // Hover/leave pro CELÝ control (včetně dětí)
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
        }

        private void WireHover(Control c)
        {
            // v konstruktoru místo WireHover(this) + foreach
            headerButton.MouseEnter += (_, __) => SetExpanded(true);
            headerButton.MouseLeave += (_, __) => SetExpanded(false);

        }

        public void SetExpanded(bool expanded)
        {
            targetHeight = expanded ? ExpandedHeight : 0;
            anim.Start();
            // přepnutí obrázku se řeší v Paintu přes currentHeight
        }
    }
}
