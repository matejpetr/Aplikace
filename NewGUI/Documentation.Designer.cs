namespace NewGUI
{
    partial class Documentation
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
            this.Document = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // Document
            // 
            this.Document.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Document.Location = new System.Drawing.Point(0, 0);
            this.Document.Name = "Document";
            this.Document.Size = new System.Drawing.Size(666, 450);
            this.Document.TabIndex = 0;
            // 
            // Documentation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Document);
            this.Name = "Documentation";
            this.Size = new System.Drawing.Size(666, 450);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel Document;
    }
}
