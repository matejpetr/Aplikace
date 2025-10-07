namespace NewGUI
{
    partial class Form1
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

        #region Kód generovaný Návrhářem Windows Form

        /// <summary>
        /// Metoda vyžadovaná pro podporu Návrháře - neupravovat
        /// obsah této metody v editoru kódu.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.sidebar = new System.Windows.Forms.FlowLayoutPanel();
            this.menu_panel = new System.Windows.Forms.Panel();
            this.menu_button = new System.Windows.Forms.PictureBox();
            this.home_panel = new System.Windows.Forms.Panel();
            this.home_button = new System.Windows.Forms.Button();
            this.setttings_panel = new System.Windows.Forms.Panel();
            this.setting_button = new System.Windows.Forms.Button();
            this.help_panel = new System.Windows.Forms.Panel();
            this.help_button = new System.Windows.Forms.Button();
            this.info_panel = new System.Windows.Forms.Panel();
            this.info_button = new System.Windows.Forms.Button();
            this.sidebar_timer = new System.Windows.Forms.Timer(this.components);
            this.Main_panel = new System.Windows.Forms.Panel();
            this.sidebar.SuspendLayout();
            this.menu_panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.menu_button)).BeginInit();
            this.home_panel.SuspendLayout();
            this.setttings_panel.SuspendLayout();
            this.help_panel.SuspendLayout();
            this.info_panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // sidebar
            // 
            this.sidebar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.sidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(92)))), ((int)(((byte)(169)))));
            this.sidebar.Controls.Add(this.menu_panel);
            this.sidebar.Controls.Add(this.home_panel);
            this.sidebar.Controls.Add(this.setttings_panel);
            this.sidebar.Controls.Add(this.help_panel);
            this.sidebar.Controls.Add(this.info_panel);
            this.sidebar.Location = new System.Drawing.Point(-5, 0);
            this.sidebar.MaximumSize = new System.Drawing.Size(175, 450);
            this.sidebar.MinimumSize = new System.Drawing.Size(65, 450);
            this.sidebar.Name = "sidebar";
            this.sidebar.Size = new System.Drawing.Size(175, 450);
            this.sidebar.TabIndex = 0;
            // 
            // menu_panel
            // 
            this.menu_panel.BackgroundImage = global::NewGUI.Properties.Resources.MTA_Mini;
            this.menu_panel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.menu_panel.Controls.Add(this.menu_button);
            this.menu_panel.Location = new System.Drawing.Point(3, 3);
            this.menu_panel.Name = "menu_panel";
            this.menu_panel.Size = new System.Drawing.Size(243, 61);
            this.menu_panel.TabIndex = 0;
            // 
            // menu_button
            // 
            this.menu_button.Cursor = System.Windows.Forms.Cursors.Hand;
            this.menu_button.Image = global::NewGUI.Properties.Resources.list;
            this.menu_button.Location = new System.Drawing.Point(9, 9);
            this.menu_button.Name = "menu_button";
            this.menu_button.Size = new System.Drawing.Size(41, 42);
            this.menu_button.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.menu_button.TabIndex = 0;
            this.menu_button.TabStop = false;
            this.menu_button.Click += new System.EventHandler(this.menu_button_Click);
            // 
            // home_panel
            // 
            this.home_panel.Controls.Add(this.home_button);
            this.home_panel.Location = new System.Drawing.Point(3, 70);
            this.home_panel.Name = "home_panel";
            this.home_panel.Size = new System.Drawing.Size(200, 48);
            this.home_panel.TabIndex = 1;
            // 
            // home_button
            // 
            this.home_button.Cursor = System.Windows.Forms.Cursors.Hand;
            this.home_button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.home_button.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.home_button.ForeColor = System.Drawing.Color.White;
            this.home_button.Image = global::NewGUI.Properties.Resources.HOME;
            this.home_button.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.home_button.Location = new System.Drawing.Point(-28, -10);
            this.home_button.Name = "home_button";
            this.home_button.Padding = new System.Windows.Forms.Padding(30, 0, 0, 0);
            this.home_button.Size = new System.Drawing.Size(234, 68);
            this.home_button.TabIndex = 2;
            this.home_button.Text = "          Domů";
            this.home_button.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.home_button.UseVisualStyleBackColor = true;
            this.home_button.Click += new System.EventHandler(this.home_button_Click);
            // 
            // setttings_panel
            // 
            this.setttings_panel.Controls.Add(this.setting_button);
            this.setttings_panel.Location = new System.Drawing.Point(3, 124);
            this.setttings_panel.Name = "setttings_panel";
            this.setttings_panel.Size = new System.Drawing.Size(200, 48);
            this.setttings_panel.TabIndex = 3;
            // 
            // setting_button
            // 
            this.setting_button.Cursor = System.Windows.Forms.Cursors.Hand;
            this.setting_button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.setting_button.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.setting_button.ForeColor = System.Drawing.Color.White;
            this.setting_button.Image = global::NewGUI.Properties.Resources.settings;
            this.setting_button.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.setting_button.Location = new System.Drawing.Point(-27, -10);
            this.setting_button.Name = "setting_button";
            this.setting_button.Padding = new System.Windows.Forms.Padding(30, 0, 0, 0);
            this.setting_button.Size = new System.Drawing.Size(234, 68);
            this.setting_button.TabIndex = 2;
            this.setting_button.Text = "          Nastavení";
            this.setting_button.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.setting_button.UseVisualStyleBackColor = true;
            this.setting_button.Click += new System.EventHandler(this.setting_button_Click);
            // 
            // help_panel
            // 
            this.help_panel.Controls.Add(this.help_button);
            this.help_panel.Location = new System.Drawing.Point(3, 178);
            this.help_panel.Name = "help_panel";
            this.help_panel.Size = new System.Drawing.Size(200, 48);
            this.help_panel.TabIndex = 3;
            // 
            // help_button
            // 
            this.help_button.Cursor = System.Windows.Forms.Cursors.Hand;
            this.help_button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.help_button.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.help_button.ForeColor = System.Drawing.Color.White;
            this.help_button.Image = global::NewGUI.Properties.Resources.help;
            this.help_button.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.help_button.Location = new System.Drawing.Point(-26, -10);
            this.help_button.Name = "help_button";
            this.help_button.Padding = new System.Windows.Forms.Padding(30, 0, 0, 0);
            this.help_button.Size = new System.Drawing.Size(234, 68);
            this.help_button.TabIndex = 2;
            this.help_button.Text = "          Help";
            this.help_button.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.help_button.UseVisualStyleBackColor = true;
            this.help_button.Click += new System.EventHandler(this.help_button_Click);
            // 
            // info_panel
            // 
            this.info_panel.Controls.Add(this.info_button);
            this.info_panel.Location = new System.Drawing.Point(3, 232);
            this.info_panel.Name = "info_panel";
            this.info_panel.Size = new System.Drawing.Size(200, 48);
            this.info_panel.TabIndex = 3;
            // 
            // info_button
            // 
            this.info_button.Cursor = System.Windows.Forms.Cursors.Hand;
            this.info_button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.info_button.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.info_button.ForeColor = System.Drawing.Color.White;
            this.info_button.Image = global::NewGUI.Properties.Resources.info2;
            this.info_button.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.info_button.Location = new System.Drawing.Point(-26, -10);
            this.info_button.Name = "info_button";
            this.info_button.Padding = new System.Windows.Forms.Padding(30, 0, 0, 0);
            this.info_button.Size = new System.Drawing.Size(234, 68);
            this.info_button.TabIndex = 2;
            this.info_button.Text = "          O nás";
            this.info_button.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.info_button.UseVisualStyleBackColor = true;
            this.info_button.Click += new System.EventHandler(this.info_button_Click);
            // 
            // sidebar_timer
            // 
            this.sidebar_timer.Interval = 10;
            // 
            // Main_panel
            // 
            this.Main_panel.Location = new System.Drawing.Point(166, 0);
            this.Main_panel.Name = "Main_panel";
            this.Main_panel.Size = new System.Drawing.Size(666, 450);
            this.Main_panel.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(833, 450);
            this.Controls.Add(this.sidebar);
            this.Controls.Add(this.Main_panel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Moravskoslezská technologická akademie";
            this.sidebar.ResumeLayout(false);
            this.menu_panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.menu_button)).EndInit();
            this.home_panel.ResumeLayout(false);
            this.setttings_panel.ResumeLayout(false);
            this.help_panel.ResumeLayout(false);
            this.info_panel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel sidebar;
        private System.Windows.Forms.Timer sidebar_timer;
        private System.Windows.Forms.Panel menu_panel;
        private System.Windows.Forms.Panel home_panel;
        private System.Windows.Forms.Button home_button;
        private System.Windows.Forms.Panel setttings_panel;
        private System.Windows.Forms.Button setting_button;
        private System.Windows.Forms.Panel help_panel;
        private System.Windows.Forms.Button help_button;
        private System.Windows.Forms.Panel info_panel;
        private System.Windows.Forms.Button info_button;
        private System.Windows.Forms.Panel Main_panel;
        private System.Windows.Forms.PictureBox menu_button;
    }
}

