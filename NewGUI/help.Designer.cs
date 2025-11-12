namespace NewGUI
{
    partial class help
    {
        /// <summary> 
        /// Vyžaduje se proměnná návrháře.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Uvolněte všechny používané prostředky.
        /// </summary>
        /// <param name="disposing">hodnota true, když by se měl spravovaný prostředek odstranit; jinak false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kód vygenerovaný pomocí Návrháře komponent

        /// <summary> 
        /// Metoda vyžadovaná pro podporu Návrháře - neupravovat
        /// obsah této metody v editoru kódu.
        /// </summary>
        private void InitializeComponent()
        {
            this.Document_panel = new System.Windows.Forms.Panel();
            this.Document_button = new System.Windows.Forms.Button();
            this.Popis_panel = new System.Windows.Forms.Panel();
            this.Popis_button = new System.Windows.Forms.Button();
            this.Document_panel.SuspendLayout();
            this.Popis_panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // Document_panel
            // 
            this.Document_panel.Controls.Add(this.Document_button);
            this.Document_panel.Location = new System.Drawing.Point(0, 0);
            this.Document_panel.Name = "Document_panel";
            this.Document_panel.Size = new System.Drawing.Size(223, 420);
            this.Document_panel.TabIndex = 5;
            // 
            // Document_button
            // 
            this.Document_button.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(218)))), ((int)(((byte)(215)))));
            this.Document_button.Font = new System.Drawing.Font("Bahnschrift", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Document_button.Image = global::NewGUI.Properties.Resources.half_brain_mini3;
            this.Document_button.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Document_button.Location = new System.Drawing.Point(0, 0);
            this.Document_button.Name = "Document_button";
            this.Document_button.Size = new System.Drawing.Size(225, 180);
            this.Document_button.TabIndex = 1;
            this.Document_button.Text = "Dokumentace";
            this.Document_button.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.Document_button.UseMnemonic = false;
            this.Document_button.UseVisualStyleBackColor = false;
            this.Document_button.Click += new System.EventHandler(this.Document_button_click);
            // 
            // Popis_panel
            // 
            this.Popis_panel.Controls.Add(this.Popis_button);
            this.Popis_panel.Location = new System.Drawing.Point(227, 0);
            this.Popis_panel.Name = "Popis_panel";
            this.Popis_panel.Size = new System.Drawing.Size(223, 420);
            this.Popis_panel.TabIndex = 6;
            // 
            // Popis_button
            // 
            this.Popis_button.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(218)))), ((int)(((byte)(215)))));
            this.Popis_button.Font = new System.Drawing.Font("Bahnschrift", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Popis_button.Image = global::NewGUI.Properties.Resources.half_brain_mini3;
            this.Popis_button.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Popis_button.Location = new System.Drawing.Point(0, 0);
            this.Popis_button.Name = "Popis_button";
            this.Popis_button.Size = new System.Drawing.Size(225, 180);
            this.Popis_button.TabIndex = 1;
            this.Popis_button.Text = "Popis Aplikace";
            this.Popis_button.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.Popis_button.UseMnemonic = false;
            this.Popis_button.UseVisualStyleBackColor = false;
            this.Popis_button.Click += new System.EventHandler(this.Popis_button_Click);
            // 
            // help
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.Popis_panel);
            this.Controls.Add(this.Document_panel);
            this.Name = "help";
            this.Size = new System.Drawing.Size(666, 420);
            this.Document_panel.ResumeLayout(false);
            this.Popis_panel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel Document_panel;
        private System.Windows.Forms.Button Document_button;
        private System.Windows.Forms.Panel Popis_panel;
        private System.Windows.Forms.Button Popis_button;
    }
}
