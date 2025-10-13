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
            if (disposing && (components != null))
            {
                components.Dispose();
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.button1 = new System.Windows.Forms.Button();
            this.comboBoxSensor = new System.Windows.Forms.ComboBox();
            this.comboBoxMode = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.comboBoxCOM = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBoxTIMER = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.AktivBox = new System.Windows.Forms.TextBox();
            this.ConnectBtn = new System.Windows.Forms.Button();
            this.badgeConn = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label8 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(108)))), ((int)(((byte)(189)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Segoe UI Variable Text Semibold", 14.25F, System.Drawing.FontStyle.Bold);
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Location = new System.Drawing.Point(30, 121);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(120, 40);
            this.button1.TabIndex = 1;
            this.button1.Text = "Spustit";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // comboBoxSensor
            // 
            this.comboBoxSensor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.comboBoxSensor.FormattingEnabled = true;
            this.comboBoxSensor.IntegralHeight = false;
            this.comboBoxSensor.Location = new System.Drawing.Point(249, 115);
            this.comboBoxSensor.MaxDropDownItems = 10;
            this.comboBoxSensor.Name = "comboBoxSensor";
            this.comboBoxSensor.Size = new System.Drawing.Size(129, 35);
            this.comboBoxSensor.TabIndex = 3;
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
            "INIT",
            "RESET"});
            this.comboBoxMode.Location = new System.Drawing.Point(249, 155);
            this.comboBoxMode.MaxDropDownItems = 5;
            this.comboBoxMode.Name = "comboBoxMode";
            this.comboBoxMode.Size = new System.Drawing.Size(129, 35);
            this.comboBoxMode.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(181, 115);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 27);
            this.label1.TabIndex = 5;
            this.label1.Text = "Senzor";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(180, 155);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 27);
            this.label2.TabIndex = 6;
            this.label2.Text = "Režim";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Variable Text Semibold", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label4.Location = new System.Drawing.Point(400, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(196, 46);
            this.label4.TabIndex = 8;
            this.label4.Text = "Přijatá data";
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
            this.chart1.Location = new System.Drawing.Point(0, 239);
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
            this.chart1.Size = new System.Drawing.Size(441, 211);
            this.chart1.TabIndex = 9;
            this.chart1.Text = "chart1";
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.textBox2.Location = new System.Drawing.Point(406, 55);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox2.Size = new System.Drawing.Size(257, 140);
            this.textBox2.TabIndex = 10;
            // 
            // comboBoxCOM
            // 
            this.comboBoxCOM.AllowDrop = true;
            this.comboBoxCOM.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCOM.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.comboBoxCOM.FormattingEnabled = true;
            this.comboBoxCOM.Location = new System.Drawing.Point(249, 75);
            this.comboBoxCOM.MaxDropDownItems = 5;
            this.comboBoxCOM.Name = "comboBoxCOM";
            this.comboBoxCOM.Size = new System.Drawing.Size(129, 35);
            this.comboBoxCOM.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(180, 195);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(78, 27);
            this.label5.TabIndex = 6;
            this.label5.Text = "Perioda";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(181, 75);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(48, 27);
            this.label6.TabIndex = 12;
            this.label6.Text = "Port";
            // 
            // comboBoxTIMER
            // 
            this.comboBoxTIMER.AllowDrop = true;
            this.comboBoxTIMER.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.comboBoxTIMER.FormattingEnabled = true;
            this.comboBoxTIMER.Items.AddRange(new object[] {
            "10",
            "50",
            "100",
            "250",
            "500",
            "1000"});
            this.comboBoxTIMER.Location = new System.Drawing.Point(249, 195);
            this.comboBoxTIMER.MaxDropDownItems = 5;
            this.comboBoxTIMER.Name = "comboBoxTIMER";
            this.comboBoxTIMER.Size = new System.Drawing.Size(129, 35);
            this.comboBoxTIMER.TabIndex = 3;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI Variable Text Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label7.Location = new System.Drawing.Point(446, 345);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(245, 27);
            this.label7.TabIndex = 8;
            this.label7.Text = "Seznam aktivních senzorů";
            // 
            // AktivBox
            // 
            this.AktivBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.AktivBox.Location = new System.Drawing.Point(450, 369);
            this.AktivBox.Multiline = true;
            this.AktivBox.Name = "AktivBox";
            this.AktivBox.ReadOnly = true;
            this.AktivBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.AktivBox.Size = new System.Drawing.Size(213, 65);
            this.AktivBox.TabIndex = 10;
            // 
            // ConnectBtn
            // 
            this.ConnectBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(108)))), ((int)(((byte)(189)))));
            this.ConnectBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ConnectBtn.Font = new System.Drawing.Font("Segoe UI Variable Text Semibold", 14.25F, System.Drawing.FontStyle.Bold);
            this.ConnectBtn.ForeColor = System.Drawing.Color.White;
            this.ConnectBtn.Location = new System.Drawing.Point(30, 75);
            this.ConnectBtn.Name = "ConnectBtn";
            this.ConnectBtn.Size = new System.Drawing.Size(120, 40);
            this.ConnectBtn.TabIndex = 1;
            this.ConnectBtn.Text = "Připojit";
            this.ConnectBtn.UseVisualStyleBackColor = false;
            this.ConnectBtn.Click += new System.EventHandler(this.ConnectBtn_Click);
            // 
            // badgeConn
            // 
            this.badgeConn.AutoSize = true;
            this.badgeConn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.badgeConn.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.badgeConn.ForeColor = System.Drawing.Color.White;
            this.badgeConn.Location = new System.Drawing.Point(165, 20);
            this.badgeConn.Name = "badgeConn";
            this.badgeConn.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            this.badgeConn.Size = new System.Drawing.Size(146, 35);
            this.badgeConn.TabIndex = 13;
            this.badgeConn.Text = "Nepřipojeno";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Variable Text Semibold", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label3.Location = new System.Drawing.Point(24, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(158, 46);
            this.label3.TabIndex = 14;
            this.label3.Text = "Připojení";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(480, 202);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(140, 140);
            this.pictureBox1.TabIndex = 15;
            this.pictureBox1.TabStop = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI Variable Text", 7F);
            this.label8.ForeColor = System.Drawing.SystemColors.AppWorkspace;
            this.label8.Location = new System.Drawing.Point(450, 436);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(38, 16);
            this.label8.TabIndex = 16;
            this.label8.Text = "label8";
            this.label8.UseMnemonic = false;
            // 
            // Senzory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 27F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label8);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.badgeConn);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.comboBoxCOM);
            this.Controls.Add(this.AktivBox);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.chart1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label4);
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
            this.Size = new System.Drawing.Size(666, 450);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
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
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.ComboBox comboBoxCOM;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboBoxTIMER;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox AktivBox;
        private System.Windows.Forms.Button ConnectBtn;
        private System.Windows.Forms.Label badgeConn;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label8;
    }
}
