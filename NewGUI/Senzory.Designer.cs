namespace NewGUI
{
    partial class Senzory
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
            if (disposing)
            {
                // Odhlášení eventů a uvolnění timerů/objektů
                try { _serialController?.Dispose(); } catch { }
                try { _chartManager?.Dispose(); } catch { }

                try
                {
                    if (comPortWatcherTimer != null)
                    {
                        comPortWatcherTimer.Stop();
                        comPortWatcherTimer.Tick -= ComPortWatcherTimer_Tick;
                        comPortWatcherTimer.Dispose();
                        comPortWatcherTimer = null;
                    }
                }
                catch { }

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.button1 = new System.Windows.Forms.Button();
            this.comboBoxSensor = new System.Windows.Forms.ComboBox();
            this.comboBoxMode = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.comboBoxCOM = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBoxTIMER = new System.Windows.Forms.ComboBox();
            this.ConnectBtn = new System.Windows.Forms.Button();
            this.badgeConn = new System.Windows.Forms.Label();
            this.ToolTipS = new System.Windows.Forms.ToolTip(this.components);
            this.label8 = new System.Windows.Forms.Label();
            this.PIN1 = new System.Windows.Forms.Label();
            this.PIN2 = new System.Windows.Forms.Label();
            this.PIN3 = new System.Windows.Forms.Label();
            this.textPIN1 = new System.Windows.Forms.TextBox();
            this.textPIN2 = new System.Windows.Forms.TextBox();
            this.textPIN3 = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.valueText = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.savecsv_btn = new System.Windows.Forms.Button();
            this.reset_btn = new System.Windows.Forms.Button();
            this.init_btn = new System.Windows.Forms.Button();
            this.link_btn = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(108)))), ((int)(((byte)(189)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Segoe UI Variable Text Semibold", 14.25F, System.Drawing.FontStyle.Bold);
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Location = new System.Drawing.Point(28, 75);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(197, 40);
            this.button1.TabIndex = 1;
            this.button1.Text = "Spustit";
            this.ToolTipS.SetToolTip(this.button1, "Spustí/Zastaví akci ve zvoleném režimu");
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // comboBoxSensor
            // 
            this.comboBoxSensor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.comboBoxSensor.FormattingEnabled = true;
            this.comboBoxSensor.IntegralHeight = false;
            this.comboBoxSensor.Location = new System.Drawing.Point(390, 120);
            this.comboBoxSensor.MaxDropDownItems = 10;
            this.comboBoxSensor.Name = "comboBoxSensor";
            this.comboBoxSensor.Size = new System.Drawing.Size(129, 29);
            this.comboBoxSensor.TabIndex = 3;
            this.ToolTipS.SetToolTip(this.comboBoxSensor, "Vyber odpovídající senzor");
            // 
            // comboBoxMode
            // 
            this.comboBoxMode.AllowDrop = true;
            this.comboBoxMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMode.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.comboBoxMode.FormattingEnabled = true;
            this.comboBoxMode.Items.AddRange(new object[] {
            "UPDATE",
            "CONFIG",
            "CONNECT",
            "DISCONNECT"});
            this.comboBoxMode.Location = new System.Drawing.Point(616, 75);
            this.comboBoxMode.MaxDropDownItems = 5;
            this.comboBoxMode.Name = "comboBoxMode";
            this.comboBoxMode.Size = new System.Drawing.Size(129, 29);
            this.comboBoxMode.TabIndex = 4;
            this.ToolTipS.SetToolTip(this.comboBoxMode, "Vyber režim práce senzoru");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(322, 120);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 21);
            this.label1.TabIndex = 5;
            this.label1.Text = "Senzor";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(542, 78);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 21);
            this.label2.TabIndex = 6;
            this.label2.Text = "Režim";
            // 
            // chart1
            // 
            this.chart1.BorderlineColor = System.Drawing.Color.Transparent;
            chartArea1.AxisX.IsLabelAutoFit = false;
            chartArea1.AxisX.LabelStyle.Font = new System.Drawing.Font("Segoe UI Variable Text", 8.25F);
            chartArea1.AxisX.LabelStyle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(95)))));
            chartArea1.AxisX.LineColor = System.Drawing.Color.Gainsboro;
            chartArea1.AxisX.MajorGrid.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(236)))));
            chartArea1.AxisX.MajorTickMark.LineColor = System.Drawing.Color.Gainsboro;
            chartArea1.AxisX.Title = "\"\"";
            chartArea1.AxisY.IsLabelAutoFit = false;
            chartArea1.AxisY.LabelStyle.Font = new System.Drawing.Font("Segoe UI Variable Text", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            chartArea1.AxisY.LabelStyle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(95)))));
            chartArea1.AxisY.LineColor = System.Drawing.Color.Gainsboro;
            chartArea1.AxisY.MajorGrid.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(236)))));
            chartArea1.AxisY.MajorTickMark.LineColor = System.Drawing.Color.Gainsboro;
            chartArea1.AxisY.Title = "\"\"";
            chartArea1.BackColor = System.Drawing.Color.White;
            chartArea1.BorderWidth = 0;
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            this.chart1.ImeMode = System.Windows.Forms.ImeMode.Disable;
            legend1.BackColor = System.Drawing.Color.Transparent;
            legend1.BorderWidth = 0;
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top;
            legend1.Font = new System.Drawing.Font("Segoe UI Variable Text", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            legend1.IsTextAutoFit = false;
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(28, 191);
            this.chart1.Margin = new System.Windows.Forms.Padding(12);
            this.chart1.Name = "chart1";
            this.chart1.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
            series1.BorderWidth = 2;
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series1.Color = System.Drawing.SystemColors.Highlight;
            series1.LabelBorderWidth = 2;
            series1.LabelForeColor = System.Drawing.Color.SteelBlue;
            series1.Legend = "Legend1";
            series1.MarkerBorderColor = System.Drawing.SystemColors.Highlight;
            series1.MarkerBorderWidth = 2;
            series1.MarkerColor = System.Drawing.Color.White;
            series1.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            series1.Name = "Series1";
            this.chart1.Series.Add(series1);
            this.chart1.Size = new System.Drawing.Size(522, 243);
            this.chart1.TabIndex = 9;
            this.chart1.Text = "chart1";
            // 
            // comboBoxCOM
            // 
            this.comboBoxCOM.AllowDrop = true;
            this.comboBoxCOM.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCOM.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.comboBoxCOM.FormattingEnabled = true;
            this.comboBoxCOM.Location = new System.Drawing.Point(390, 75);
            this.comboBoxCOM.MaxDropDownItems = 5;
            this.comboBoxCOM.Name = "comboBoxCOM";
            this.comboBoxCOM.Size = new System.Drawing.Size(129, 29);
            this.comboBoxCOM.TabIndex = 11;
            this.ToolTipS.SetToolTip(this.comboBoxCOM, "Zvol odpovídají COM pro sériovou komunikaci");
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(542, 120);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 21);
            this.label5.TabIndex = 6;
            this.label5.Text = "Perioda";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(322, 78);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(38, 21);
            this.label6.TabIndex = 12;
            this.label6.Text = "Port";
            // 
            // comboBoxTIMER
            // 
            this.comboBoxTIMER.AllowDrop = true;
            this.comboBoxTIMER.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.comboBoxTIMER.FormattingEnabled = true;
            this.comboBoxTIMER.Items.AddRange(new object[] {
            "10 ms",
            "50 ms",
            "100 ms",
            "250 ms",
            "500 ms",
            "1000 ms"});
            this.comboBoxTIMER.Location = new System.Drawing.Point(616, 120);
            this.comboBoxTIMER.MaxDropDownItems = 5;
            this.comboBoxTIMER.Name = "comboBoxTIMER";
            this.comboBoxTIMER.Size = new System.Drawing.Size(129, 29);
            this.comboBoxTIMER.TabIndex = 3;
            this.ToolTipS.SetToolTip(this.comboBoxTIMER, "Zvol periodu výpisu");
            // 
            // ConnectBtn
            // 
            this.ConnectBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(108)))), ((int)(((byte)(189)))));
            this.ConnectBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ConnectBtn.Font = new System.Drawing.Font("Segoe UI Variable Text Semibold", 14.25F, System.Drawing.FontStyle.Bold);
            this.ConnectBtn.ForeColor = System.Drawing.Color.White;
            this.ConnectBtn.Location = new System.Drawing.Point(28, 20);
            this.ConnectBtn.Name = "ConnectBtn";
            this.ConnectBtn.Size = new System.Drawing.Size(197, 40);
            this.ConnectBtn.TabIndex = 1;
            this.ConnectBtn.Text = "Připojit";
            this.ToolTipS.SetToolTip(this.ConnectBtn, "Naváže spojení se zadaným portem nebo ho odpojí");
            this.ConnectBtn.UseVisualStyleBackColor = false;
            this.ConnectBtn.Click += new System.EventHandler(this.ConnectBtn_Click);
            // 
            // badgeConn
            // 
            this.badgeConn.AutoSize = true;
            this.badgeConn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.badgeConn.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.badgeConn.ForeColor = System.Drawing.Color.White;
            this.badgeConn.Location = new System.Drawing.Point(257, 20);
            this.badgeConn.Name = "badgeConn";
            this.badgeConn.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            this.badgeConn.Size = new System.Drawing.Size(123, 29);
            this.badgeConn.TabIndex = 13;
            this.badgeConn.Text = "Nepřipojeno";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI Variable Text", 7F);
            this.label8.ForeColor = System.Drawing.SystemColors.AppWorkspace;
            this.label8.Location = new System.Drawing.Point(12, 436);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(32, 14);
            this.label8.TabIndex = 16;
            this.label8.Text = "label8";
            this.label8.UseMnemonic = false;
            // 
            // PIN1
            // 
            this.PIN1.AutoSize = true;
            this.PIN1.Location = new System.Drawing.Point(625, 343);
            this.PIN1.Name = "PIN1";
            this.PIN1.Size = new System.Drawing.Size(44, 21);
            this.PIN1.TabIndex = 18;
            this.PIN1.Text = "PIN1:";
            this.PIN1.Visible = false;
            // 
            // PIN2
            // 
            this.PIN2.AutoSize = true;
            this.PIN2.Location = new System.Drawing.Point(625, 378);
            this.PIN2.Name = "PIN2";
            this.PIN2.Size = new System.Drawing.Size(47, 21);
            this.PIN2.TabIndex = 18;
            this.PIN2.Text = "PIN2:";
            this.PIN2.Visible = false;
            // 
            // PIN3
            // 
            this.PIN3.AutoSize = true;
            this.PIN3.Location = new System.Drawing.Point(625, 413);
            this.PIN3.Name = "PIN3";
            this.PIN3.Size = new System.Drawing.Size(47, 21);
            this.PIN3.TabIndex = 18;
            this.PIN3.Text = "PIN3:";
            this.PIN3.Visible = false;
            // 
            // textPIN1
            // 
            this.textPIN1.Location = new System.Drawing.Point(710, 338);
            this.textPIN1.Name = "textPIN1";
            this.textPIN1.Size = new System.Drawing.Size(35, 29);
            this.textPIN1.TabIndex = 19;
            this.textPIN1.Visible = false;
            // 
            // textPIN2
            // 
            this.textPIN2.Location = new System.Drawing.Point(710, 373);
            this.textPIN2.Name = "textPIN2";
            this.textPIN2.Size = new System.Drawing.Size(35, 29);
            this.textPIN2.TabIndex = 19;
            this.textPIN2.Visible = false;
            // 
            // textPIN3
            // 
            this.textPIN3.Location = new System.Drawing.Point(710, 408);
            this.textPIN3.Name = "textPIN3";
            this.textPIN3.Size = new System.Drawing.Size(35, 29);
            this.textPIN3.TabIndex = 19;
            this.textPIN3.Visible = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.link_btn);
            this.panel1.Location = new System.Drawing.Point(28, 131);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(43, 45);
            this.panel1.TabIndex = 20;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.init_btn);
            this.panel2.Location = new System.Drawing.Point(132, 131);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(43, 45);
            this.panel2.TabIndex = 22;
            // 
            // valueText
            // 
            this.valueText.AutoSize = true;
            this.valueText.Location = new System.Drawing.Point(250, 200);
            this.valueText.Name = "valueText";
            this.valueText.Size = new System.Drawing.Size(0, 21);
            this.valueText.TabIndex = 24;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.reset_btn);
            this.panel3.Location = new System.Drawing.Point(80, 131);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(43, 45);
            this.panel3.TabIndex = 23;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.savecsv_btn);
            this.panel4.Location = new System.Drawing.Point(182, 131);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(43, 45);
            this.panel4.TabIndex = 25;
            // 
            // savecsv_btn
            // 
            this.savecsv_btn.Image = global::NewGUI.Properties.Resources.csv_mini;
            this.savecsv_btn.Location = new System.Drawing.Point(-25, -25);
            this.savecsv_btn.Name = "savecsv_btn";
            this.savecsv_btn.Size = new System.Drawing.Size(94, 97);
            this.savecsv_btn.TabIndex = 0;
            this.savecsv_btn.UseVisualStyleBackColor = true;
            this.savecsv_btn.Click += new System.EventHandler(this.savecsv_btn_Click);
            // 
            // reset_btn
            // 
            this.reset_btn.Image = global::NewGUI.Properties.Resources.reset_mini_2;
            this.reset_btn.Location = new System.Drawing.Point(-25, -25);
            this.reset_btn.Name = "reset_btn";
            this.reset_btn.Size = new System.Drawing.Size(94, 97);
            this.reset_btn.TabIndex = 0;
            this.reset_btn.UseVisualStyleBackColor = true;
            this.reset_btn.Click += new System.EventHandler(this.reset_btn_Click);
            // 
            // init_btn
            // 
            this.init_btn.Image = global::NewGUI.Properties.Resources.init_mini;
            this.init_btn.Location = new System.Drawing.Point(-25, -25);
            this.init_btn.Name = "init_btn";
            this.init_btn.Size = new System.Drawing.Size(94, 97);
            this.init_btn.TabIndex = 0;
            this.init_btn.UseVisualStyleBackColor = true;
            this.init_btn.Click += new System.EventHandler(this.init_btn_Click);
            // 
            // link_btn
            // 
            this.link_btn.Image = global::NewGUI.Properties.Resources.link_mini;
            this.link_btn.Location = new System.Drawing.Point(-25, -25);
            this.link_btn.Name = "link_btn";
            this.link_btn.Size = new System.Drawing.Size(94, 97);
            this.link_btn.TabIndex = 0;
            this.ToolTipS.SetToolTip(this.link_btn, "Zobrazí výpis inicializace");
            this.link_btn.UseVisualStyleBackColor = true;
            this.link_btn.Click += new System.EventHandler(this.link_btn_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(605, 191);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(140, 140);
            this.pictureBox1.TabIndex = 15;
            this.pictureBox1.TabStop = false;
            // 
            // Senzory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.valueText);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.textPIN3);
            this.Controls.Add(this.textPIN2);
            this.Controls.Add(this.textPIN1);
            this.Controls.Add(this.PIN3);
            this.Controls.Add(this.PIN2);
            this.Controls.Add(this.PIN1);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.badgeConn);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.comboBoxCOM);
            this.Controls.Add(this.chart1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxMode);
            this.Controls.Add(this.comboBoxTIMER);
            this.Controls.Add(this.comboBoxSensor);
            this.Controls.Add(this.ConnectBtn);
            this.Controls.Add(this.button1);
            this.Font = new System.Drawing.Font("Segoe UI Variable Text", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Name = "Senzory";
            this.Size = new System.Drawing.Size(794, 450);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox comboBoxSensor;
        private System.Windows.Forms.ComboBox comboBoxMode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.ComboBox comboBoxCOM;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboBoxTIMER;
        private System.Windows.Forms.Button ConnectBtn;
        private System.Windows.Forms.Label badgeConn;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label PIN1;
        private System.Windows.Forms.Label PIN2;
        private System.Windows.Forms.Label PIN3;
        private System.Windows.Forms.TextBox textPIN1;
        private System.Windows.Forms.TextBox textPIN2;
        private System.Windows.Forms.TextBox textPIN3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button link_btn;
        private System.Windows.Forms.Button init_btn;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label valueText;
        private System.Windows.Forms.ToolTip ToolTipS;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button reset_btn;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button savecsv_btn;
    }
}
