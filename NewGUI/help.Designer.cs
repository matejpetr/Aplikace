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
            this.Aktuatory_panel = new System.Windows.Forms.Panel();
            this.Document_button = new System.Windows.Forms.Button();
            this.Aktuatory_panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // Aktuatory_panel
            // 
            this.Aktuatory_panel.Controls.Add(this.Document_button);
            this.Aktuatory_panel.Location = new System.Drawing.Point(3, 3);
            this.Aktuatory_panel.Name = "Aktuatory_panel";
            this.Aktuatory_panel.Size = new System.Drawing.Size(244, 444);
            this.Aktuatory_panel.TabIndex = 5;
            // 
            // Document_button
            // 
            this.Document_button.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(218)))), ((int)(((byte)(215)))));
            this.Document_button.Font = new System.Drawing.Font("Bahnschrift", 25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
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
            // help
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Aktuatory_panel);
            this.Name = "help";
            this.Size = new System.Drawing.Size(666, 420);
            this.Aktuatory_panel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel Aktuatory_panel;
        private System.Windows.Forms.Button Document_button;
    }
}
