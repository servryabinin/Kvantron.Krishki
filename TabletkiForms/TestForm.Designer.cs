namespace KrishkiForms
{
    partial class TestForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestForm));
            mainPanel = new Panel();
            ovalityCB = new CheckBox();
            inclusionCB = new CheckBox();
            inpaintCB = new CheckBox();
            obloyCB = new CheckBox();
            comboBox1 = new ComboBox();
            pictureBox1 = new PictureBox();
            timeOvality = new TextBox();
            ovalityDef = new TextBox();
            obloyTime = new TextBox();
            obloyDef = new TextBox();
            inpaintTime = new TextBox();
            InpaintDef = new TextBox();
            inclusionTime = new TextBox();
            inclusionDef = new TextBox();
            imagesGroup = new GroupBox();
            originPb = new PictureBox();
            recognizeButton = new Button();
            startStreamButton = new Button();
            loadImageButton = new Button();
            label15 = new Label();
            camStatus = new Label();
            label16 = new Label();
            cameraStatusLabel = new Label();
            panel2 = new Panel();
            panel3 = new Panel();
            label2 = new Label();
            groupBox1 = new GroupBox();
            comboBox2 = new ComboBox();
            label34 = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            label35 = new Label();
            label42 = new Label();
            label41 = new Label();
            label39 = new Label();
            label38 = new Label();
            label37 = new Label();
            label36 = new Label();
            generslCapsCountTb = new TextBox();
            okCapsCountTb = new TextBox();
            percentOkCapsTb = new TextBox();
            percentNgCapsTb = new TextBox();
            ngCapsCountTb = new TextBox();
            generalTimeTb = new TextBox();
            label44 = new Label();
            tableLayoutPanel2 = new TableLayoutPanel();
            label45 = new Label();
            label46 = new Label();
            label52 = new Label();
            label53 = new Label();
            label54 = new Label();
            label55 = new Label();
            label56 = new Label();
            label57 = new Label();
            label58 = new Label();
            percentOvalityCapsTb = new TextBox();
            percentInclusionCapsTb = new TextBox();
            percentInpaintCapsTb = new TextBox();
            percentObloyCapsTb = new TextBox();
            label59 = new Label();
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            tableLayoutPanel3 = new TableLayoutPanel();
            mainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            imagesGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)originPb).BeginInit();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            groupBox1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            SuspendLayout();
            // 
            // mainPanel
            // 
            mainPanel.BackColor = Color.White;
            mainPanel.Controls.Add(panel3);
            mainPanel.Controls.Add(imagesGroup);
            mainPanel.Controls.Add(panel2);
            mainPanel.Controls.Add(pictureBox1);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new Point(0, 0);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new Size(1740, 1039);
            mainPanel.TabIndex = 0;
            // 
            // ovalityCB
            // 
            ovalityCB.Anchor = AnchorStyles.None;
            ovalityCB.AutoSize = true;
            ovalityCB.BackColor = Color.White;
            ovalityCB.Checked = true;
            ovalityCB.CheckState = CheckState.Checked;
            ovalityCB.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            ovalityCB.ForeColor = Color.White;
            ovalityCB.Location = new Point(29, 71);
            ovalityCB.Name = "ovalityCB";
            ovalityCB.Size = new Size(15, 14);
            ovalityCB.TabIndex = 9;
            ovalityCB.UseVisualStyleBackColor = false;
            ovalityCB.CheckedChanged += ovalityCB_CheckedChanged;
            // 
            // inclusionCB
            // 
            inclusionCB.Anchor = AnchorStyles.None;
            inclusionCB.AutoSize = true;
            inclusionCB.BackColor = Color.White;
            inclusionCB.Checked = true;
            inclusionCB.CheckState = CheckState.Checked;
            inclusionCB.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            inclusionCB.ForeColor = Color.White;
            inclusionCB.Location = new Point(29, 122);
            inclusionCB.Name = "inclusionCB";
            inclusionCB.Size = new Size(15, 14);
            inclusionCB.TabIndex = 13;
            inclusionCB.UseVisualStyleBackColor = false;
            inclusionCB.CheckedChanged += inclusionCB_CheckedChanged;
            // 
            // inpaintCB
            // 
            inpaintCB.Anchor = AnchorStyles.None;
            inpaintCB.AutoSize = true;
            inpaintCB.BackColor = Color.White;
            inpaintCB.Checked = true;
            inpaintCB.CheckState = CheckState.Checked;
            inpaintCB.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            inpaintCB.ForeColor = Color.White;
            inpaintCB.Location = new Point(29, 173);
            inpaintCB.Name = "inpaintCB";
            inpaintCB.Size = new Size(15, 14);
            inpaintCB.TabIndex = 35;
            inpaintCB.UseVisualStyleBackColor = false;
            inpaintCB.CheckedChanged += inpaintCB_CheckedChanged;
            // 
            // obloyCB
            // 
            obloyCB.Anchor = AnchorStyles.None;
            obloyCB.AutoSize = true;
            obloyCB.BackColor = Color.White;
            obloyCB.Checked = true;
            obloyCB.CheckState = CheckState.Checked;
            obloyCB.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            obloyCB.ForeColor = Color.White;
            obloyCB.Location = new Point(29, 224);
            obloyCB.Name = "obloyCB";
            obloyCB.Size = new Size(15, 14);
            obloyCB.TabIndex = 9;
            obloyCB.UseVisualStyleBackColor = false;
            obloyCB.CheckedChanged += obloyCB_CheckedChanged;
            // 
            // comboBox1
            // 
            comboBox1.BackColor = Color.LightGray;
            comboBox1.Dock = DockStyle.Fill;
            comboBox1.FlatStyle = FlatStyle.Popup;
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "Желтые", "Синие", "Золотые", "Белые", "Зеленые", "Оранжевые" });
            comboBox1.Location = new Point(217, 3);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(209, 23);
            comboBox1.TabIndex = 38;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // pictureBox1
            // 
            pictureBox1.BorderStyle = BorderStyle.Fixed3D;
            pictureBox1.Image = Properties.Resources.лого;
            pictureBox1.Location = new Point(1306, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(431, 126);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 8;
            pictureBox1.TabStop = false;
            // 
            // timeOvality
            // 
            timeOvality.Anchor = AnchorStyles.None;
            timeOvality.BackColor = Color.White;
            timeOvality.BorderStyle = BorderStyle.None;
            timeOvality.Location = new Point(267, 70);
            timeOvality.Name = "timeOvality";
            timeOvality.Size = new Size(69, 16);
            timeOvality.TabIndex = 9;
            timeOvality.Text = "0";
            timeOvality.TextAlign = HorizontalAlignment.Center;
            // 
            // ovalityDef
            // 
            ovalityDef.Anchor = AnchorStyles.None;
            ovalityDef.BackColor = Color.White;
            ovalityDef.BorderStyle = BorderStyle.None;
            ovalityDef.Location = new Point(181, 70);
            ovalityDef.Name = "ovalityDef";
            ovalityDef.Size = new Size(69, 16);
            ovalityDef.TabIndex = 8;
            ovalityDef.Text = "0";
            ovalityDef.TextAlign = HorizontalAlignment.Center;
            // 
            // obloyTime
            // 
            obloyTime.Anchor = AnchorStyles.None;
            obloyTime.BackColor = Color.White;
            obloyTime.BorderStyle = BorderStyle.None;
            obloyTime.Location = new Point(268, 223);
            obloyTime.Name = "obloyTime";
            obloyTime.Size = new Size(67, 16);
            obloyTime.TabIndex = 10;
            obloyTime.Text = "0";
            obloyTime.TextAlign = HorizontalAlignment.Center;
            // 
            // obloyDef
            // 
            obloyDef.Anchor = AnchorStyles.None;
            obloyDef.BackColor = Color.White;
            obloyDef.BorderStyle = BorderStyle.None;
            obloyDef.Location = new Point(181, 223);
            obloyDef.Name = "obloyDef";
            obloyDef.Size = new Size(69, 16);
            obloyDef.TabIndex = 9;
            obloyDef.Text = "0";
            obloyDef.TextAlign = HorizontalAlignment.Center;
            // 
            // inpaintTime
            // 
            inpaintTime.Anchor = AnchorStyles.None;
            inpaintTime.BackColor = Color.White;
            inpaintTime.BorderStyle = BorderStyle.None;
            inpaintTime.Location = new Point(268, 172);
            inpaintTime.Name = "inpaintTime";
            inpaintTime.Size = new Size(67, 16);
            inpaintTime.TabIndex = 12;
            inpaintTime.Text = "0";
            inpaintTime.TextAlign = HorizontalAlignment.Center;
            // 
            // InpaintDef
            // 
            InpaintDef.Anchor = AnchorStyles.None;
            InpaintDef.BackColor = Color.White;
            InpaintDef.BorderStyle = BorderStyle.None;
            InpaintDef.Location = new Point(182, 172);
            InpaintDef.Name = "InpaintDef";
            InpaintDef.Size = new Size(67, 16);
            InpaintDef.TabIndex = 11;
            InpaintDef.Text = "0";
            InpaintDef.TextAlign = HorizontalAlignment.Center;
            // 
            // inclusionTime
            // 
            inclusionTime.Anchor = AnchorStyles.None;
            inclusionTime.BackColor = Color.White;
            inclusionTime.BorderStyle = BorderStyle.None;
            inclusionTime.Location = new Point(267, 121);
            inclusionTime.Name = "inclusionTime";
            inclusionTime.Size = new Size(68, 16);
            inclusionTime.TabIndex = 11;
            inclusionTime.Text = "0";
            inclusionTime.TextAlign = HorizontalAlignment.Center;
            // 
            // inclusionDef
            // 
            inclusionDef.Anchor = AnchorStyles.None;
            inclusionDef.BackColor = Color.White;
            inclusionDef.BorderStyle = BorderStyle.None;
            inclusionDef.Location = new Point(181, 121);
            inclusionDef.Name = "inclusionDef";
            inclusionDef.Size = new Size(68, 16);
            inclusionDef.TabIndex = 10;
            inclusionDef.Text = "0";
            inclusionDef.TextAlign = HorizontalAlignment.Center;
            // 
            // imagesGroup
            // 
            imagesGroup.BackColor = Color.White;
            imagesGroup.Controls.Add(originPb);
            imagesGroup.FlatStyle = FlatStyle.Flat;
            imagesGroup.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 204);
            imagesGroup.ForeColor = Color.FromArgb(4, 85, 191);
            imagesGroup.Location = new Point(3, 135);
            imagesGroup.Name = "imagesGroup";
            imagesGroup.Size = new Size(1297, 880);
            imagesGroup.TabIndex = 0;
            imagesGroup.TabStop = false;
            imagesGroup.Text = "Камера";
            // 
            // originPb
            // 
            originPb.Location = new Point(10, 24);
            originPb.Name = "originPb";
            originPb.Size = new Size(1281, 847);
            originPb.SizeMode = PictureBoxSizeMode.Zoom;
            originPb.TabIndex = 0;
            originPb.TabStop = false;
            originPb.Click += originPb_Click;
            // 
            // recognizeButton
            // 
            recognizeButton.BackColor = Color.FromArgb(4, 85, 191);
            recognizeButton.FlatAppearance.BorderSize = 0;
            recognizeButton.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 204);
            recognizeButton.ForeColor = Color.White;
            recognizeButton.Location = new Point(192, 53);
            recognizeButton.Name = "recognizeButton";
            recognizeButton.Size = new Size(99, 51);
            recognizeButton.TabIndex = 4;
            recognizeButton.Text = "Начать анализ";
            recognizeButton.UseVisualStyleBackColor = false;
            recognizeButton.Click += recognizeButton_Click;
            // 
            // startStreamButton
            // 
            startStreamButton.BackColor = Color.FromArgb(66, 133, 244);
            startStreamButton.FlatAppearance.BorderSize = 0;
            startStreamButton.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 204);
            startStreamButton.ForeColor = Color.White;
            startStreamButton.Location = new Point(97, 53);
            startStreamButton.Name = "startStreamButton";
            startStreamButton.Size = new Size(89, 51);
            startStreamButton.TabIndex = 2;
            startStreamButton.Text = "Получить";
            startStreamButton.UseVisualStyleBackColor = false;
            startStreamButton.Click += startStreamButton_Click;
            // 
            // loadImageButton
            // 
            loadImageButton.BackColor = Color.FromArgb(66, 133, 244);
            loadImageButton.FlatAppearance.BorderSize = 0;
            loadImageButton.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 204);
            loadImageButton.ForeColor = Color.White;
            loadImageButton.Location = new Point(3, 54);
            loadImageButton.Name = "loadImageButton";
            loadImageButton.Size = new Size(88, 50);
            loadImageButton.TabIndex = 1;
            loadImageButton.Text = "Загрузить";
            loadImageButton.UseVisualStyleBackColor = false;
            loadImageButton.Click += loadImageButton_Click;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Font = new Font("Segoe UI", 8.25F);
            label15.ForeColor = Color.Black;
            label15.Location = new Point(17, 61);
            label15.Name = "label15";
            label15.Size = new Size(87, 13);
            label15.TabIndex = 0;
            label15.Text = "Статус камеры:";
            // 
            // camStatus
            // 
            camStatus.AutoSize = true;
            camStatus.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            camStatus.ForeColor = Color.Black;
            camStatus.Location = new Point(146, 61);
            camStatus.Name = "camStatus";
            camStatus.Size = new Size(94, 13);
            camStatus.TabIndex = 2;
            camStatus.Text = "Не подключена";
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Font = new Font("Segoe UI", 8.25F);
            label16.ForeColor = Color.Black;
            label16.Location = new Point(17, 28);
            label16.Name = "label16";
            label16.Size = new Size(74, 13);
            label16.TabIndex = 1;
            label16.Text = "Видеопоток:";
            // 
            // cameraStatusLabel
            // 
            cameraStatusLabel.AutoSize = true;
            cameraStatusLabel.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            cameraStatusLabel.ForeColor = Color.Black;
            cameraStatusLabel.Location = new Point(147, 29);
            cameraStatusLabel.Name = "cameraStatusLabel";
            cameraStatusLabel.Size = new Size(71, 13);
            cameraStatusLabel.TabIndex = 3;
            cameraStatusLabel.Text = "Не запущен";
            // 
            // panel2
            // 
            panel2.BorderStyle = BorderStyle.Fixed3D;
            panel2.Controls.Add(cameraStatusLabel);
            panel2.Controls.Add(label16);
            panel2.Controls.Add(label15);
            panel2.Controls.Add(camStatus);
            panel2.Location = new Point(3, 3);
            panel2.Name = "panel2";
            panel2.Size = new Size(1297, 126);
            panel2.TabIndex = 9;
            // 
            // panel3
            // 
            panel3.BorderStyle = BorderStyle.Fixed3D;
            panel3.Controls.Add(tableLayoutPanel3);
            panel3.Controls.Add(label59);
            panel3.Controls.Add(tableLayoutPanel2);
            panel3.Controls.Add(label44);
            panel3.Controls.Add(tableLayoutPanel1);
            panel3.Controls.Add(label34);
            panel3.Controls.Add(recognizeButton);
            panel3.Controls.Add(groupBox1);
            panel3.Controls.Add(startStreamButton);
            panel3.Controls.Add(label2);
            panel3.Controls.Add(loadImageButton);
            panel3.Location = new Point(1306, 145);
            panel3.Name = "panel3";
            panel3.Size = new Size(431, 870);
            panel3.TabIndex = 10;
            // 
            // label2
            // 
            label2.BackColor = Color.FromArgb(4, 85, 191);
            label2.BorderStyle = BorderStyle.Fixed3D;
            label2.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label2.ForeColor = Color.White;
            label2.Location = new Point(-2, 0);
            label2.Name = "label2";
            label2.Size = new Size(431, 43);
            label2.TabIndex = 0;
            label2.Text = "Получение изображения и анализ";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(comboBox2);
            groupBox1.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 204);
            groupBox1.ForeColor = Color.FromArgb(4, 85, 191);
            groupBox1.Location = new Point(297, 44);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(127, 60);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Что выводить";
            // 
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Items.AddRange(new object[] { "Все", "Хорошие", "Плохие" });
            comboBox2.Location = new Point(10, 24);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(111, 25);
            comboBox2.TabIndex = 0;
            // 
            // label34
            // 
            label34.BackColor = Color.FromArgb(4, 85, 191);
            label34.BorderStyle = BorderStyle.Fixed3D;
            label34.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label34.ForeColor = Color.White;
            label34.Location = new Point(-2, 116);
            label34.Name = "label34";
            label34.Size = new Size(431, 43);
            label34.TabIndex = 5;
            label34.Text = "Общая информация";
            label34.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(generalTimeTb, 1, 6);
            tableLayoutPanel1.Controls.Add(ngCapsCountTb, 1, 3);
            tableLayoutPanel1.Controls.Add(label35, 0, 0);
            tableLayoutPanel1.Controls.Add(comboBox1, 1, 0);
            tableLayoutPanel1.Controls.Add(label36, 0, 1);
            tableLayoutPanel1.Controls.Add(label37, 0, 2);
            tableLayoutPanel1.Controls.Add(label38, 0, 3);
            tableLayoutPanel1.Controls.Add(label39, 0, 4);
            tableLayoutPanel1.Controls.Add(label41, 0, 5);
            tableLayoutPanel1.Controls.Add(label42, 0, 6);
            tableLayoutPanel1.Controls.Add(generslCapsCountTb, 1, 1);
            tableLayoutPanel1.Controls.Add(okCapsCountTb, 1, 2);
            tableLayoutPanel1.Controls.Add(percentOkCapsTb, 1, 4);
            tableLayoutPanel1.Controls.Add(percentNgCapsTb, 1, 5);
            tableLayoutPanel1.Location = new Point(0, 159);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 7;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 14.2857141F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 14.2857141F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 14.2857141F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 14.2857141F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 14.2857141F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 14.2857141F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 14.2857141F));
            tableLayoutPanel1.Size = new Size(429, 248);
            tableLayoutPanel1.TabIndex = 6;
            // 
            // label35
            // 
            label35.AutoSize = true;
            label35.Dock = DockStyle.Right;
            label35.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label35.Location = new Point(160, 0);
            label35.Name = "label35";
            label35.Size = new Size(51, 35);
            label35.TabIndex = 0;
            label35.Text = "Рецепт:";
            label35.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label42
            // 
            label42.AutoSize = true;
            label42.Dock = DockStyle.Right;
            label42.Location = new Point(125, 210);
            label42.Name = "label42";
            label42.Size = new Size(86, 38);
            label42.TabIndex = 6;
            label42.Text = "Общее время:";
            label42.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label41
            // 
            label41.AutoSize = true;
            label41.Dock = DockStyle.Right;
            label41.Location = new Point(133, 175);
            label41.Name = "label41";
            label41.Size = new Size(78, 35);
            label41.TabIndex = 5;
            label41.Text = "Процент NG:";
            label41.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label39
            // 
            label39.AutoSize = true;
            label39.Dock = DockStyle.Right;
            label39.Location = new Point(134, 140);
            label39.Name = "label39";
            label39.Size = new Size(77, 35);
            label39.TabIndex = 4;
            label39.Text = "Процент ОК:";
            label39.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label38
            // 
            label38.AutoSize = true;
            label38.Dock = DockStyle.Right;
            label38.Location = new Point(116, 105);
            label38.Name = "label38";
            label38.Size = new Size(95, 35);
            label38.TabIndex = 3;
            label38.Text = "Количество NG:";
            label38.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label37
            // 
            label37.AutoSize = true;
            label37.Dock = DockStyle.Right;
            label37.Location = new Point(143, 70);
            label37.Name = "label37";
            label37.Size = new Size(68, 35);
            label37.TabIndex = 2;
            label37.Text = "Кол-во ОК:";
            label37.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label36
            // 
            label36.AutoSize = true;
            label36.Dock = DockStyle.Right;
            label36.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label36.Location = new Point(61, 35);
            label36.Name = "label36";
            label36.Size = new Size(150, 35);
            label36.TabIndex = 1;
            label36.Text = "Общее кол-во крышек:";
            label36.TextAlign = ContentAlignment.MiddleRight;
            // 
            // generslCapsCountTb
            // 
            generslCapsCountTb.Dock = DockStyle.Fill;
            generslCapsCountTb.Location = new Point(217, 38);
            generslCapsCountTb.Name = "generslCapsCountTb";
            generslCapsCountTb.Size = new Size(209, 23);
            generslCapsCountTb.TabIndex = 39;
            // 
            // okCapsCountTb
            // 
            okCapsCountTb.Dock = DockStyle.Fill;
            okCapsCountTb.Location = new Point(217, 73);
            okCapsCountTb.Name = "okCapsCountTb";
            okCapsCountTb.Size = new Size(209, 23);
            okCapsCountTb.TabIndex = 40;
            // 
            // percentOkCapsTb
            // 
            percentOkCapsTb.Dock = DockStyle.Fill;
            percentOkCapsTb.Location = new Point(217, 143);
            percentOkCapsTb.Name = "percentOkCapsTb";
            percentOkCapsTb.Size = new Size(209, 23);
            percentOkCapsTb.TabIndex = 41;
            // 
            // percentNgCapsTb
            // 
            percentNgCapsTb.Dock = DockStyle.Fill;
            percentNgCapsTb.Location = new Point(217, 178);
            percentNgCapsTb.Name = "percentNgCapsTb";
            percentNgCapsTb.Size = new Size(209, 23);
            percentNgCapsTb.TabIndex = 42;
            // 
            // ngCapsCountTb
            // 
            ngCapsCountTb.Dock = DockStyle.Fill;
            ngCapsCountTb.Location = new Point(217, 108);
            ngCapsCountTb.Name = "ngCapsCountTb";
            ngCapsCountTb.Size = new Size(209, 23);
            ngCapsCountTb.TabIndex = 43;
            // 
            // generalTimeTb
            // 
            generalTimeTb.Dock = DockStyle.Fill;
            generalTimeTb.Location = new Point(217, 213);
            generalTimeTb.Name = "generalTimeTb";
            generalTimeTb.Size = new Size(209, 23);
            generalTimeTb.TabIndex = 44;
            // 
            // label44
            // 
            label44.BackColor = Color.FromArgb(4, 85, 191);
            label44.BorderStyle = BorderStyle.Fixed3D;
            label44.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label44.ForeColor = Color.White;
            label44.Location = new Point(-2, 418);
            label44.Name = "label44";
            label44.Size = new Size(431, 43);
            label44.TabIndex = 7;
            label44.Text = "Результаты работы";
            label44.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.CellBorderStyle = TableLayoutPanelCellBorderStyle.OutsetPartial;
            tableLayoutPanel2.ColumnCount = 5;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.2037029F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23.61111F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.Controls.Add(inpaintTime, 3, 3);
            tableLayoutPanel2.Controls.Add(obloyTime, 3, 4);
            tableLayoutPanel2.Controls.Add(inclusionTime, 3, 2);
            tableLayoutPanel2.Controls.Add(timeOvality, 3, 1);
            tableLayoutPanel2.Controls.Add(label45, 0, 0);
            tableLayoutPanel2.Controls.Add(ovalityDef, 2, 1);
            tableLayoutPanel2.Controls.Add(inclusionDef, 2, 2);
            tableLayoutPanel2.Controls.Add(obloyDef, 2, 4);
            tableLayoutPanel2.Controls.Add(InpaintDef, 2, 3);
            tableLayoutPanel2.Controls.Add(label46, 1, 0);
            tableLayoutPanel2.Controls.Add(label52, 2, 0);
            tableLayoutPanel2.Controls.Add(obloyCB, 0, 4);
            tableLayoutPanel2.Controls.Add(label53, 3, 0);
            tableLayoutPanel2.Controls.Add(label54, 4, 0);
            tableLayoutPanel2.Controls.Add(label55, 1, 1);
            tableLayoutPanel2.Controls.Add(label56, 1, 2);
            tableLayoutPanel2.Controls.Add(label57, 1, 3);
            tableLayoutPanel2.Controls.Add(label58, 1, 4);
            tableLayoutPanel2.Controls.Add(ovalityCB, 0, 1);
            tableLayoutPanel2.Controls.Add(inclusionCB, 0, 2);
            tableLayoutPanel2.Controls.Add(inpaintCB, 0, 3);
            tableLayoutPanel2.Controls.Add(percentOvalityCapsTb, 4, 1);
            tableLayoutPanel2.Controls.Add(percentInclusionCapsTb, 4, 2);
            tableLayoutPanel2.Controls.Add(percentInpaintCapsTb, 4, 3);
            tableLayoutPanel2.Controls.Add(percentObloyCapsTb, 4, 4);
            tableLayoutPanel2.Location = new Point(-2, 464);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 5;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.Size = new Size(434, 258);
            tableLayoutPanel2.TabIndex = 8;
            // 
            // label45
            // 
            label45.AutoSize = true;
            label45.Dock = DockStyle.Fill;
            label45.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label45.Location = new Point(6, 3);
            label45.Name = "label45";
            label45.Size = new Size(61, 48);
            label45.TabIndex = 0;
            label45.Text = "✔";
            label45.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label46
            // 
            label46.AutoSize = true;
            label46.Dock = DockStyle.Fill;
            label46.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label46.Location = new Point(76, 3);
            label46.Name = "label46";
            label46.Size = new Size(92, 48);
            label46.TabIndex = 1;
            label46.Text = "Название дефекта";
            label46.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label52
            // 
            label52.AutoSize = true;
            label52.Dock = DockStyle.Fill;
            label52.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label52.Location = new Point(177, 3);
            label52.Name = "label52";
            label52.Size = new Size(77, 48);
            label52.TabIndex = 2;
            label52.Text = "Количество";
            label52.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label53
            // 
            label53.AutoSize = true;
            label53.Dock = DockStyle.Fill;
            label53.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label53.Location = new Point(263, 3);
            label53.Name = "label53";
            label53.Size = new Size(77, 48);
            label53.TabIndex = 3;
            label53.Text = "Время";
            label53.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label54
            // 
            label54.AutoSize = true;
            label54.Dock = DockStyle.Fill;
            label54.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label54.Location = new Point(349, 3);
            label54.Name = "label54";
            label54.Size = new Size(79, 48);
            label54.TabIndex = 4;
            label54.Text = "Процент";
            label54.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label55
            // 
            label55.AutoSize = true;
            label55.Dock = DockStyle.Fill;
            label55.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label55.Location = new Point(76, 54);
            label55.Name = "label55";
            label55.Size = new Size(92, 48);
            label55.TabIndex = 5;
            label55.Text = "Овальность";
            label55.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label56
            // 
            label56.AutoSize = true;
            label56.Dock = DockStyle.Fill;
            label56.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label56.Location = new Point(76, 105);
            label56.Name = "label56";
            label56.Size = new Size(92, 48);
            label56.TabIndex = 6;
            label56.Text = "Вкрапления";
            label56.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label57
            // 
            label57.AutoSize = true;
            label57.Dock = DockStyle.Fill;
            label57.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label57.Location = new Point(76, 156);
            label57.Name = "label57";
            label57.Size = new Size(92, 48);
            label57.TabIndex = 7;
            label57.Text = "Непрокрас";
            label57.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label58
            // 
            label58.AutoSize = true;
            label58.Dock = DockStyle.Fill;
            label58.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label58.Location = new Point(76, 207);
            label58.Name = "label58";
            label58.Size = new Size(92, 48);
            label58.TabIndex = 8;
            label58.Text = "Облой";
            label58.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // percentOvalityCapsTb
            // 
            percentOvalityCapsTb.Anchor = AnchorStyles.None;
            percentOvalityCapsTb.BorderStyle = BorderStyle.None;
            percentOvalityCapsTb.Location = new Point(349, 66);
            percentOvalityCapsTb.Name = "percentOvalityCapsTb";
            percentOvalityCapsTb.Size = new Size(79, 16);
            percentOvalityCapsTb.TabIndex = 36;
            percentOvalityCapsTb.Text = "0";
            percentOvalityCapsTb.TextAlign = HorizontalAlignment.Center;
            // 
            // percentInclusionCapsTb
            // 
            percentInclusionCapsTb.Anchor = AnchorStyles.None;
            percentInclusionCapsTb.BorderStyle = BorderStyle.None;
            percentInclusionCapsTb.Location = new Point(349, 121);
            percentInclusionCapsTb.Name = "percentInclusionCapsTb";
            percentInclusionCapsTb.Size = new Size(79, 16);
            percentInclusionCapsTb.TabIndex = 37;
            percentInclusionCapsTb.Text = "0";
            percentInclusionCapsTb.TextAlign = HorizontalAlignment.Center;
            // 
            // percentInpaintCapsTb
            // 
            percentInpaintCapsTb.Anchor = AnchorStyles.None;
            percentInpaintCapsTb.BorderStyle = BorderStyle.None;
            percentInpaintCapsTb.Location = new Point(349, 168);
            percentInpaintCapsTb.Name = "percentInpaintCapsTb";
            percentInpaintCapsTb.Size = new Size(79, 16);
            percentInpaintCapsTb.TabIndex = 38;
            percentInpaintCapsTb.Text = "0";
            percentInpaintCapsTb.TextAlign = HorizontalAlignment.Center;
            // 
            // percentObloyCapsTb
            // 
            percentObloyCapsTb.Anchor = AnchorStyles.None;
            percentObloyCapsTb.BorderStyle = BorderStyle.None;
            percentObloyCapsTb.Location = new Point(349, 223);
            percentObloyCapsTb.Name = "percentObloyCapsTb";
            percentObloyCapsTb.Size = new Size(79, 16);
            percentObloyCapsTb.TabIndex = 39;
            percentObloyCapsTb.Text = "0";
            percentObloyCapsTb.TextAlign = HorizontalAlignment.Center;
            // 
            // label59
            // 
            label59.BackColor = Color.FromArgb(4, 85, 191);
            label59.BorderStyle = BorderStyle.Fixed3D;
            label59.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label59.ForeColor = Color.White;
            label59.Location = new Point(-2, 730);
            label59.Name = "label59";
            label59.Size = new Size(431, 43);
            label59.TabIndex = 9;
            label59.Text = "Настройкки аппаратуры и параметров";
            label59.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // button1
            // 
            button1.BackColor = Color.FromArgb(66, 133, 244);
            button1.Dock = DockStyle.Fill;
            button1.FlatAppearance.BorderSize = 0;
            button1.Font = new Font("Segoe UI", 36F, FontStyle.Bold, GraphicsUnit.Point, 204);
            button1.ForeColor = Color.White;
            button1.Location = new Point(3, 3);
            button1.Name = "button1";
            button1.Size = new Size(129, 75);
            button1.TabIndex = 10;
            button1.Text = "⛭";
            button1.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            button2.BackColor = Color.FromArgb(66, 133, 244);
            button2.Dock = DockStyle.Fill;
            button2.FlatAppearance.BorderSize = 0;
            button2.Font = new Font("Segoe UI", 36F, FontStyle.Bold, GraphicsUnit.Point, 204);
            button2.ForeColor = Color.White;
            button2.Location = new Point(138, 3);
            button2.Name = "button2";
            button2.Size = new Size(129, 75);
            button2.TabIndex = 11;
            button2.Text = "🔍";
            button2.UseVisualStyleBackColor = false;
            // 
            // button3
            // 
            button3.BackColor = Color.FromArgb(66, 133, 244);
            button3.Dock = DockStyle.Fill;
            button3.FlatAppearance.BorderSize = 0;
            button3.Font = new Font("Segoe UI", 36F, FontStyle.Bold, GraphicsUnit.Point, 204);
            button3.ForeColor = Color.White;
            button3.Location = new Point(273, 3);
            button3.Name = "button3";
            button3.Size = new Size(130, 75);
            button3.TabIndex = 12;
            button3.Text = "◯";
            button3.UseVisualStyleBackColor = false;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 3;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel3.Controls.Add(button3, 2, 0);
            tableLayoutPanel3.Controls.Add(button2, 1, 0);
            tableLayoutPanel3.Controls.Add(button1, 0, 0);
            tableLayoutPanel3.Location = new Point(13, 778);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel3.Size = new Size(406, 81);
            tableLayoutPanel3.TabIndex = 10;
            // 
            // TestForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1740, 1039);
            Controls.Add(mainPanel);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(1200, 800);
            Name = "TestForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CapDefect Detector";
            FormClosing += Form2_FormClosing;
            mainPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            imagesGroup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)originPb).EndInit();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel3.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.GroupBox imagesGroup;
        private System.Windows.Forms.Button recognizeButton;
        private System.Windows.Forms.Button startStreamButton;
        private Button loadImageButton;
        private TextBox obloyDef;
        private TextBox InpaintDef;
        private TextBox ovalityDef;
        private TextBox inclusionDef;
        private TextBox timeOvality;
        private TextBox inpaintTime;
        private TextBox obloyTime;
        private TextBox inclusionTime;
        private ComboBox comboBox1;
        private CheckBox ovalityCB;
        private CheckBox inclusionCB;
        private CheckBox inpaintCB;
        private CheckBox obloyCB;
        private PictureBox pictureBox1;
        private TextBox generalTime;
        private PictureBox originPb;
        private Panel panel3;
        private Label label2;
        private Panel panel2;
        private Label cameraStatusLabel;
        private Label label16;
        private Label label15;
        private Label camStatus;
        private GroupBox groupBox1;
        private TableLayoutPanel tableLayoutPanel1;
        private Label label35;
        private Label label36;
        private Label label37;
        private Label label38;
        private Label label39;
        private Label label41;
        private Label label42;
        private Label label34;
        private ComboBox comboBox2;
        private TextBox generslCapsCountTb;
        private TextBox okCapsCountTb;
        private TextBox percentOkCapsTb;
        private TextBox percentNgCapsTb;
        private TextBox ngCapsCountTb;
        private TableLayoutPanel tableLayoutPanel2;
        private Label label45;
        private Label label46;
        private Label label52;
        private Label label53;
        private Label label54;
        private Label label44;
        private TextBox generalTimeTb;
        private Label label55;
        private Label label56;
        private Label label57;
        private Label label58;
        private Button button1;
        private Label label59;
        private TextBox percentOvalityCapsTb;
        private TextBox percentInclusionCapsTb;
        private TextBox percentInpaintCapsTb;
        private TextBox percentObloyCapsTb;
        private TableLayoutPanel tableLayoutPanel3;
        private Button button3;
        private Button button2;
    }
}