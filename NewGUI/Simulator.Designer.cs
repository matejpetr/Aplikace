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
            this.btnConnect = new System.Windows.Forms.Button();
            this.comBox = new System.Windows.Forms.ComboBox();
            this.lblComPort = new System.Windows.Forms.Label();
            this.lblSensor = new System.Windows.Forms.Label();
            this.sensorBox = new System.Windows.Forms.ComboBox();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.component_pic = new System.Windows.Forms.PictureBox();
            this.textBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.TypeBox = new System.Windows.Forms.ComboBox();
            this.lblAktuator = new System.Windows.Forms.Label();
            this.lblType = new System.Windows.Forms.Label();
            this.badgeConn = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.component_pic)).BeginInit();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnConnect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConnect.FlatAppearance.BorderSize = 0;
            this.btnConnect.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btnConnect.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(62)))), ((int)(((byte)(181)))));
            this.btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConnect.Font = new System.Drawing.Font("Segoe UI Variable Text Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btnConnect.ForeColor = System.Drawing.Color.White;
            this.btnConnect.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnConnect.Location = new System.Drawing.Point(59, 128);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(120, 40);
            this.btnConnect.TabIndex = 2;
            this.btnConnect.Text = "Připojit";
            this.btnConnect.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnConnect.UseVisualStyleBackColor = false;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // comBox
            // 
            this.comBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.comBox.BackColor = System.Drawing.Color.White;
            this.comBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comBox.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.5F);
            this.comBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.comBox.FormattingEnabled = true;
            this.comBox.IntegralHeight = false;
            this.comBox.ItemHeight = 19;
            this.comBox.Location = new System.Drawing.Point(59, 82);
            this.comBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comBox.Name = "comBox";
            this.comBox.Size = new System.Drawing.Size(160, 27);
            this.comBox.TabIndex = 4;
            // 
            // lblComPort
            // 
            this.lblComPort.AutoSize = true;
            this.lblComPort.Location = new System.Drawing.Point(16, 84);
            this.lblComPort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblComPort.Name = "lblComPort";
            this.lblComPort.Size = new System.Drawing.Size(35, 20);
            this.lblComPort.TabIndex = 5;
            this.lblComPort.Text = "Port";
            // 
            // lblSensor
            // 
            this.lblSensor.AutoSize = true;
            this.lblSensor.Location = new System.Drawing.Point(354, 146);
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
            this.sensorBox.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.5F);
            this.sensorBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.sensorBox.FormattingEnabled = true;
            this.sensorBox.IntegralHeight = false;
            this.sensorBox.ItemHeight = 19;
            this.sensorBox.Location = new System.Drawing.Point(455, 144);
            this.sensorBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.sensorBox.Name = "sensorBox";
            this.sensorBox.Size = new System.Drawing.Size(160, 27);
            this.sensorBox.TabIndex = 4;
            this.sensorBox.Visible = false;
            // 
            // btnStartStop
            // 
            this.btnStartStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnStartStop.Enabled = false;
            this.btnStartStop.FlatAppearance.BorderSize = 0;
            this.btnStartStop.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btnStartStop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(62)))), ((int)(((byte)(181)))));
            this.btnStartStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartStop.Font = new System.Drawing.Font("Segoe UI Variable Text Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btnStartStop.ForeColor = System.Drawing.Color.White;
            this.btnStartStop.Location = new System.Drawing.Point(59, 178);
            this.btnStartStop.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(120, 40);
            this.btnStartStop.TabIndex = 6;
            this.btnStartStop.Text = "Spustit ";
            this.btnStartStop.UseVisualStyleBackColor = false;
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // 
            // component_pic
            // 
            this.component_pic.Location = new System.Drawing.Point(475, 181);
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
            this.textBox.Location = new System.Drawing.Point(0, 331);
            this.textBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox.Multiline = true;
            this.textBox.Name = "textBox";
            this.textBox.ReadOnly = true;
            this.textBox.Size = new System.Drawing.Size(666, 119);
            this.textBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(25, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(123, 36);
            this.label1.TabIndex = 8;
            this.label1.Text = "Připojení";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label2.Location = new System.Drawing.Point(349, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(126, 36);
            this.label2.TabIndex = 8;
            this.label2.Text = "Simulace";
            // 
            // TypeBox
            // 
            this.TypeBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.TypeBox.AutoCompleteCustomSource.AddRange(new string[] {
            "Aktuátory",
            "Senzory"});
            this.TypeBox.BackColor = System.Drawing.Color.White;
            this.TypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TypeBox.Enabled = false;
            this.TypeBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TypeBox.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.5F);
            this.TypeBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.TypeBox.FormattingEnabled = true;
            this.TypeBox.IntegralHeight = false;
            this.TypeBox.ItemHeight = 19;
            this.TypeBox.Items.AddRange(new object[] {
            "Aktuátory",
            "Senzory"});
            this.TypeBox.Location = new System.Drawing.Point(455, 82);
            this.TypeBox.Name = "TypeBox";
            this.TypeBox.Size = new System.Drawing.Size(160, 27);
            this.TypeBox.TabIndex = 9;
            // 
            // lblAktuator
            // 
            this.lblAktuator.AutoSize = true;
            this.lblAktuator.Location = new System.Drawing.Point(354, 146);
            this.lblAktuator.Name = "lblAktuator";
            this.lblAktuator.Size = new System.Drawing.Size(65, 20);
            this.lblAktuator.TabIndex = 10;
            this.lblAktuator.Text = "Aktuátor";
            this.lblAktuator.Visible = false;
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(354, 84);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(73, 20);
            this.lblType.TabIndex = 10;
            this.lblType.Text = "Typ prvku";
            // 
            // badgeConn
            // 
            this.badgeConn.AutoSize = true;
            this.badgeConn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.badgeConn.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.badgeConn.ForeColor = System.Drawing.Color.White;
            this.badgeConn.Location = new System.Drawing.Point(163, 27);
            this.badgeConn.Name = "badgeConn";
            this.badgeConn.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            this.badgeConn.Size = new System.Drawing.Size(123, 29);
            this.badgeConn.TabIndex = 13;
            this.badgeConn.Text = "Nepřipojeno";
            // 
            // Simulator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(249)))));
            this.Controls.Add(this.badgeConn);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.lblAktuator);
            this.Controls.Add(this.TypeBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox TypeBox;
        private System.Windows.Forms.Label lblAktuator;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.Label badgeConn;
    }
}
