namespace NewGUI
{
    partial class Home
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
            this.Simulator_panel = new System.Windows.Forms.Panel();
            this.Simulator_button = new System.Windows.Forms.Button();
            this.Sensor_panel = new System.Windows.Forms.Panel();
            this.Sensor_button = new System.Windows.Forms.Button();
            this.Aktuatory_panel = new System.Windows.Forms.Panel();
            this.Aktuatory_button = new System.Windows.Forms.Button();
            this.Simulator_panel.SuspendLayout();
            this.Sensor_panel.SuspendLayout();
            this.Aktuatory_panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // Simulator_panel
            // 
            this.Simulator_panel.Controls.Add(this.Simulator_button);
            this.Simulator_panel.Location = new System.Drawing.Point(452, 0);
            this.Simulator_panel.Name = "Simulator_panel";
            this.Simulator_panel.Size = new System.Drawing.Size(220, 444);
            this.Simulator_panel.TabIndex = 3;
            // 
            // Simulator_button
            // 
            this.Simulator_button.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(218)))), ((int)(((byte)(215)))));
            this.Simulator_button.Font = new System.Drawing.Font("Bahnschrift", 25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Simulator_button.Image = global::NewGUI.Properties.Resources.half_brain_mini3;
            this.Simulator_button.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Simulator_button.Location = new System.Drawing.Point(0, 0);
            this.Simulator_button.Name = "Simulator_button";
            this.Simulator_button.Size = new System.Drawing.Size(225, 180);
            this.Simulator_button.TabIndex = 1;
            this.Simulator_button.Text = "Simulator";
            this.Simulator_button.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.Simulator_button.UseMnemonic = false;
            this.Simulator_button.UseVisualStyleBackColor = false;
            this.Simulator_button.Click += new System.EventHandler(this.Simulator_button_Click);
            // 
            // Sensor_panel
            // 
            this.Sensor_panel.Controls.Add(this.Sensor_button);
            this.Sensor_panel.Location = new System.Drawing.Point(2, 0);
            this.Sensor_panel.Name = "Sensor_panel";
            this.Sensor_panel.Size = new System.Drawing.Size(220, 444);
            this.Sensor_panel.TabIndex = 2;
            // 
            // Sensor_button
            // 
            this.Sensor_button.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(218)))), ((int)(((byte)(215)))));
            this.Sensor_button.Font = new System.Drawing.Font("Bahnschrift", 25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Sensor_button.Image = global::NewGUI.Properties.Resources.half_brain_mini3;
            this.Sensor_button.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Sensor_button.Location = new System.Drawing.Point(0, 0);
            this.Sensor_button.Name = "Sensor_button";
            this.Sensor_button.Size = new System.Drawing.Size(225, 180);
            this.Sensor_button.TabIndex = 1;
            this.Sensor_button.Text = "Senzory";
            this.Sensor_button.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.Sensor_button.UseMnemonic = false;
            this.Sensor_button.UseVisualStyleBackColor = false;
            this.Sensor_button.Click += new System.EventHandler(this.Sensor_button_Click);
            // 
            // Aktuatory_panel
            // 
            this.Aktuatory_panel.Controls.Add(this.Aktuatory_button);
            this.Aktuatory_panel.Location = new System.Drawing.Point(227, 0);
            this.Aktuatory_panel.Name = "Aktuatory_panel";
            this.Aktuatory_panel.Size = new System.Drawing.Size(220, 444);
            this.Aktuatory_panel.TabIndex = 4;
            this.Aktuatory_panel.Paint += new System.Windows.Forms.PaintEventHandler(this.Aktuatory_panel_Paint);
            // 
            // Aktuatory_button
            // 
            this.Aktuatory_button.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(218)))), ((int)(((byte)(215)))));
            this.Aktuatory_button.Font = new System.Drawing.Font("Bahnschrift", 25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Aktuatory_button.Image = global::NewGUI.Properties.Resources.half_brain_mini3;
            this.Aktuatory_button.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Aktuatory_button.Location = new System.Drawing.Point(0, 0);
            this.Aktuatory_button.Name = "Aktuatory_button";
            this.Aktuatory_button.Size = new System.Drawing.Size(225, 180);
            this.Aktuatory_button.TabIndex = 1;
            this.Aktuatory_button.Text = "Aktuátory";
            this.Aktuatory_button.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.Aktuatory_button.UseMnemonic = false;
            this.Aktuatory_button.UseVisualStyleBackColor = false;
            this.Aktuatory_button.Click += new System.EventHandler(this.Aktuatory_button_Click);
            // 
            // Home
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.Simulator_panel);
            this.Controls.Add(this.Sensor_panel);
            this.Controls.Add(this.Aktuatory_panel);
            this.Name = "Home";
            this.Size = new System.Drawing.Size(666, 450);
            this.Load += new System.EventHandler(this.Home_Load);
            this.Simulator_panel.ResumeLayout(false);
            this.Sensor_panel.ResumeLayout(false);
            this.Aktuatory_panel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel Simulator_panel;
        private System.Windows.Forms.Button Simulator_button;
        private System.Windows.Forms.Panel Sensor_panel;
        private System.Windows.Forms.Button Sensor_button;
        private System.Windows.Forms.Panel Aktuatory_panel;
        private System.Windows.Forms.Button Aktuatory_button;
    }
}
