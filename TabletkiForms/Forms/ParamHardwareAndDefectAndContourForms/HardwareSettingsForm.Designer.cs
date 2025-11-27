namespace KrishkiForms.Forms.ParamHardwareAndDefectAndContourForms
{
    partial class HardwareSettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
            pr205Group = new GroupBox();
            tabControl4 = new TabControl();
            prBreakingSettings = new TabPage();
            breakingAllowCb = new CheckBox();
            breakerOffsetTb = new TextBox();
            label25 = new Label();
            applyPrBreakerParamButton = new Button();
            label31 = new Label();
            breakingTimeTb = new TextBox();
            cameraOffsetTb = new TextBox();
            label27 = new Label();
            tabControl3 = new TabControl();
            tabPrNetworkSettings = new TabPage();
            prIpTextBox = new TextBox();
            label40 = new Label();
            connectPrButton = new Button();
            label26 = new Label();
            pr205PortTb = new TextBox();
            label30 = new Label();
            prStatus = new Label();
            savePrSettings = new Button();
            loadPrSettings = new Button();
            connectCameraButton = new Button();
            loadSettingsButton = new Button();
            saveSettingsButton = new Button();
            cameraParamsPanel = new Panel();
            gainTb = new TextBox();
            label6 = new Label();
            exposureTb = new TextBox();
            label5 = new Label();
            widthTb = new TextBox();
            label4 = new Label();
            heightTb = new TextBox();
            label3 = new Label();
            applyCameraSettingsButton = new Button();
            groupBox1 = new GroupBox();
            label15 = new Label();
            camStatus = new Label();
            cameraSettingsPb = new PictureBox();
            foldersGroup = new GroupBox();
            browseUnderfillButton = new Button();
            underfillPathTextBox = new TextBox();
            label13 = new Label();
            applyFoldersButton = new Button();
            browseObloyButton = new Button();
            obloyPathTextBox = new TextBox();
            label12 = new Label();
            browseInpaintButton = new Button();
            inpaintPathTextBox = new TextBox();
            label11 = new Label();
            browseInclusionButton = new Button();
            inclusionPathTextBox = new TextBox();
            label10 = new Label();
            browseOvalityButton = new Button();
            ovalityPathTextBox = new TextBox();
            label9 = new Label();
            browseOriginalButton = new Button();
            originalPathTextBox = new TextBox();
            label8 = new Label();
            pr205Group.SuspendLayout();
            tabControl4.SuspendLayout();
            prBreakingSettings.SuspendLayout();
            tabControl3.SuspendLayout();
            tabPrNetworkSettings.SuspendLayout();
            cameraParamsPanel.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cameraSettingsPb).BeginInit();
            foldersGroup.SuspendLayout();
            SuspendLayout();
            // 
            // pr205Group
            // 
            pr205Group.BackColor = Color.White;
            pr205Group.Controls.Add(tabControl4);
            pr205Group.Controls.Add(tabControl3);
            pr205Group.Controls.Add(savePrSettings);
            pr205Group.Controls.Add(loadPrSettings);
            pr205Group.FlatStyle = FlatStyle.Flat;
            pr205Group.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 204);
            pr205Group.ForeColor = Color.FromArgb(4, 85, 191);
            pr205Group.Location = new Point(8, 8);
            pr205Group.Name = "pr205Group";
            pr205Group.Size = new Size(412, 534);
            pr205Group.TabIndex = 2;
            pr205Group.TabStop = false;
            pr205Group.Text = "Настройки ПР205";
            // 
            // tabControl4
            // 
            tabControl4.Controls.Add(prBreakingSettings);
            tabControl4.Location = new Point(7, 215);
            tabControl4.Name = "tabControl4";
            tabControl4.SelectedIndex = 0;
            tabControl4.Size = new Size(398, 233);
            tabControl4.TabIndex = 34;
            // 
            // prBreakingSettings
            // 
            prBreakingSettings.Controls.Add(breakingAllowCb);
            prBreakingSettings.Controls.Add(breakerOffsetTb);
            prBreakingSettings.Controls.Add(label25);
            prBreakingSettings.Controls.Add(applyPrBreakerParamButton);
            prBreakingSettings.Controls.Add(label31);
            prBreakingSettings.Controls.Add(breakingTimeTb);
            prBreakingSettings.Controls.Add(cameraOffsetTb);
            prBreakingSettings.Controls.Add(label27);
            prBreakingSettings.Font = new Font("Segoe UI", 9F, FontStyle.Italic, GraphicsUnit.Point, 204);
            prBreakingSettings.ForeColor = Color.Black;
            prBreakingSettings.Location = new Point(4, 25);
            prBreakingSettings.Name = "prBreakingSettings";
            prBreakingSettings.Padding = new Padding(3);
            prBreakingSettings.Size = new Size(390, 204);
            prBreakingSettings.TabIndex = 0;
            prBreakingSettings.Text = "Настройки отбраковки";
            prBreakingSettings.UseVisualStyleBackColor = true;
            // 
            // breakingAllowCb
            // 
            breakingAllowCb.AutoSize = true;
            breakingAllowCb.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            breakingAllowCb.ForeColor = Color.Black;
            breakingAllowCb.Location = new Point(15, 21);
            breakingAllowCb.Name = "breakingAllowCb";
            breakingAllowCb.Size = new Size(116, 19);
            breakingAllowCb.TabIndex = 17;
            breakingAllowCb.Text = "Включить обдув";
            breakingAllowCb.UseVisualStyleBackColor = true;
            breakingAllowCb.CheckedChanged += breakingAllowCb_CheckedChanged;
            // 
            // breakerOffsetTb
            // 
            breakerOffsetTb.BackColor = Color.FromArgb(240, 245, 255);
            breakerOffsetTb.BorderStyle = BorderStyle.FixedSingle;
            breakerOffsetTb.Font = new Font("Segoe UI", 9F);
            breakerOffsetTb.Location = new Point(308, 96);
            breakerOffsetTb.Name = "breakerOffsetTb";
            breakerOffsetTb.Size = new Size(53, 23);
            breakerOffsetTb.TabIndex = 21;
            breakerOffsetTb.Text = "2430";
            // 
            // label25
            // 
            label25.AutoSize = true;
            label25.Font = new Font("Segoe UI", 9F);
            label25.ForeColor = Color.Black;
            label25.Location = new Point(155, 25);
            label25.Name = "label25";
            label25.Size = new Size(86, 15);
            label25.TabIndex = 15;
            label25.Text = "Время обдува:";
            // 
            // applyPrBreakerParamButton
            // 
            applyPrBreakerParamButton.BackColor = Color.FromArgb(4, 85, 191);
            applyPrBreakerParamButton.FlatAppearance.BorderSize = 0;
            applyPrBreakerParamButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            applyPrBreakerParamButton.ForeColor = Color.White;
            applyPrBreakerParamButton.Location = new Point(59, 142);
            applyPrBreakerParamButton.Name = "applyPrBreakerParamButton";
            applyPrBreakerParamButton.Size = new Size(280, 43);
            applyPrBreakerParamButton.TabIndex = 2;
            applyPrBreakerParamButton.Text = "Применить настройки";
            applyPrBreakerParamButton.UseVisualStyleBackColor = false;
            applyPrBreakerParamButton.Click += applyPrBreakerParamButton_Click;
            // 
            // label31
            // 
            label31.AutoSize = true;
            label31.Font = new Font("Segoe UI", 9F);
            label31.ForeColor = Color.Black;
            label31.Location = new Point(13, 101);
            label31.Name = "label31";
            label31.Size = new Size(222, 15);
            label31.TabIndex = 20;
            label31.Text = "Расстояние от датчика до обдува, тики:";
            // 
            // breakingTimeTb
            // 
            breakingTimeTb.BackColor = Color.FromArgb(240, 245, 255);
            breakingTimeTb.BorderStyle = BorderStyle.FixedSingle;
            breakingTimeTb.Font = new Font("Segoe UI", 9F);
            breakingTimeTb.Location = new Point(304, 15);
            breakingTimeTb.Name = "breakingTimeTb";
            breakingTimeTb.Size = new Size(57, 23);
            breakingTimeTb.TabIndex = 16;
            breakingTimeTb.Text = "55";
            // 
            // cameraOffsetTb
            // 
            cameraOffsetTb.BackColor = Color.FromArgb(240, 245, 255);
            cameraOffsetTb.BorderStyle = BorderStyle.FixedSingle;
            cameraOffsetTb.Font = new Font("Segoe UI", 9F);
            cameraOffsetTb.Location = new Point(306, 54);
            cameraOffsetTb.Name = "cameraOffsetTb";
            cameraOffsetTb.Size = new Size(55, 23);
            cameraOffsetTb.TabIndex = 19;
            cameraOffsetTb.Text = "300";
            // 
            // label27
            // 
            label27.AutoSize = true;
            label27.Font = new Font("Segoe UI", 9F);
            label27.ForeColor = Color.Black;
            label27.Location = new Point(14, 59);
            label27.Name = "label27";
            label27.Size = new Size(227, 15);
            label27.TabIndex = 18;
            label27.Text = "Расстояние от датчика до камеры, тики:";
            // 
            // tabControl3
            // 
            tabControl3.Controls.Add(tabPrNetworkSettings);
            tabControl3.Location = new Point(7, 36);
            tabControl3.Name = "tabControl3";
            tabControl3.SelectedIndex = 0;
            tabControl3.Size = new Size(398, 167);
            tabControl3.TabIndex = 33;
            // 
            // tabPrNetworkSettings
            // 
            tabPrNetworkSettings.Controls.Add(prIpTextBox);
            tabPrNetworkSettings.Controls.Add(label40);
            tabPrNetworkSettings.Controls.Add(connectPrButton);
            tabPrNetworkSettings.Controls.Add(label26);
            tabPrNetworkSettings.Controls.Add(pr205PortTb);
            tabPrNetworkSettings.Controls.Add(label30);
            tabPrNetworkSettings.Controls.Add(prStatus);
            tabPrNetworkSettings.Font = new Font("Segoe UI", 9F, FontStyle.Italic, GraphicsUnit.Point, 204);
            tabPrNetworkSettings.Location = new Point(4, 25);
            tabPrNetworkSettings.Name = "tabPrNetworkSettings";
            tabPrNetworkSettings.Padding = new Padding(3);
            tabPrNetworkSettings.Size = new Size(390, 138);
            tabPrNetworkSettings.TabIndex = 0;
            tabPrNetworkSettings.Text = "Сетевые настройки ПР205";
            tabPrNetworkSettings.UseVisualStyleBackColor = true;
            // 
            // prIpTextBox
            // 
            prIpTextBox.BackColor = Color.FromArgb(240, 245, 255);
            prIpTextBox.Font = new Font("Segoe UI", 9F);
            prIpTextBox.Location = new Point(136, 90);
            prIpTextBox.Name = "prIpTextBox";
            prIpTextBox.Size = new Size(94, 23);
            prIpTextBox.TabIndex = 31;
            prIpTextBox.Text = "10.10.69.38";
            // 
            // label40
            // 
            label40.AutoSize = true;
            label40.Font = new Font("Segoe UI", 9F);
            label40.ForeColor = Color.Black;
            label40.Location = new Point(13, 100);
            label40.Name = "label40";
            label40.Size = new Size(95, 15);
            label40.TabIndex = 30;
            label40.Text = "Ip адрес обдува:";
            // 
            // connectPrButton
            // 
            connectPrButton.BackColor = Color.FromArgb(4, 85, 191);
            connectPrButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);
            connectPrButton.ForeColor = SystemColors.Control;
            connectPrButton.Location = new Point(254, 26);
            connectPrButton.Name = "connectPrButton";
            connectPrButton.Size = new Size(128, 93);
            connectPrButton.TabIndex = 32;
            connectPrButton.Text = "Подключиться к ПР";
            connectPrButton.UseVisualStyleBackColor = false;
            connectPrButton.Click += connectPrButton_Click;
            // 
            // label26
            // 
            label26.AutoSize = true;
            label26.Font = new Font("Segoe UI", 9F);
            label26.ForeColor = Color.Black;
            label26.Location = new Point(13, 62);
            label26.Name = "label26";
            label26.Size = new Size(75, 15);
            label26.TabIndex = 13;
            label26.Text = "Порт ПР205:";
            // 
            // pr205PortTb
            // 
            pr205PortTb.BackColor = Color.FromArgb(240, 245, 255);
            pr205PortTb.BorderStyle = BorderStyle.FixedSingle;
            pr205PortTb.Font = new Font("Segoe UI", 9F);
            pr205PortTb.Location = new Point(136, 52);
            pr205PortTb.Name = "pr205PortTb";
            pr205PortTb.Size = new Size(94, 23);
            pr205PortTb.TabIndex = 14;
            pr205PortTb.Text = "502";
            // 
            // label30
            // 
            label30.AutoSize = true;
            label30.Font = new Font("Segoe UI", 8.25F);
            label30.ForeColor = Color.Black;
            label30.Location = new Point(10, 27);
            label30.Name = "label30";
            label30.Size = new Size(78, 13);
            label30.TabIndex = 12;
            label30.Text = "Статус ПР205:";
            // 
            // prStatus
            // 
            prStatus.AutoSize = true;
            prStatus.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            prStatus.ForeColor = Color.Black;
            prStatus.Location = new Point(139, 27);
            prStatus.Name = "prStatus";
            prStatus.Size = new Size(94, 13);
            prStatus.TabIndex = 11;
            prStatus.Text = "Не подключена";
            // 
            // savePrSettings
            // 
            savePrSettings.BackColor = Color.FromArgb(66, 133, 244);
            savePrSettings.FlatAppearance.BorderSize = 0;
            savePrSettings.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            savePrSettings.ForeColor = Color.White;
            savePrSettings.Location = new Point(221, 463);
            savePrSettings.Name = "savePrSettings";
            savePrSettings.Size = new Size(129, 52);
            savePrSettings.TabIndex = 1;
            savePrSettings.Text = "Сохранить";
            savePrSettings.UseVisualStyleBackColor = false;
            savePrSettings.Click += savePrSettings_Click;
            // 
            // loadPrSettings
            // 
            loadPrSettings.BackColor = Color.FromArgb(66, 133, 244);
            loadPrSettings.FlatAppearance.BorderSize = 0;
            loadPrSettings.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            loadPrSettings.ForeColor = Color.White;
            loadPrSettings.Location = new Point(70, 463);
            loadPrSettings.Name = "loadPrSettings";
            loadPrSettings.Size = new Size(134, 52);
            loadPrSettings.TabIndex = 0;
            loadPrSettings.Text = "Загрузить настройки";
            loadPrSettings.UseVisualStyleBackColor = false;
            loadPrSettings.Click += loadPrSettings_Click;
            // 
            // connectCameraButton
            // 
            connectCameraButton.BackColor = Color.FromArgb(4, 85, 191);
            connectCameraButton.FlatAppearance.BorderSize = 0;
            connectCameraButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            connectCameraButton.ForeColor = Color.White;
            connectCameraButton.Location = new Point(160, 37);
            connectCameraButton.Name = "connectCameraButton";
            connectCameraButton.Size = new Size(129, 38);
            connectCameraButton.TabIndex = 6;
            connectCameraButton.Text = "Подключиться";
            connectCameraButton.UseVisualStyleBackColor = false;
            connectCameraButton.Click += connectCameraButton_Click;
            // 
            // loadSettingsButton
            // 
            loadSettingsButton.BackColor = Color.FromArgb(66, 133, 244);
            loadSettingsButton.FlatAppearance.BorderSize = 0;
            loadSettingsButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            loadSettingsButton.ForeColor = Color.White;
            loadSettingsButton.Location = new Point(12, 459);
            loadSettingsButton.Name = "loadSettingsButton";
            loadSettingsButton.Size = new Size(134, 52);
            loadSettingsButton.TabIndex = 0;
            loadSettingsButton.Text = "Загрузить настройки";
            loadSettingsButton.UseVisualStyleBackColor = false;
            loadSettingsButton.Click += loadSettingsButton_Click;
            // 
            // saveSettingsButton
            // 
            saveSettingsButton.BackColor = Color.FromArgb(66, 133, 244);
            saveSettingsButton.FlatAppearance.BorderSize = 0;
            saveSettingsButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            saveSettingsButton.ForeColor = Color.White;
            saveSettingsButton.Location = new Point(161, 459);
            saveSettingsButton.Name = "saveSettingsButton";
            saveSettingsButton.Size = new Size(129, 52);
            saveSettingsButton.TabIndex = 1;
            saveSettingsButton.Text = "Сохранить";
            saveSettingsButton.UseVisualStyleBackColor = false;
            saveSettingsButton.Click += saveSettingsButton_Click;
            // 
            // cameraParamsPanel
            // 
            cameraParamsPanel.BackColor = Color.FromArgb(240, 245, 255);
            cameraParamsPanel.BorderStyle = BorderStyle.Fixed3D;
            cameraParamsPanel.Controls.Add(gainTb);
            cameraParamsPanel.Controls.Add(label6);
            cameraParamsPanel.Controls.Add(exposureTb);
            cameraParamsPanel.Controls.Add(label5);
            cameraParamsPanel.Controls.Add(widthTb);
            cameraParamsPanel.Controls.Add(label4);
            cameraParamsPanel.Controls.Add(heightTb);
            cameraParamsPanel.Controls.Add(label3);
            cameraParamsPanel.Location = new Point(9, 86);
            cameraParamsPanel.Name = "cameraParamsPanel";
            cameraParamsPanel.Size = new Size(280, 125);
            cameraParamsPanel.TabIndex = 3;
            // 
            // gainTb
            // 
            gainTb.BackColor = Color.White;
            gainTb.BorderStyle = BorderStyle.FixedSingle;
            gainTb.Font = new Font("Segoe UI", 9F);
            gainTb.Location = new Point(150, 83);
            gainTb.Name = "gainTb";
            gainTb.Size = new Size(100, 23);
            gainTb.TabIndex = 7;
            gainTb.Text = "3.01";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 9F);
            label6.ForeColor = Color.Black;
            label6.Location = new Point(150, 63);
            label6.Name = "label6";
            label6.Size = new Size(95, 15);
            label6.TabIndex = 6;
            label6.Text = "Насыщенность:";
            // 
            // exposureTb
            // 
            exposureTb.BackColor = Color.White;
            exposureTb.BorderStyle = BorderStyle.FixedSingle;
            exposureTb.Font = new Font("Segoe UI", 9F);
            exposureTb.Location = new Point(20, 83);
            exposureTb.Name = "exposureTb";
            exposureTb.Size = new Size(100, 23);
            exposureTb.TabIndex = 5;
            exposureTb.Text = "450";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 9F);
            label5.ForeColor = Color.Black;
            label5.Location = new Point(20, 63);
            label5.Name = "label5";
            label5.Size = new Size(75, 15);
            label5.TabIndex = 4;
            label5.Text = "Экспозиция:";
            // 
            // widthTb
            // 
            widthTb.BackColor = Color.White;
            widthTb.BorderStyle = BorderStyle.FixedSingle;
            widthTb.Font = new Font("Segoe UI", 9F);
            widthTb.Location = new Point(150, 30);
            widthTb.Name = "widthTb";
            widthTb.Size = new Size(100, 23);
            widthTb.TabIndex = 3;
            widthTb.Text = "500";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 9F);
            label4.ForeColor = Color.Black;
            label4.Location = new Point(150, 10);
            label4.Name = "label4";
            label4.Size = new Size(55, 15);
            label4.TabIndex = 2;
            label4.Text = "Ширина:";
            // 
            // heightTb
            // 
            heightTb.BackColor = Color.White;
            heightTb.BorderStyle = BorderStyle.FixedSingle;
            heightTb.Font = new Font("Segoe UI", 9F);
            heightTb.Location = new Point(20, 30);
            heightTb.Name = "heightTb";
            heightTb.Size = new Size(100, 23);
            heightTb.TabIndex = 1;
            heightTb.Text = "532";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F);
            label3.ForeColor = Color.Black;
            label3.Location = new Point(20, 10);
            label3.Name = "label3";
            label3.Size = new Size(50, 15);
            label3.TabIndex = 0;
            label3.Text = "Высота:";
            // 
            // applyCameraSettingsButton
            // 
            applyCameraSettingsButton.BackColor = Color.FromArgb(4, 85, 191);
            applyCameraSettingsButton.FlatAppearance.BorderSize = 0;
            applyCameraSettingsButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            applyCameraSettingsButton.ForeColor = Color.White;
            applyCameraSettingsButton.Location = new Point(9, 217);
            applyCameraSettingsButton.Name = "applyCameraSettingsButton";
            applyCameraSettingsButton.Size = new Size(280, 43);
            applyCameraSettingsButton.TabIndex = 2;
            applyCameraSettingsButton.Text = "Применить настройки";
            applyCameraSettingsButton.UseVisualStyleBackColor = false;
            applyCameraSettingsButton.Click += applySettingsButton_Click;
            // 
            // groupBox1
            // 
            groupBox1.BackColor = Color.White;
            groupBox1.Controls.Add(label15);
            groupBox1.Controls.Add(camStatus);
            groupBox1.Controls.Add(cameraSettingsPb);
            groupBox1.Controls.Add(connectCameraButton);
            groupBox1.Controls.Add(saveSettingsButton);
            groupBox1.Controls.Add(loadSettingsButton);
            groupBox1.Controls.Add(applyCameraSettingsButton);
            groupBox1.Controls.Add(cameraParamsPanel);
            groupBox1.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 204);
            groupBox1.ForeColor = Color.FromArgb(4, 85, 191);
            groupBox1.Location = new Point(426, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(302, 528);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "Настройки камеры";
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Font = new Font("Segoe UI", 8.25F);
            label15.ForeColor = Color.Black;
            label15.Location = new Point(19, 33);
            label15.Name = "label15";
            label15.Size = new Size(87, 13);
            label15.TabIndex = 8;
            label15.Text = "Статус камеры:";
            // 
            // camStatus
            // 
            camStatus.AutoSize = true;
            camStatus.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            camStatus.ForeColor = Color.Black;
            camStatus.Location = new Point(19, 55);
            camStatus.Name = "camStatus";
            camStatus.Size = new Size(94, 13);
            camStatus.TabIndex = 9;
            camStatus.Text = "Не подключена";
            // 
            // cameraSettingsPb
            // 
            cameraSettingsPb.BorderStyle = BorderStyle.Fixed3D;
            cameraSettingsPb.Location = new Point(9, 266);
            cameraSettingsPb.Name = "cameraSettingsPb";
            cameraSettingsPb.Size = new Size(280, 174);
            cameraSettingsPb.TabIndex = 7;
            cameraSettingsPb.TabStop = false;
            // 
            // foldersGroup
            // 
            foldersGroup.BackColor = Color.White;
            foldersGroup.Controls.Add(browseUnderfillButton);
            foldersGroup.Controls.Add(underfillPathTextBox);
            foldersGroup.Controls.Add(label13);
            foldersGroup.Controls.Add(applyFoldersButton);
            foldersGroup.Controls.Add(browseObloyButton);
            foldersGroup.Controls.Add(obloyPathTextBox);
            foldersGroup.Controls.Add(label12);
            foldersGroup.Controls.Add(browseInpaintButton);
            foldersGroup.Controls.Add(inpaintPathTextBox);
            foldersGroup.Controls.Add(label11);
            foldersGroup.Controls.Add(browseInclusionButton);
            foldersGroup.Controls.Add(inclusionPathTextBox);
            foldersGroup.Controls.Add(label10);
            foldersGroup.Controls.Add(browseOvalityButton);
            foldersGroup.Controls.Add(ovalityPathTextBox);
            foldersGroup.Controls.Add(label9);
            foldersGroup.Controls.Add(browseOriginalButton);
            foldersGroup.Controls.Add(originalPathTextBox);
            foldersGroup.Controls.Add(label8);
            foldersGroup.FlatStyle = FlatStyle.Flat;
            foldersGroup.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 204);
            foldersGroup.ForeColor = Color.FromArgb(4, 85, 191);
            foldersGroup.Location = new Point(734, 8);
            foldersGroup.Name = "foldersGroup";
            foldersGroup.Size = new Size(418, 532);
            foldersGroup.TabIndex = 8;
            foldersGroup.TabStop = false;
            foldersGroup.Text = "Настройки папок";
            // 
            // browseUnderfillButton
            // 
            browseUnderfillButton.BackColor = Color.FromArgb(66, 133, 244);
            browseUnderfillButton.FlatAppearance.BorderSize = 0;
            browseUnderfillButton.FlatStyle = FlatStyle.Popup;
            browseUnderfillButton.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            browseUnderfillButton.ForeColor = Color.White;
            browseUnderfillButton.Location = new Point(361, 294);
            browseUnderfillButton.Name = "browseUnderfillButton";
            browseUnderfillButton.Size = new Size(40, 23);
            browseUnderfillButton.TabIndex = 18;
            browseUnderfillButton.Text = "...";
            browseUnderfillButton.UseVisualStyleBackColor = false;
            browseUnderfillButton.Click += browseUnderfillButton_Click;
            // 
            // underfillPathTextBox
            // 
            underfillPathTextBox.BackColor = Color.FromArgb(240, 245, 255);
            underfillPathTextBox.BorderStyle = BorderStyle.FixedSingle;
            underfillPathTextBox.Font = new Font("Segoe UI", 8F);
            underfillPathTextBox.Location = new Point(8, 295);
            underfillPathTextBox.Name = "underfillPathTextBox";
            underfillPathTextBox.Size = new Size(347, 22);
            underfillPathTextBox.TabIndex = 17;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label13.ForeColor = Color.Black;
            label13.Location = new Point(8, 275);
            label13.Name = "label13";
            label13.Size = new Size(103, 15);
            label13.TabIndex = 16;
            label13.Text = "Неполный залив:";
            // 
            // applyFoldersButton
            // 
            applyFoldersButton.BackColor = Color.FromArgb(4, 85, 191);
            applyFoldersButton.FlatAppearance.BorderSize = 0;
            applyFoldersButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);
            applyFoldersButton.ForeColor = Color.White;
            applyFoldersButton.Location = new Point(75, 465);
            applyFoldersButton.Name = "applyFoldersButton";
            applyFoldersButton.Size = new Size(280, 50);
            applyFoldersButton.TabIndex = 0;
            applyFoldersButton.Text = "Применить настройки";
            applyFoldersButton.UseVisualStyleBackColor = false;
            // 
            // browseObloyButton
            // 
            browseObloyButton.BackColor = Color.FromArgb(66, 133, 244);
            browseObloyButton.FlatAppearance.BorderSize = 0;
            browseObloyButton.FlatStyle = FlatStyle.Popup;
            browseObloyButton.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            browseObloyButton.ForeColor = Color.White;
            browseObloyButton.Location = new Point(361, 234);
            browseObloyButton.Name = "browseObloyButton";
            browseObloyButton.Size = new Size(40, 23);
            browseObloyButton.TabIndex = 15;
            browseObloyButton.Text = "...";
            browseObloyButton.UseVisualStyleBackColor = false;
            browseObloyButton.Click += browseObloyButton_Click;
            // 
            // obloyPathTextBox
            // 
            obloyPathTextBox.BackColor = Color.FromArgb(240, 245, 255);
            obloyPathTextBox.BorderStyle = BorderStyle.FixedSingle;
            obloyPathTextBox.Font = new Font("Segoe UI", 8F);
            obloyPathTextBox.Location = new Point(8, 235);
            obloyPathTextBox.Name = "obloyPathTextBox";
            obloyPathTextBox.Size = new Size(347, 22);
            obloyPathTextBox.TabIndex = 14;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label12.ForeColor = Color.Black;
            label12.Location = new Point(8, 215);
            label12.Name = "label12";
            label12.Size = new Size(47, 15);
            label12.TabIndex = 13;
            label12.Text = "Облой:";
            // 
            // browseInpaintButton
            // 
            browseInpaintButton.BackColor = Color.FromArgb(66, 133, 244);
            browseInpaintButton.FlatAppearance.BorderSize = 0;
            browseInpaintButton.FlatStyle = FlatStyle.Popup;
            browseInpaintButton.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            browseInpaintButton.ForeColor = Color.White;
            browseInpaintButton.Location = new Point(361, 174);
            browseInpaintButton.Name = "browseInpaintButton";
            browseInpaintButton.Size = new Size(40, 23);
            browseInpaintButton.TabIndex = 12;
            browseInpaintButton.Text = "...";
            browseInpaintButton.UseVisualStyleBackColor = false;
            browseInpaintButton.Click += browseInpaintButton_Click;
            // 
            // inpaintPathTextBox
            // 
            inpaintPathTextBox.BackColor = Color.FromArgb(240, 245, 255);
            inpaintPathTextBox.BorderStyle = BorderStyle.FixedSingle;
            inpaintPathTextBox.Font = new Font("Segoe UI", 8F);
            inpaintPathTextBox.Location = new Point(8, 175);
            inpaintPathTextBox.Name = "inpaintPathTextBox";
            inpaintPathTextBox.Size = new Size(347, 22);
            inpaintPathTextBox.TabIndex = 11;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label11.ForeColor = Color.Black;
            label11.Location = new Point(8, 155);
            label11.Name = "label11";
            label11.Size = new Size(80, 15);
            label11.TabIndex = 10;
            label11.Text = "Непрокрасы:";
            // 
            // browseInclusionButton
            // 
            browseInclusionButton.BackColor = Color.FromArgb(66, 133, 244);
            browseInclusionButton.FlatAppearance.BorderSize = 0;
            browseInclusionButton.FlatStyle = FlatStyle.Popup;
            browseInclusionButton.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            browseInclusionButton.ForeColor = Color.White;
            browseInclusionButton.Location = new Point(361, 114);
            browseInclusionButton.Name = "browseInclusionButton";
            browseInclusionButton.Size = new Size(40, 23);
            browseInclusionButton.TabIndex = 9;
            browseInclusionButton.Text = "...";
            browseInclusionButton.UseVisualStyleBackColor = false;
            browseInclusionButton.Click += browseInclusionButton_Click;
            // 
            // inclusionPathTextBox
            // 
            inclusionPathTextBox.BackColor = Color.FromArgb(240, 245, 255);
            inclusionPathTextBox.BorderStyle = BorderStyle.FixedSingle;
            inclusionPathTextBox.Font = new Font("Segoe UI", 8F);
            inclusionPathTextBox.Location = new Point(8, 115);
            inclusionPathTextBox.Name = "inclusionPathTextBox";
            inclusionPathTextBox.Size = new Size(347, 22);
            inclusionPathTextBox.TabIndex = 8;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label10.ForeColor = Color.Black;
            label10.Location = new Point(8, 95);
            label10.Name = "label10";
            label10.Size = new Size(76, 15);
            label10.TabIndex = 7;
            label10.Text = "Вкрапления:";
            // 
            // browseOvalityButton
            // 
            browseOvalityButton.BackColor = Color.FromArgb(66, 133, 244);
            browseOvalityButton.FlatAppearance.BorderSize = 0;
            browseOvalityButton.FlatStyle = FlatStyle.Popup;
            browseOvalityButton.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            browseOvalityButton.ForeColor = Color.White;
            browseOvalityButton.Location = new Point(361, 54);
            browseOvalityButton.Name = "browseOvalityButton";
            browseOvalityButton.Size = new Size(40, 23);
            browseOvalityButton.TabIndex = 6;
            browseOvalityButton.Text = "...";
            browseOvalityButton.UseVisualStyleBackColor = false;
            browseOvalityButton.Click += browseOvalityButton_Click;
            // 
            // ovalityPathTextBox
            // 
            ovalityPathTextBox.BackColor = Color.FromArgb(240, 245, 255);
            ovalityPathTextBox.BorderStyle = BorderStyle.FixedSingle;
            ovalityPathTextBox.Font = new Font("Segoe UI", 8F);
            ovalityPathTextBox.Location = new Point(8, 55);
            ovalityPathTextBox.Name = "ovalityPathTextBox";
            ovalityPathTextBox.Size = new Size(347, 22);
            ovalityPathTextBox.TabIndex = 5;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label9.ForeColor = Color.Black;
            label9.Location = new Point(8, 35);
            label9.Name = "label9";
            label9.Size = new Size(75, 15);
            label9.TabIndex = 4;
            label9.Text = "Овальность:";
            // 
            // browseOriginalButton
            // 
            browseOriginalButton.BackColor = Color.FromArgb(66, 133, 244);
            browseOriginalButton.FlatAppearance.BorderSize = 0;
            browseOriginalButton.FlatStyle = FlatStyle.Popup;
            browseOriginalButton.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            browseOriginalButton.ForeColor = Color.White;
            browseOriginalButton.Location = new Point(361, 360);
            browseOriginalButton.Name = "browseOriginalButton";
            browseOriginalButton.Size = new Size(40, 23);
            browseOriginalButton.TabIndex = 3;
            browseOriginalButton.Text = "...";
            browseOriginalButton.UseVisualStyleBackColor = false;
            browseOriginalButton.Click += browseOriginalButton_Click;
            // 
            // originalPathTextBox
            // 
            originalPathTextBox.BackColor = Color.FromArgb(240, 245, 255);
            originalPathTextBox.BorderStyle = BorderStyle.FixedSingle;
            originalPathTextBox.Font = new Font("Segoe UI", 8F);
            originalPathTextBox.Location = new Point(8, 361);
            originalPathTextBox.Name = "originalPathTextBox";
            originalPathTextBox.Size = new Size(347, 22);
            originalPathTextBox.TabIndex = 2;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label8.ForeColor = Color.Black;
            label8.Location = new Point(8, 341);
            label8.Name = "label8";
            label8.Size = new Size(111, 15);
            label8.TabIndex = 1;
            label8.Text = "Исходные снимки:";
            // 
            // HardwareSettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1164, 552);
            Controls.Add(foldersGroup);
            Controls.Add(groupBox1);
            Controls.Add(pr205Group);
            Name = "HardwareSettingsForm";
            Text = "HardwareSettingsForm";
            pr205Group.ResumeLayout(false);
            tabControl4.ResumeLayout(false);
            prBreakingSettings.ResumeLayout(false);
            prBreakingSettings.PerformLayout();
            tabControl3.ResumeLayout(false);
            tabPrNetworkSettings.ResumeLayout(false);
            tabPrNetworkSettings.PerformLayout();
            cameraParamsPanel.ResumeLayout(false);
            cameraParamsPanel.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)cameraSettingsPb).EndInit();
            foldersGroup.ResumeLayout(false);
            foldersGroup.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox pr205Group;
        private TabControl tabControl4;
        private TabPage prBreakingSettings;
        private CheckBox breakingAllowCb;
        private TextBox breakerOffsetTb;
        private Label label25;
        private Button applyPrBreakerParamButton;
        private Label label31;
        private TextBox breakingTimeTb;
        private TextBox cameraOffsetTb;
        private Label label27;
        private TabControl tabControl3;
        private TabPage tabPrNetworkSettings;
        private TextBox prIpTextBox;
        private Label label40;
        private Button connectPrButton;
        private Label label26;
        private TextBox pr205PortTb;
        private Label label30;
        private Label prStatus;
        private Button savePrSettings;
        private Button loadPrSettings;
        private Button connectCameraButton;
        private Button loadSettingsButton;
        private Button saveSettingsButton;
        private Panel cameraParamsPanel;
        private TextBox gainTb;
        private Label label6;
        private TextBox exposureTb;
        private Label label5;
        private TextBox widthTb;
        private Label label4;
        private TextBox heightTb;
        private Label label3;
        private Button applyCameraSettingsButton;
        private GroupBox groupBox1;
        private PictureBox cameraSettingsPb;
        private GroupBox foldersGroup;
        private Button browseUnderfillButton;
        private TextBox underfillPathTextBox;
        private Label label13;
        private Button applyFoldersButton;
        private Button browseObloyButton;
        private TextBox obloyPathTextBox;
        private Label label12;
        private Button browseInpaintButton;
        private TextBox inpaintPathTextBox;
        private Label label11;
        private Button browseInclusionButton;
        private TextBox inclusionPathTextBox;
        private Label label10;
        private Button browseOvalityButton;
        private TextBox ovalityPathTextBox;
        private Label label9;
        private Button browseOriginalButton;
        private TextBox originalPathTextBox;
        private Label label8;
        private Label label15;
        private Label camStatus;
    }
}