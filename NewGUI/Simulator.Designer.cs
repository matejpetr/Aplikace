namespace NewGUI
{
    partial class Simulator
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
            this.components = new System.ComponentModel.Container();
            this.btnConnect = new System.Windows.Forms.Button();
            this.comBox = new System.Windows.Forms.ComboBox();
            this.lblComPort = new System.Windows.Forms.Label();
            this.lblSensor = new System.Windows.Forms.Label();
            this.sensorBox = new System.Windows.Forms.ComboBox();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.component_pic = new System.Windows.Forms.PictureBox();
            this.textBox = new System.Windows.Forms.TextBox();
            this.ToolTipSim = new System.Windows.Forms.ToolTip(this.components);
            this.lblAktuator = new System.Windows.Forms.Label();
            this.lblType = new System.Windows.Forms.Label();
            this.badgeConn = new System.Windows.Forms.Label();
            this.TypeBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.component_pic)).BeginInit();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(108)))), ((int)(((byte)(189)))));
            this.btnConnect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConnect.FlatAppearance.BorderSize = 0;
            this.btnConnect.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btnConnect.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(62)))), ((int)(((byte)(181)))));
            this.btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConnect.Font = new System.Drawing.Font("Segoe UI Variable Text Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btnConnect.ForeColor = System.Drawing.Color.White;
            this.btnConnect.Location = new System.Drawing.Point(28, 20);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(2);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);
            this.btnConnect.Size = new System.Drawing.Size(147, 40);
            this.btnConnect.TabIndex = 2;
            this.btnConnect.Text = "Připojit";
            this.btnConnect.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTipSim.SetToolTip(this.btnConnect, "Naváže spojení se zadaným portem nebo ho odpojí");
            this.btnConnect.UseVisualStyleBackColor = false;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // comBox
            // 
            this.comBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.comBox.BackColor = System.Drawing.Color.White;
            this.comBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comBox.Font = new System.Drawing.Font("Segoe UI Variable Display", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.comBox.ForeColor = System.Drawing.SystemColors.WindowText;
            this.comBox.FormattingEnabled = true;
            this.comBox.IntegralHeight = false;
            this.comBox.ItemHeight = 21;
            this.comBox.Location = new System.Drawing.Point(292, 75);
            this.comBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comBox.Name = "comBox";
            this.comBox.Size = new System.Drawing.Size(130, 29);
            this.comBox.TabIndex = 4;
            this.ToolTipSim.SetToolTip(this.comBox, "Zvol odpovídají COM pro sériovou komunikaci");
            // 
            // lblComPort
            // 
            this.lblComPort.AutoSize = true;
            this.lblComPort.Font = new System.Drawing.Font("Segoe UI Variable Display", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblComPort.Location = new System.Drawing.Point(224, 78);
            this.lblComPort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblComPort.Name = "lblComPort";
            this.lblComPort.Size = new System.Drawing.Size(39, 21);
            this.lblComPort.TabIndex = 5;
            this.lblComPort.Text = "Port";
            // 
            // lblSensor
            // 
            this.lblSensor.AutoSize = true;
            this.lblSensor.Location = new System.Drawing.Point(224, 120);
            this.lblSensor.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSensor.Name = "lblSensor";
            this.lblSensor.Size = new System.Drawing.Size(53, 20);
            this.lblSensor.TabIndex = 5;
            this.lblSensor.Text = "Senzor";
            this.lblSensor.Visible = false;
            // 
            // sensorBox
            // 
            this.sensorBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.sensorBox.BackColor = System.Drawing.Color.White;
            this.sensorBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sensorBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.sensorBox.Font = new System.Drawing.Font("Segoe UI Variable Display", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.sensorBox.ForeColor = System.Drawing.SystemColors.WindowText;
            this.sensorBox.FormattingEnabled = true;
            this.sensorBox.IntegralHeight = false;
            this.sensorBox.ItemHeight = 21;
            this.sensorBox.Location = new System.Drawing.Point(292, 120);
            this.sensorBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.sensorBox.Name = "sensorBox";
            this.sensorBox.Size = new System.Drawing.Size(130, 29);
            this.sensorBox.TabIndex = 4;
            this.ToolTipSim.SetToolTip(this.sensorBox, "Vyber odpovídající senzor");
            this.sensorBox.Visible = false;
            // 
            // btnStartStop
            // 
            this.btnStartStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(108)))), ((int)(((byte)(189)))));
            this.btnStartStop.Enabled = false;
            this.btnStartStop.FlatAppearance.BorderSize = 0;
            this.btnStartStop.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btnStartStop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(62)))), ((int)(((byte)(181)))));
            this.btnStartStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartStop.Font = new System.Drawing.Font("Segoe UI Variable Text Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btnStartStop.ForeColor = System.Drawing.Color.White;
            this.btnStartStop.Location = new System.Drawing.Point(28, 75);
            this.btnStartStop.Margin = new System.Windows.Forms.Padding(2);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);
            this.btnStartStop.Size = new System.Drawing.Size(147, 40);
            this.btnStartStop.TabIndex = 4;
            this.btnStartStop.Text = "Spustit ";
            this.ToolTipSim.SetToolTip(this.btnStartStop, "Spustí/Zastaví akci simulace");
            this.btnStartStop.UseVisualStyleBackColor = false;
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // 
            // component_pic
            // 
            this.component_pic.Location = new System.Drawing.Point(505, 191);
            this.component_pic.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.component_pic.Name = "component_pic";
            this.component_pic.Size = new System.Drawing.Size(140, 140);
            this.component_pic.TabIndex = 7;
            this.component_pic.TabStop = false;
            // 
            // textBox
            // 
            this.textBox.BackColor = System.Drawing.Color.White;
            this.textBox.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBox.Location = new System.Drawing.Point(0, 341);
            this.textBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox.Multiline = true;
            this.textBox.Name = "textBox";
            this.textBox.ReadOnly = true;
            this.textBox.Size = new System.Drawing.Size(666, 109);
            this.textBox.TabIndex = 0;
            this.ToolTipSim.SetToolTip(this.textBox, "Výpis náhodně simulovaných hodnot");
            // 
            // lblAktuator
            // 
            this.lblAktuator.AutoSize = true;
            this.lblAktuator.Font = new System.Drawing.Font("Segoe UI Variable Display", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblAktuator.Location = new System.Drawing.Point(224, 120);
            this.lblAktuator.Name = "lblAktuator";
            this.lblAktuator.Size = new System.Drawing.Size(66, 21);
            this.lblAktuator.TabIndex = 10;
            this.lblAktuator.Text = "Senzory";
            this.lblAktuator.Visible = false;
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Font = new System.Drawing.Font("Segoe UI Variable Display", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblType.Location = new System.Drawing.Point(437, 78);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(78, 21);
            this.lblType.TabIndex = 10;
            this.lblType.Text = "Typ prvku";
            // 
            // badgeConn
            // 
            this.badgeConn.AutoSize = true;
            this.badgeConn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.badgeConn.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.badgeConn.ForeColor = System.Drawing.Color.White;
            this.badgeConn.Location = new System.Drawing.Point(224, 20);
            this.badgeConn.Name = "badgeConn";
            this.badgeConn.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            this.badgeConn.Size = new System.Drawing.Size(123, 29);
            this.badgeConn.TabIndex = 13;
            this.badgeConn.Text = "Nepřipojeno";
            // 
            // TypeBox
            // 
            this.TypeBox.Font = new System.Drawing.Font("Segoe UI Variable Display", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.TypeBox.Location = new System.Drawing.Point(516, 75);
            this.TypeBox.Name = "TypeBox";
            this.TypeBox.Size = new System.Drawing.Size(140, 33);
            this.TypeBox.TabIndex = 14;
            this.TypeBox.Text = "Senzory";
            this.TypeBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Simulator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(249)))));
            this.Controls.Add(this.TypeBox);
            this.Controls.Add(this.badgeConn);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.lblAktuator);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.btnStartStop);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.component_pic);
            this.Controls.Add(this.lblSensor);
            this.Controls.Add(this.lblComPort);
            this.Controls.Add(this.sensorBox);
            this.Controls.Add(this.comBox);
            this.Font = new System.Drawing.Font("Segoe UI Variable Display", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Simulator";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.Size = new System.Drawing.Size(666, 450);
            ((System.ComponentModel.ISupportInitialize)(this.component_pic)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.ComboBox comBox;
        private System.Windows.Forms.Label lblComPort;
        private System.Windows.Forms.Label lblSensor;
        private System.Windows.Forms.ComboBox sensorBox;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.PictureBox component_pic;
        private System.Windows.Forms.TextBox textBox;
        private System.Windows.Forms.ToolTip ToolTipSim;
        private System.Windows.Forms.Label lblAktuator;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.Label badgeConn;
        private System.Windows.Forms.TextBox TypeBox;
    }
}
