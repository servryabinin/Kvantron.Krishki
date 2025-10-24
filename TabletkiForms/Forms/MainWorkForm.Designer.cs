namespace KrishkiForms
{
    partial class MainWorkForm
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
                _imageForOvality?.Dispose();
                _grayForOvality?.Dispose();
                _imageForInclusions?.Dispose();
                _grayForInclusions?.Dispose();
                _imageForPaintDefects?.Dispose();
                _grayForPaintDefects?.Dispose();
                _imageForUnderfill?.Dispose();
                _grayForUnderfill?.Dispose();
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
            tabParameter = new TabControl();
            tabParSearch = new TabPage();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label1 = new Label();
            tabParCamera = new TabPage();
            label11 = new Label();
            label6 = new Label();
            label7 = new Label();
            label8 = new Label();
            label9 = new Label();
            label10 = new Label();
            panel1 = new Panel();
            panel2 = new Panel();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            groupBox5 = new GroupBox();
            obloyPixCount = new TextBox();
            label47 = new Label();
            tabControl6 = new TabControl();
            tabPage7 = new TabPage();
            label18 = new Label();
            textBox4 = new TextBox();
            tabControl5 = new TabControl();
            tabPage6 = new TabPage();
            drawRoi = new Button();
            redrawRoi = new Button();
            groupBox4 = new GroupBox();
            whiteThresoldTx = new TextBox();
            label29 = new Label();
            label28 = new Label();
            minSquareInpaint = new TextBox();
            groupBox2 = new GroupBox();
            groupBox3 = new GroupBox();
            textBox2 = new TextBox();
            label16 = new Label();
            maxSquareInclusion = new TextBox();
            label15 = new Label();
            minSquareInclusion = new TextBox();
            label14 = new Label();
            circleCoefTx = new TextBox();
            label13 = new Label();
            groupBox1 = new GroupBox();
            ovalityCoef = new TextBox();
            label12 = new Label();
            obduvCB = new CheckBox();
            prStatus = new Label();
            camStatus = new Label();
            label39 = new Label();
            label19 = new Label();
            button10 = new Button();
            button11 = new Button();
            textBox5 = new TextBox();
            label40 = new Label();
            textBox6 = new TextBox();
            label41 = new Label();
            gainTb = new TextBox();
            label17 = new Label();
            saveImageButton = new Button();
            applySettingsButton = new Button();
            exposureTb = new TextBox();
            label20 = new Label();
            widthTb = new TextBox();
            label21 = new Label();
            heightTb = new TextBox();
            label22 = new Label();
            panel3 = new Panel();
            panel14 = new Panel();
            pixelCount = new TextBox();
            obloyCB = new CheckBox();
            label32 = new Label();
            obloyPb = new PictureBox();
            obloyTime = new TextBox();
            label45 = new Label();
            obloyDef = new TextBox();
            label46 = new Label();
            panel13 = new Panel();
            tabControl4 = new TabControl();
            tabPage5 = new TabPage();
            comboBox1 = new ComboBox();
            label43 = new Label();
            tabControl3 = new TabControl();
            tabPage4 = new TabPage();
            tabControl2 = new TabControl();
            tabPage3 = new TabPage();
            loadSettingsButton = new Button();
            panel12 = new Panel();
            label42 = new Label();
            originPb = new PictureBox();
            panel10 = new Panel();
            imageProcDelay = new TextBox();
            lan1 = new Label();
            delayTb = new TextBox();
            label44 = new Label();
            generalTime = new TextBox();
            inpaintCB = new CheckBox();
            label31 = new Label();
            inpaintTime = new TextBox();
            InpaintDef = new TextBox();
            label27 = new Label();
            inpaintPb = new PictureBox();
            label24 = new Label();
            panel9 = new Panel();
            inclusionCB = new CheckBox();
            conclusionTime = new TextBox();
            inclusionDef = new TextBox();
            label26 = new Label();
            inclusionPb = new PictureBox();
            label23 = new Label();
            panel6 = new Panel();
            ovalityCB = new CheckBox();
            label30 = new Label();
            ovalityPb = new PictureBox();
            timeOvality = new TextBox();
            label25 = new Label();
            ovalityDef = new TextBox();
            label2 = new Label();
            obduvBatton = new Button();
            panel4 = new Panel();
            label33 = new Label();
            textBox1 = new TextBox();
            label34 = new Label();
            textBox3 = new TextBox();
            label35 = new Label();
            pictureBox2 = new PictureBox();
            panel11 = new Panel();
            ratioTb = new TextBox();
            radiusTb = new TextBox();
            label36 = new Label();
            timeUnderfill = new TextBox();
            label37 = new Label();
            underfillDef = new TextBox();
            label38 = new Label();
            underfillPictureBox = new PictureBox();
            panel7 = new Panel();
            recognizeButton = new Button();
            panel5 = new Panel();
            endStream = new Button();
            getImageButton = new Button();
            panel8 = new Panel();
            button3 = new Button();
            loadImageButton = new Button();
            tabParameter.SuspendLayout();
            tabParSearch.SuspendLayout();
            tabParCamera.SuspendLayout();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            groupBox5.SuspendLayout();
            tabControl6.SuspendLayout();
            tabPage7.SuspendLayout();
            tabControl5.SuspendLayout();
            tabPage6.SuspendLayout();
            groupBox4.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox1.SuspendLayout();
            panel3.SuspendLayout();
            panel14.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)obloyPb).BeginInit();
            panel13.SuspendLayout();
            tabControl4.SuspendLayout();
            tabPage5.SuspendLayout();
            tabControl3.SuspendLayout();
            tabPage4.SuspendLayout();
            tabControl2.SuspendLayout();
            tabPage3.SuspendLayout();
            panel12.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)originPb).BeginInit();
            panel10.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)inpaintPb).BeginInit();
            panel9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)inclusionPb).BeginInit();
            panel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)ovalityPb).BeginInit();
            panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            panel11.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)underfillPictureBox).BeginInit();
            panel7.SuspendLayout();
            panel5.SuspendLayout();
            panel8.SuspendLayout();
            SuspendLayout();
            // 
            // tabParameter
            // 
            tabParameter.Controls.Add(tabParSearch);
            tabParameter.Controls.Add(tabParCamera);
            tabParameter.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);
            tabParameter.Location = new Point(3, 4);
            tabParameter.Margin = new Padding(3, 4, 3, 4);
            tabParameter.Name = "tabParameter";
            tabParameter.SelectedIndex = 0;
            tabParameter.Size = new Size(377, 412);
            tabParameter.TabIndex = 0;
            // 
            // tabParSearch
            // 
            tabParSearch.Controls.Add(label5);
            tabParSearch.Controls.Add(label4);
            tabParSearch.Controls.Add(label3);
            tabParSearch.Controls.Add(label1);
            tabParSearch.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            tabParSearch.Location = new Point(4, 27);
            tabParSearch.Margin = new Padding(3, 4, 3, 4);
            tabParSearch.Name = "tabParSearch";
            tabParSearch.Padding = new Padding(3, 4, 3, 4);
            tabParSearch.Size = new Size(369, 381);
            tabParSearch.TabIndex = 0;
            tabParSearch.Text = "Параметры поиска";
            tabParSearch.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(19, 308);
            label5.Name = "label5";
            label5.Size = new Size(198, 18);
            label5.TabIndex = 8;
            label5.Text = "Максимальное расстояние";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(19, 232);
            label4.Name = "label4";
            label4.Size = new Size(110, 18);
            label4.TabIndex = 6;
            label4.Text = "Цвет таблетки";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(19, 159);
            label3.Name = "label3";
            label3.Size = new Size(145, 18);
            label3.TabIndex = 4;
            label3.Text = "Цвет пустой ячейки";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(19, 17);
            label1.Name = "label1";
            label1.Size = new Size(84, 18);
            label1.TabIndex = 0;
            label1.Text = "Цвет фона";
            // 
            // tabParCamera
            // 
            tabParCamera.Controls.Add(label11);
            tabParCamera.Controls.Add(label6);
            tabParCamera.Controls.Add(label7);
            tabParCamera.Controls.Add(label8);
            tabParCamera.Controls.Add(label9);
            tabParCamera.Controls.Add(label10);
            tabParCamera.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            tabParCamera.Location = new Point(4, 27);
            tabParCamera.Margin = new Padding(3, 4, 3, 4);
            tabParCamera.Name = "tabParCamera";
            tabParCamera.Padding = new Padding(3, 4, 3, 4);
            tabParCamera.Size = new Size(369, 381);
            tabParCamera.TabIndex = 1;
            tabParCamera.Text = "Параметры камера";
            tabParCamera.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(199, 172);
            label11.Name = "label11";
            label11.Size = new Size(113, 18);
            label11.TabIndex = 22;
            label11.Text = "Насыщенность";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(26, 95);
            label6.Name = "label6";
            label6.Size = new Size(124, 18);
            label6.TabIndex = 18;
            label6.Text = "Правый столбец";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(199, 95);
            label7.Name = "label7";
            label7.Size = new Size(116, 18);
            label7.TabIndex = 16;
            label7.Text = "Левый столбец";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(26, 172);
            label8.Name = "label8";
            label8.Size = new Size(92, 18);
            label8.TabIndex = 14;
            label8.Text = "Экспозиция";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(199, 16);
            label9.Name = "label9";
            label9.Size = new Size(106, 18);
            label9.TabIndex = 12;
            label9.Text = "Ширина кадра";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(26, 16);
            label10.Name = "label10";
            label10.Size = new Size(106, 18);
            label10.TabIndex = 10;
            label10.Text = "Высота кадра";
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.ControlLightLight;
            panel1.BorderStyle = BorderStyle.Fixed3D;
            panel1.Controls.Add(panel2);
            panel1.Controls.Add(tabParameter);
            panel1.Location = new Point(13, 16);
            panel1.Margin = new Padding(3, 4, 3, 4);
            panel1.Name = "panel1";
            panel1.Size = new Size(388, 781);
            panel1.TabIndex = 1;
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.ControlLightLight;
            panel2.BorderStyle = BorderStyle.Fixed3D;
            panel2.Controls.Add(tabControl1);
            panel2.Location = new Point(-2, -3);
            panel2.Margin = new Padding(3, 4, 3, 4);
            panel2.Name = "panel2";
            panel2.Size = new Size(388, 775);
            panel2.TabIndex = 2;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);
            tabControl1.Location = new Point(3, 4);
            tabControl1.Margin = new Padding(3, 4, 3, 4);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(377, 971);
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.BorderStyle = BorderStyle.Fixed3D;
            tabPage1.Controls.Add(groupBox5);
            tabPage1.Controls.Add(tabControl6);
            tabPage1.Controls.Add(tabControl5);
            tabPage1.Controls.Add(groupBox4);
            tabPage1.Controls.Add(groupBox2);
            tabPage1.Controls.Add(groupBox1);
            tabPage1.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            tabPage1.Location = new Point(4, 27);
            tabPage1.Margin = new Padding(3, 4, 3, 4);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3, 4, 3, 4);
            tabPage1.Size = new Size(369, 940);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Параметры поиска";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(obloyPixCount);
            groupBox5.Controls.Add(label47);
            groupBox5.Location = new Point(8, 493);
            groupBox5.Margin = new Padding(3, 4, 3, 4);
            groupBox5.Name = "groupBox5";
            groupBox5.Padding = new Padding(3, 4, 3, 4);
            groupBox5.Size = new Size(350, 92);
            groupBox5.TabIndex = 38;
            groupBox5.TabStop = false;
            groupBox5.Text = "Параметры для определения облоя";
            // 
            // obloyPixCount
            // 
            obloyPixCount.Location = new Point(35, 53);
            obloyPixCount.Margin = new Padding(3, 4, 3, 4);
            obloyPixCount.Name = "obloyPixCount";
            obloyPixCount.Size = new Size(114, 24);
            obloyPixCount.TabIndex = 6;
            obloyPixCount.Text = "25";
            // 
            // label47
            // 
            label47.AutoSize = true;
            label47.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label47.Location = new Point(32, 32);
            label47.Name = "label47";
            label47.Size = new Size(209, 17);
            label47.TabIndex = 5;
            label47.Text = "Минимальная площадь облоя:";
            // 
            // tabControl6
            // 
            tabControl6.Controls.Add(tabPage7);
            tabControl6.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            tabControl6.Location = new Point(203, 593);
            tabControl6.Margin = new Padding(3, 4, 3, 4);
            tabControl6.Name = "tabControl6";
            tabControl6.SelectedIndex = 0;
            tabControl6.Size = new Size(159, 131);
            tabControl6.TabIndex = 37;
            // 
            // tabPage7
            // 
            tabPage7.BorderStyle = BorderStyle.Fixed3D;
            tabPage7.Controls.Add(label18);
            tabPage7.Controls.Add(textBox4);
            tabPage7.Location = new Point(4, 29);
            tabPage7.Margin = new Padding(3, 4, 3, 4);
            tabPage7.Name = "tabPage7";
            tabPage7.Padding = new Padding(3, 4, 3, 4);
            tabPage7.Size = new Size(151, 98);
            tabPage7.TabIndex = 0;
            tabPage7.Text = "Брак";
            tabPage7.UseVisualStyleBackColor = true;
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new Point(2, 9);
            label18.Name = "label18";
            label18.Size = new Size(103, 40);
            label18.TabIndex = 7;
            label18.Text = "Бракованных\r\nкрышек:";
            // 
            // textBox4
            // 
            textBox4.Location = new Point(6, 51);
            textBox4.Margin = new Padding(3, 4, 3, 4);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(114, 27);
            textBox4.TabIndex = 8;
            // 
            // tabControl5
            // 
            tabControl5.Controls.Add(tabPage6);
            tabControl5.Location = new Point(7, 591);
            tabControl5.Margin = new Padding(3, 4, 3, 4);
            tabControl5.Name = "tabControl5";
            tabControl5.SelectedIndex = 0;
            tabControl5.Size = new Size(194, 133);
            tabControl5.TabIndex = 36;
            // 
            // tabPage6
            // 
            tabPage6.BorderStyle = BorderStyle.Fixed3D;
            tabPage6.Controls.Add(drawRoi);
            tabPage6.Controls.Add(redrawRoi);
            tabPage6.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);
            tabPage6.Location = new Point(4, 27);
            tabPage6.Margin = new Padding(3, 4, 3, 4);
            tabPage6.Name = "tabPage6";
            tabPage6.Padding = new Padding(3, 4, 3, 4);
            tabPage6.Size = new Size(186, 102);
            tabPage6.TabIndex = 0;
            tabPage6.Text = "Настройка области кадра";
            tabPage6.UseVisualStyleBackColor = true;
            // 
            // drawRoi
            // 
            drawRoi.BackColor = Color.FromArgb(4, 85, 191);
            drawRoi.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 204);
            drawRoi.ForeColor = SystemColors.Control;
            drawRoi.Location = new Point(3, 1);
            drawRoi.Margin = new Padding(3, 4, 3, 4);
            drawRoi.Name = "drawRoi";
            drawRoi.Size = new Size(169, 43);
            drawRoi.TabIndex = 26;
            drawRoi.Text = "Вырезать кадр";
            drawRoi.UseVisualStyleBackColor = false;
            drawRoi.Click += drawRoi_Click;
            // 
            // redrawRoi
            // 
            redrawRoi.BackColor = Color.FromArgb(4, 85, 191);
            redrawRoi.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 204);
            redrawRoi.ForeColor = SystemColors.Control;
            redrawRoi.Location = new Point(3, 47);
            redrawRoi.Margin = new Padding(3, 4, 3, 4);
            redrawRoi.Name = "redrawRoi";
            redrawRoi.Size = new Size(169, 43);
            redrawRoi.TabIndex = 27;
            redrawRoi.Text = "Вернуть кадр";
            redrawRoi.UseVisualStyleBackColor = false;
            redrawRoi.Click += redrawRoi_Click;
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(whiteThresoldTx);
            groupBox4.Controls.Add(label29);
            groupBox4.Controls.Add(label28);
            groupBox4.Controls.Add(minSquareInpaint);
            groupBox4.Location = new Point(8, 329);
            groupBox4.Margin = new Padding(3, 4, 3, 4);
            groupBox4.Name = "groupBox4";
            groupBox4.Padding = new Padding(3, 4, 3, 4);
            groupBox4.Size = new Size(350, 156);
            groupBox4.TabIndex = 30;
            groupBox4.TabStop = false;
            groupBox4.Text = "Параметры для определения непрокраса:";
            // 
            // whiteThresoldTx
            // 
            whiteThresoldTx.Location = new Point(34, 115);
            whiteThresoldTx.Margin = new Padding(3, 4, 3, 4);
            whiteThresoldTx.Name = "whiteThresoldTx";
            whiteThresoldTx.Size = new Size(114, 24);
            whiteThresoldTx.TabIndex = 34;
            whiteThresoldTx.Text = "10";
            // 
            // label29
            // 
            label29.AutoSize = true;
            label29.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label29.Location = new Point(31, 35);
            label29.Name = "label29";
            label29.Size = new Size(352, 17);
            label29.TabIndex = 31;
            label29.Text = "Минимальная площадь непрокраса (от 1 до 10000):";
            // 
            // label28
            // 
            label28.AutoSize = true;
            label28.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label28.Location = new Point(31, 93);
            label28.Name = "label28";
            label28.Size = new Size(221, 17);
            label28.TabIndex = 33;
            label28.Text = "Близость к белому (от 1 до 255)";
            // 
            // minSquareInpaint
            // 
            minSquareInpaint.Location = new Point(34, 56);
            minSquareInpaint.Margin = new Padding(3, 4, 3, 4);
            minSquareInpaint.Name = "minSquareInpaint";
            minSquareInpaint.Size = new Size(114, 24);
            minSquareInpaint.TabIndex = 32;
            minSquareInpaint.Text = "500";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(groupBox3);
            groupBox2.Controls.Add(maxSquareInclusion);
            groupBox2.Controls.Add(label15);
            groupBox2.Controls.Add(minSquareInclusion);
            groupBox2.Controls.Add(label14);
            groupBox2.Controls.Add(circleCoefTx);
            groupBox2.Controls.Add(label13);
            groupBox2.Location = new Point(7, 115);
            groupBox2.Margin = new Padding(3, 4, 3, 4);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(3, 4, 3, 4);
            groupBox2.Size = new Size(350, 207);
            groupBox2.TabIndex = 29;
            groupBox2.TabStop = false;
            groupBox2.Text = "Параметры для вкраплений:";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(textBox2);
            groupBox3.Controls.Add(label16);
            groupBox3.Location = new Point(1, 204);
            groupBox3.Margin = new Padding(3, 4, 3, 4);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new Padding(3, 4, 3, 4);
            groupBox3.Size = new Size(350, 112);
            groupBox3.TabIndex = 30;
            groupBox3.TabStop = false;
            groupBox3.Text = "Параметры для определения овальности";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(35, 59);
            textBox2.Margin = new Padding(3, 4, 3, 4);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(114, 24);
            textBox2.TabIndex = 6;
            textBox2.Text = "0,96";
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label16.Location = new Point(32, 37);
            label16.Name = "label16";
            label16.Size = new Size(185, 17);
            label16.TabIndex = 5;
            label16.Text = "Коэффициент овальности:";
            // 
            // maxSquareInclusion
            // 
            maxSquareInclusion.Location = new Point(35, 165);
            maxSquareInclusion.Margin = new Padding(3, 4, 3, 4);
            maxSquareInclusion.Name = "maxSquareInclusion";
            maxSquareInclusion.Size = new Size(114, 24);
            maxSquareInclusion.TabIndex = 10;
            maxSquareInclusion.Text = "500";
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label15.Location = new Point(32, 144);
            label15.Name = "label15";
            label15.Size = new Size(358, 17);
            label15.TabIndex = 9;
            label15.Text = "Максимальная площадь вкрапления (от 1 до 10000):";
            // 
            // minSquareInclusion
            // 
            minSquareInclusion.Location = new Point(35, 107);
            minSquareInclusion.Margin = new Padding(3, 4, 3, 4);
            minSquareInclusion.Name = "minSquareInclusion";
            minSquareInclusion.Size = new Size(114, 24);
            minSquareInclusion.TabIndex = 8;
            minSquareInclusion.Text = "200";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label14.Location = new Point(32, 85);
            label14.Name = "label14";
            label14.Size = new Size(352, 17);
            label14.TabIndex = 7;
            label14.Text = "Минимальная площадь вкрапления (от 1 до 10000):";
            // 
            // circleCoefTx
            // 
            circleCoefTx.Location = new Point(35, 53);
            circleCoefTx.Margin = new Padding(3, 4, 3, 4);
            circleCoefTx.Name = "circleCoefTx";
            circleCoefTx.Size = new Size(114, 24);
            circleCoefTx.TabIndex = 6;
            circleCoefTx.Text = "0,8";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label13.Location = new Point(32, 32);
            label13.Name = "label13";
            label13.Size = new Size(337, 17);
            label13.TabIndex = 5;
            label13.Text = "Коэффициент округлости вкраплений (от 0 до 1):";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(ovalityCoef);
            groupBox1.Controls.Add(label12);
            groupBox1.Location = new Point(7, 15);
            groupBox1.Margin = new Padding(3, 4, 3, 4);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(3, 4, 3, 4);
            groupBox1.Size = new Size(350, 92);
            groupBox1.TabIndex = 28;
            groupBox1.TabStop = false;
            groupBox1.Text = "Параметры для определения овальности";
            // 
            // ovalityCoef
            // 
            ovalityCoef.Location = new Point(35, 53);
            ovalityCoef.Margin = new Padding(3, 4, 3, 4);
            ovalityCoef.Name = "ovalityCoef";
            ovalityCoef.Size = new Size(114, 24);
            ovalityCoef.TabIndex = 6;
            ovalityCoef.Text = "0,7";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label12.Location = new Point(32, 32);
            label12.Name = "label12";
            label12.Size = new Size(185, 17);
            label12.TabIndex = 5;
            label12.Text = "Коэффициент овальности:";
            // 
            // obduvCB
            // 
            obduvCB.AutoSize = true;
            obduvCB.Location = new Point(254, 1473);
            obduvCB.Margin = new Padding(3, 4, 3, 4);
            obduvCB.Name = "obduvCB";
            obduvCB.Size = new Size(143, 24);
            obduvCB.TabIndex = 35;
            obduvCB.Text = "Включить обдув";
            obduvCB.UseVisualStyleBackColor = true;
            // 
            // prStatus
            // 
            prStatus.AutoSize = true;
            prStatus.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            prStatus.Location = new Point(112, 40);
            prStatus.Name = "prStatus";
            prStatus.Size = new Size(91, 17);
            prStatus.TabIndex = 34;
            prStatus.Text = "Подключено";
            // 
            // camStatus
            // 
            camStatus.AutoSize = true;
            camStatus.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            camStatus.Location = new Point(112, 13);
            camStatus.Name = "camStatus";
            camStatus.Size = new Size(91, 17);
            camStatus.TabIndex = 33;
            camStatus.Text = "Подключено";
            // 
            // label39
            // 
            label39.AutoSize = true;
            label39.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label39.Location = new Point(6, 40);
            label39.Name = "label39";
            label39.Size = new Size(107, 17);
            label39.TabIndex = 32;
            label39.Text = "Статус обдува:";
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label19.Location = new Point(6, 13);
            label19.Name = "label19";
            label19.Size = new Size(111, 17);
            label19.TabIndex = 31;
            label19.Text = "Статус камеры:";
            // 
            // button10
            // 
            button10.BackColor = Color.FromArgb(4, 85, 191);
            button10.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            button10.ForeColor = SystemColors.Control;
            button10.Location = new Point(177, 77);
            button10.Margin = new Padding(3, 4, 3, 4);
            button10.Name = "button10";
            button10.Size = new Size(158, 101);
            button10.TabIndex = 29;
            button10.Text = "Подключиться ПР";
            button10.UseVisualStyleBackColor = false;
            button10.Click += button10_Click;
            // 
            // button11
            // 
            button11.BackColor = Color.FromArgb(4, 85, 191);
            button11.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            button11.ForeColor = SystemColors.Control;
            button11.Location = new Point(3, 77);
            button11.Margin = new Padding(3, 4, 3, 4);
            button11.Name = "button11";
            button11.Size = new Size(162, 101);
            button11.TabIndex = 28;
            button11.Text = "Подключиться камере";
            button11.UseVisualStyleBackColor = false;
            button11.Click += button11_Click;
            // 
            // textBox5
            // 
            textBox5.BackColor = Color.FromArgb(211, 220, 229);
            textBox5.Font = new Font("Segoe UI", 9F);
            textBox5.Location = new Point(178, 35);
            textBox5.Margin = new Padding(3, 4, 3, 4);
            textBox5.Name = "textBox5";
            textBox5.Size = new Size(157, 27);
            textBox5.TabIndex = 27;
            textBox5.Text = "10.10.69.38";
            // 
            // label40
            // 
            label40.AutoSize = true;
            label40.Font = new Font("Segoe UI", 9F);
            label40.Location = new Point(178, 8);
            label40.Name = "label40";
            label40.Size = new Size(119, 20);
            label40.TabIndex = 26;
            label40.Text = "Ip адрес обдува";
            // 
            // textBox6
            // 
            textBox6.BackColor = Color.FromArgb(211, 220, 229);
            textBox6.Font = new Font("Segoe UI", 9F);
            textBox6.Location = new Point(6, 35);
            textBox6.Margin = new Padding(3, 4, 3, 4);
            textBox6.Name = "textBox6";
            textBox6.Size = new Size(157, 27);
            textBox6.TabIndex = 25;
            textBox6.Text = "169.254.205.254";
            // 
            // label41
            // 
            label41.AutoSize = true;
            label41.Font = new Font("Segoe UI", 9F);
            label41.Location = new Point(6, 8);
            label41.Name = "label41";
            label41.Size = new Size(124, 20);
            label41.TabIndex = 24;
            label41.Text = "Ip адрес камеры";
            // 
            // gainTb
            // 
            gainTb.BackColor = Color.FromArgb(211, 220, 229);
            gainTb.Font = new Font("Segoe UI", 9F);
            gainTb.Location = new Point(179, 96);
            gainTb.Margin = new Padding(3, 4, 3, 4);
            gainTb.Name = "gainTb";
            gainTb.Size = new Size(157, 27);
            gainTb.TabIndex = 23;
            gainTb.Text = "100";
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Font = new Font("Segoe UI", 9F);
            label17.Location = new Point(179, 73);
            label17.Name = "label17";
            label17.Size = new Size(114, 20);
            label17.TabIndex = 22;
            label17.Text = "Насыщенность";
            // 
            // saveImageButton
            // 
            saveImageButton.BackColor = Color.FromArgb(4, 85, 191);
            saveImageButton.Font = new Font("Microsoft Sans Serif", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 204);
            saveImageButton.ForeColor = SystemColors.Control;
            saveImageButton.Location = new Point(178, 145);
            saveImageButton.Margin = new Padding(3, 4, 3, 4);
            saveImageButton.Name = "saveImageButton";
            saveImageButton.Size = new Size(74, 61);
            saveImageButton.TabIndex = 21;
            saveImageButton.Text = "📤";
            saveImageButton.UseVisualStyleBackColor = false;
            saveImageButton.Click += saveSettingsButton_Click;
            // 
            // applySettingsButton
            // 
            applySettingsButton.BackColor = Color.FromArgb(4, 85, 191);
            applySettingsButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            applySettingsButton.ForeColor = SystemColors.Control;
            applySettingsButton.Location = new Point(5, 145);
            applySettingsButton.Margin = new Padding(3, 4, 3, 4);
            applySettingsButton.Name = "applySettingsButton";
            applySettingsButton.Size = new Size(162, 61);
            applySettingsButton.TabIndex = 20;
            applySettingsButton.Text = "Применить";
            applySettingsButton.UseVisualStyleBackColor = false;
            applySettingsButton.Click += applySettingsButton_Click;
            // 
            // exposureTb
            // 
            exposureTb.BackColor = Color.FromArgb(211, 220, 229);
            exposureTb.Font = new Font("Segoe UI", 9F);
            exposureTb.Location = new Point(7, 96);
            exposureTb.Margin = new Padding(3, 4, 3, 4);
            exposureTb.Name = "exposureTb";
            exposureTb.Size = new Size(157, 27);
            exposureTb.TabIndex = 15;
            exposureTb.Text = "800";
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Font = new Font("Segoe UI", 9F);
            label20.Location = new Point(7, 73);
            label20.Name = "label20";
            label20.Size = new Size(92, 20);
            label20.TabIndex = 14;
            label20.Text = "Экспозиция";
            // 
            // widthTb
            // 
            widthTb.BackColor = Color.FromArgb(211, 220, 229);
            widthTb.Font = new Font("Segoe UI", 9F);
            widthTb.Location = new Point(182, 29);
            widthTb.Margin = new Padding(3, 4, 3, 4);
            widthTb.Name = "widthTb";
            widthTb.Size = new Size(157, 27);
            widthTb.TabIndex = 13;
            widthTb.Text = "1400";
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Font = new Font("Segoe UI", 9F);
            label21.Location = new Point(182, 7);
            label21.Name = "label21";
            label21.Size = new Size(111, 20);
            label21.TabIndex = 12;
            label21.Text = "Ширина кадра";
            // 
            // heightTb
            // 
            heightTb.BackColor = Color.FromArgb(211, 220, 229);
            heightTb.Font = new Font("Segoe UI", 9F);
            heightTb.Location = new Point(9, 29);
            heightTb.Margin = new Padding(3, 4, 3, 4);
            heightTb.Name = "heightTb";
            heightTb.Size = new Size(157, 27);
            heightTb.TabIndex = 11;
            heightTb.Text = "1700";
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Font = new Font("Segoe UI", 9F);
            label22.Location = new Point(9, 7);
            label22.Name = "label22";
            label22.Size = new Size(103, 20);
            label22.TabIndex = 10;
            label22.Text = "Высота кадра";
            // 
            // panel3
            // 
            panel3.BackColor = SystemColors.ButtonHighlight;
            panel3.BorderStyle = BorderStyle.Fixed3D;
            panel3.Controls.Add(panel14);
            panel3.Controls.Add(panel13);
            panel3.Controls.Add(panel12);
            panel3.Controls.Add(panel10);
            panel3.Controls.Add(panel9);
            panel3.Controls.Add(panel6);
            panel3.Location = new Point(407, 16);
            panel3.Margin = new Padding(3, 4, 3, 4);
            panel3.Name = "panel3";
            panel3.Size = new Size(1554, 1103);
            panel3.TabIndex = 2;
            // 
            // panel14
            // 
            panel14.BorderStyle = BorderStyle.Fixed3D;
            panel14.Controls.Add(pixelCount);
            panel14.Controls.Add(obloyCB);
            panel14.Controls.Add(label32);
            panel14.Controls.Add(obloyPb);
            panel14.Controls.Add(obloyTime);
            panel14.Controls.Add(label45);
            panel14.Controls.Add(obloyDef);
            panel14.Controls.Add(label46);
            panel14.Location = new Point(986, 4);
            panel14.Margin = new Padding(3, 4, 3, 4);
            panel14.Name = "panel14";
            panel14.Size = new Size(557, 308);
            panel14.TabIndex = 36;
            // 
            // pixelCount
            // 
            pixelCount.Location = new Point(313, 227);
            pixelCount.Margin = new Padding(3, 4, 3, 4);
            pixelCount.Name = "pixelCount";
            pixelCount.Size = new Size(114, 27);
            pixelCount.TabIndex = 7;
            // 
            // obloyCB
            // 
            obloyCB.AutoSize = true;
            obloyCB.BackColor = Color.Lime;
            obloyCB.Checked = true;
            obloyCB.CheckState = CheckState.Checked;
            obloyCB.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            obloyCB.Location = new Point(313, 24);
            obloyCB.Margin = new Padding(3, 4, 3, 4);
            obloyCB.Name = "obloyCB";
            obloyCB.Size = new Size(221, 32);
            obloyCB.TabIndex = 6;
            obloyCB.Text = "Определение облоя";
            obloyCB.UseVisualStyleBackColor = false;
            // 
            // label32
            // 
            label32.AutoSize = true;
            label32.BackColor = Color.Lime;
            label32.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label32.Location = new Point(313, 29);
            label32.Name = "label32";
            label32.Size = new Size(0, 28);
            label32.TabIndex = 5;
            // 
            // obloyPb
            // 
            obloyPb.BorderStyle = BorderStyle.Fixed3D;
            obloyPb.Location = new Point(3, 5);
            obloyPb.Margin = new Padding(3, 4, 3, 4);
            obloyPb.Name = "obloyPb";
            obloyPb.Size = new Size(302, 289);
            obloyPb.SizeMode = PictureBoxSizeMode.Zoom;
            obloyPb.TabIndex = 0;
            obloyPb.TabStop = false;
            // 
            // obloyTime
            // 
            obloyTime.Location = new Point(314, 108);
            obloyTime.Margin = new Padding(3, 4, 3, 4);
            obloyTime.Name = "obloyTime";
            obloyTime.Size = new Size(114, 27);
            obloyTime.TabIndex = 4;
            // 
            // label45
            // 
            label45.AutoSize = true;
            label45.Location = new Point(314, 79);
            label45.Name = "label45";
            label45.Size = new Size(234, 20);
            label45.TabIndex = 3;
            label45.Text = "Время обработки одного кадра:";
            // 
            // obloyDef
            // 
            obloyDef.Location = new Point(313, 177);
            obloyDef.Margin = new Padding(3, 4, 3, 4);
            obloyDef.Name = "obloyDef";
            obloyDef.Size = new Size(114, 27);
            obloyDef.TabIndex = 2;
            // 
            // label46
            // 
            label46.AutoSize = true;
            label46.Location = new Point(313, 148);
            label46.Name = "label46";
            label46.Size = new Size(249, 20);
            label46.TabIndex = 1;
            label46.Text = "Количество бракованных крышек:";
            // 
            // panel13
            // 
            panel13.BorderStyle = BorderStyle.Fixed3D;
            panel13.Controls.Add(tabControl4);
            panel13.Controls.Add(tabControl3);
            panel13.Controls.Add(tabControl2);
            panel13.Location = new Point(3, 380);
            panel13.Margin = new Padding(3, 4, 3, 4);
            panel13.Name = "panel13";
            panel13.Size = new Size(370, 621);
            panel13.TabIndex = 35;
            // 
            // tabControl4
            // 
            tabControl4.Controls.Add(tabPage5);
            tabControl4.Location = new Point(5, 499);
            tabControl4.Margin = new Padding(3, 4, 3, 4);
            tabControl4.Name = "tabControl4";
            tabControl4.SelectedIndex = 0;
            tabControl4.Size = new Size(358, 112);
            tabControl4.TabIndex = 36;
            // 
            // tabPage5
            // 
            tabPage5.BorderStyle = BorderStyle.Fixed3D;
            tabPage5.Controls.Add(comboBox1);
            tabPage5.Controls.Add(label43);
            tabPage5.Controls.Add(label39);
            tabPage5.Controls.Add(label19);
            tabPage5.Controls.Add(camStatus);
            tabPage5.Controls.Add(prStatus);
            tabPage5.Location = new Point(4, 29);
            tabPage5.Margin = new Padding(3, 4, 3, 4);
            tabPage5.Name = "tabPage5";
            tabPage5.Padding = new Padding(3, 4, 3, 4);
            tabPage5.Size = new Size(350, 79);
            tabPage5.TabIndex = 0;
            tabPage5.Text = "Статусы подключения";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "Желтые", "Синие", "Золотые", "Белые" });
            comboBox1.Location = new Point(216, 25);
            comboBox1.Margin = new Padding(3, 4, 3, 4);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(119, 28);
            comboBox1.TabIndex = 36;
            // 
            // label43
            // 
            label43.AutoSize = true;
            label43.Font = new Font("Segoe UI", 9F);
            label43.Location = new Point(214, 4);
            label43.Name = "label43";
            label43.Size = new Size(100, 20);
            label43.TabIndex = 35;
            label43.Text = "Цвет крышек";
            // 
            // tabControl3
            // 
            tabControl3.Controls.Add(tabPage4);
            tabControl3.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);
            tabControl3.Location = new Point(5, 267);
            tabControl3.Margin = new Padding(3, 4, 3, 4);
            tabControl3.Name = "tabControl3";
            tabControl3.SelectedIndex = 0;
            tabControl3.Size = new Size(360, 229);
            tabControl3.TabIndex = 36;
            // 
            // tabPage4
            // 
            tabPage4.BorderStyle = BorderStyle.Fixed3D;
            tabPage4.Controls.Add(label40);
            tabPage4.Controls.Add(button10);
            tabPage4.Controls.Add(label41);
            tabPage4.Controls.Add(button11);
            tabPage4.Controls.Add(textBox6);
            tabPage4.Controls.Add(textBox5);
            tabPage4.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 204);
            tabPage4.Location = new Point(4, 29);
            tabPage4.Margin = new Padding(3, 4, 3, 4);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(3, 4, 3, 4);
            tabPage4.Size = new Size(352, 196);
            tabPage4.TabIndex = 0;
            tabPage4.Text = "Сетевые настройки камеры и обдува";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // tabControl2
            // 
            tabControl2.Controls.Add(tabPage3);
            tabControl2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);
            tabControl2.Location = new Point(3, 4);
            tabControl2.Margin = new Padding(3, 4, 3, 4);
            tabControl2.Name = "tabControl2";
            tabControl2.SelectedIndex = 0;
            tabControl2.Size = new Size(361, 260);
            tabControl2.TabIndex = 36;
            // 
            // tabPage3
            // 
            tabPage3.BorderStyle = BorderStyle.Fixed3D;
            tabPage3.Controls.Add(loadSettingsButton);
            tabPage3.Controls.Add(label21);
            tabPage3.Controls.Add(saveImageButton);
            tabPage3.Controls.Add(heightTb);
            tabPage3.Controls.Add(applySettingsButton);
            tabPage3.Controls.Add(label22);
            tabPage3.Controls.Add(label17);
            tabPage3.Controls.Add(widthTb);
            tabPage3.Controls.Add(exposureTb);
            tabPage3.Controls.Add(label20);
            tabPage3.Controls.Add(gainTb);
            tabPage3.Location = new Point(4, 29);
            tabPage3.Margin = new Padding(3, 4, 3, 4);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3, 4, 3, 4);
            tabPage3.Size = new Size(353, 227);
            tabPage3.TabIndex = 0;
            tabPage3.Text = "Настройка камеры";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // loadSettingsButton
            // 
            loadSettingsButton.BackColor = Color.FromArgb(4, 85, 191);
            loadSettingsButton.Font = new Font("Microsoft Sans Serif", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 204);
            loadSettingsButton.ForeColor = SystemColors.Control;
            loadSettingsButton.Location = new Point(262, 145);
            loadSettingsButton.Margin = new Padding(3, 4, 3, 4);
            loadSettingsButton.Name = "loadSettingsButton";
            loadSettingsButton.Size = new Size(74, 61);
            loadSettingsButton.TabIndex = 24;
            loadSettingsButton.Text = "📥";
            loadSettingsButton.UseVisualStyleBackColor = false;
            loadSettingsButton.Click += loadSettingsButton_Click;
            // 
            // panel12
            // 
            panel12.BorderStyle = BorderStyle.Fixed3D;
            panel12.Controls.Add(label42);
            panel12.Controls.Add(originPb);
            panel12.Location = new Point(3, 4);
            panel12.Margin = new Padding(3, 4, 3, 4);
            panel12.Name = "panel12";
            panel12.Size = new Size(366, 359);
            panel12.TabIndex = 6;
            // 
            // label42
            // 
            label42.AutoSize = true;
            label42.BackColor = Color.Transparent;
            label42.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label42.Location = new Point(70, 5);
            label42.Name = "label42";
            label42.Size = new Size(276, 28);
            label42.TabIndex = 5;
            label42.Text = "ИЗОБРАЖЕНИЕ С КАМЕРЫ";
            // 
            // originPb
            // 
            originPb.BorderStyle = BorderStyle.Fixed3D;
            originPb.Location = new Point(15, 37);
            originPb.Margin = new Padding(3, 4, 3, 4);
            originPb.Name = "originPb";
            originPb.Size = new Size(337, 309);
            originPb.SizeMode = PictureBoxSizeMode.Zoom;
            originPb.TabIndex = 0;
            originPb.TabStop = false;
            // 
            // panel10
            // 
            panel10.BorderStyle = BorderStyle.Fixed3D;
            panel10.Controls.Add(imageProcDelay);
            panel10.Controls.Add(lan1);
            panel10.Controls.Add(delayTb);
            panel10.Controls.Add(label44);
            panel10.Controls.Add(generalTime);
            panel10.Controls.Add(inpaintCB);
            panel10.Controls.Add(label31);
            panel10.Controls.Add(inpaintTime);
            panel10.Controls.Add(InpaintDef);
            panel10.Controls.Add(label27);
            panel10.Controls.Add(inpaintPb);
            panel10.Controls.Add(label24);
            panel10.Location = new Point(377, 649);
            panel10.Margin = new Padding(3, 4, 3, 4);
            panel10.Name = "panel10";
            panel10.Size = new Size(602, 352);
            panel10.TabIndex = 5;
            // 
            // imageProcDelay
            // 
            imageProcDelay.Location = new Point(371, 285);
            imageProcDelay.Margin = new Padding(3, 4, 3, 4);
            imageProcDelay.Name = "imageProcDelay";
            imageProcDelay.Size = new Size(36, 27);
            imageProcDelay.TabIndex = 15;
            // 
            // lan1
            // 
            lan1.AutoSize = true;
            lan1.Location = new Point(371, 256);
            lan1.Name = "lan1";
            lan1.Size = new Size(30, 20);
            lan1.TabIndex = 14;
            lan1.Text = "GD";
            // 
            // delayTb
            // 
            delayTb.Location = new Point(317, 285);
            delayTb.Margin = new Padding(3, 4, 3, 4);
            delayTb.Name = "delayTb";
            delayTb.Size = new Size(36, 27);
            delayTb.TabIndex = 13;
            delayTb.Text = "25";
            // 
            // label44
            // 
            label44.AutoSize = true;
            label44.Location = new Point(317, 256);
            label44.Name = "label44";
            label44.Size = new Size(17, 20);
            label44.TabIndex = 12;
            label44.Text = "0";
            // 
            // generalTime
            // 
            generalTime.Location = new Point(433, 285);
            generalTime.Margin = new Padding(3, 4, 3, 4);
            generalTime.Name = "generalTime";
            generalTime.Size = new Size(36, 27);
            generalTime.TabIndex = 9;
            // 
            // inpaintCB
            // 
            inpaintCB.AutoSize = true;
            inpaintCB.BackColor = Color.Lime;
            inpaintCB.Checked = true;
            inpaintCB.CheckState = CheckState.Checked;
            inpaintCB.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            inpaintCB.Location = new Point(317, 32);
            inpaintCB.Margin = new Padding(3, 4, 3, 4);
            inpaintCB.Name = "inpaintCB";
            inpaintCB.Size = new Size(286, 32);
            inpaintCB.TabIndex = 9;
            inpaintCB.Text = "Определение непрокрасов";
            inpaintCB.UseVisualStyleBackColor = false;
            inpaintCB.CheckedChanged += inpaintCB_CheckedChanged;
            // 
            // label31
            // 
            label31.AutoSize = true;
            label31.Location = new Point(433, 256);
            label31.Name = "label31";
            label31.Size = new Size(30, 20);
            label31.TabIndex = 8;
            label31.Text = "GD";
            // 
            // inpaintTime
            // 
            inpaintTime.Location = new Point(313, 111);
            inpaintTime.Margin = new Padding(3, 4, 3, 4);
            inpaintTime.Name = "inpaintTime";
            inpaintTime.Size = new Size(114, 27);
            inpaintTime.TabIndex = 8;
            // 
            // InpaintDef
            // 
            InpaintDef.Location = new Point(313, 192);
            InpaintDef.Margin = new Padding(3, 4, 3, 4);
            InpaintDef.Name = "InpaintDef";
            InpaintDef.Size = new Size(114, 27);
            InpaintDef.TabIndex = 6;
            // 
            // label27
            // 
            label27.AutoSize = true;
            label27.Location = new Point(313, 81);
            label27.Name = "label27";
            label27.Size = new Size(234, 20);
            label27.TabIndex = 7;
            label27.Text = "Время обработки одного кадра:";
            // 
            // inpaintPb
            // 
            inpaintPb.BorderStyle = BorderStyle.Fixed3D;
            inpaintPb.Location = new Point(5, 16);
            inpaintPb.Margin = new Padding(3, 4, 3, 4);
            inpaintPb.Name = "inpaintPb";
            inpaintPb.Size = new Size(302, 289);
            inpaintPb.SizeMode = PictureBoxSizeMode.Zoom;
            inpaintPb.TabIndex = 2;
            inpaintPb.TabStop = false;
            // 
            // label24
            // 
            label24.AutoSize = true;
            label24.Location = new Point(313, 163);
            label24.Name = "label24";
            label24.Size = new Size(249, 20);
            label24.TabIndex = 5;
            label24.Text = "Количество бракованных крышек:";
            // 
            // panel9
            // 
            panel9.BorderStyle = BorderStyle.Fixed3D;
            panel9.Controls.Add(inclusionCB);
            panel9.Controls.Add(conclusionTime);
            panel9.Controls.Add(inclusionDef);
            panel9.Controls.Add(label26);
            panel9.Controls.Add(inclusionPb);
            panel9.Controls.Add(label23);
            panel9.Location = new Point(377, 331);
            panel9.Margin = new Padding(3, 4, 3, 4);
            panel9.Name = "panel9";
            panel9.Size = new Size(602, 304);
            panel9.TabIndex = 4;
            // 
            // inclusionCB
            // 
            inclusionCB.AutoSize = true;
            inclusionCB.BackColor = Color.Lime;
            inclusionCB.Checked = true;
            inclusionCB.CheckState = CheckState.Checked;
            inclusionCB.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            inclusionCB.Location = new Point(313, 24);
            inclusionCB.Margin = new Padding(3, 4, 3, 4);
            inclusionCB.Name = "inclusionCB";
            inclusionCB.Size = new Size(276, 32);
            inclusionCB.TabIndex = 7;
            inclusionCB.Text = "Определение вкраплений";
            inclusionCB.UseVisualStyleBackColor = false;
            inclusionCB.CheckedChanged += inclusionCB_CheckedChanged;
            // 
            // conclusionTime
            // 
            conclusionTime.Location = new Point(313, 103);
            conclusionTime.Margin = new Padding(3, 4, 3, 4);
            conclusionTime.Name = "conclusionTime";
            conclusionTime.Size = new Size(114, 27);
            conclusionTime.TabIndex = 6;
            // 
            // inclusionDef
            // 
            inclusionDef.Location = new Point(313, 175);
            inclusionDef.Margin = new Padding(3, 4, 3, 4);
            inclusionDef.Name = "inclusionDef";
            inclusionDef.Size = new Size(114, 27);
            inclusionDef.TabIndex = 4;
            // 
            // label26
            // 
            label26.AutoSize = true;
            label26.Location = new Point(313, 73);
            label26.Name = "label26";
            label26.Size = new Size(234, 20);
            label26.TabIndex = 5;
            label26.Text = "Время обработки одного кадра:";
            // 
            // inclusionPb
            // 
            inclusionPb.BorderStyle = BorderStyle.Fixed3D;
            inclusionPb.Location = new Point(3, 4);
            inclusionPb.Margin = new Padding(3, 4, 3, 4);
            inclusionPb.Name = "inclusionPb";
            inclusionPb.Size = new Size(302, 289);
            inclusionPb.SizeMode = PictureBoxSizeMode.Zoom;
            inclusionPb.TabIndex = 1;
            inclusionPb.TabStop = false;
            // 
            // label23
            // 
            label23.AutoSize = true;
            label23.Location = new Point(313, 145);
            label23.Name = "label23";
            label23.Size = new Size(249, 20);
            label23.TabIndex = 3;
            label23.Text = "Количество бракованных крышек:";
            // 
            // panel6
            // 
            panel6.BorderStyle = BorderStyle.Fixed3D;
            panel6.Controls.Add(ovalityCB);
            panel6.Controls.Add(label30);
            panel6.Controls.Add(ovalityPb);
            panel6.Controls.Add(timeOvality);
            panel6.Controls.Add(label25);
            panel6.Controls.Add(ovalityDef);
            panel6.Controls.Add(label2);
            panel6.Location = new Point(377, 4);
            panel6.Margin = new Padding(3, 4, 3, 4);
            panel6.Name = "panel6";
            panel6.Size = new Size(602, 308);
            panel6.TabIndex = 3;
            // 
            // ovalityCB
            // 
            ovalityCB.AutoSize = true;
            ovalityCB.BackColor = Color.Lime;
            ovalityCB.Checked = true;
            ovalityCB.CheckState = CheckState.Checked;
            ovalityCB.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            ovalityCB.Location = new Point(313, 24);
            ovalityCB.Margin = new Padding(3, 4, 3, 4);
            ovalityCB.Name = "ovalityCB";
            ovalityCB.Size = new Size(271, 32);
            ovalityCB.TabIndex = 6;
            ovalityCB.Text = "Определение овальности";
            ovalityCB.UseVisualStyleBackColor = false;
            ovalityCB.CheckedChanged += ovalityCB_CheckedChanged;
            // 
            // label30
            // 
            label30.AutoSize = true;
            label30.BackColor = Color.Lime;
            label30.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label30.Location = new Point(313, 29);
            label30.Name = "label30";
            label30.Size = new Size(0, 28);
            label30.TabIndex = 5;
            // 
            // ovalityPb
            // 
            ovalityPb.BorderStyle = BorderStyle.Fixed3D;
            ovalityPb.Location = new Point(3, 5);
            ovalityPb.Margin = new Padding(3, 4, 3, 4);
            ovalityPb.Name = "ovalityPb";
            ovalityPb.Size = new Size(302, 289);
            ovalityPb.SizeMode = PictureBoxSizeMode.Zoom;
            ovalityPb.TabIndex = 0;
            ovalityPb.TabStop = false;
            // 
            // timeOvality
            // 
            timeOvality.Location = new Point(314, 108);
            timeOvality.Margin = new Padding(3, 4, 3, 4);
            timeOvality.Name = "timeOvality";
            timeOvality.Size = new Size(114, 27);
            timeOvality.TabIndex = 4;
            // 
            // label25
            // 
            label25.AutoSize = true;
            label25.Location = new Point(314, 79);
            label25.Name = "label25";
            label25.Size = new Size(234, 20);
            label25.TabIndex = 3;
            label25.Text = "Время обработки одного кадра:";
            // 
            // ovalityDef
            // 
            ovalityDef.Location = new Point(313, 177);
            ovalityDef.Margin = new Padding(3, 4, 3, 4);
            ovalityDef.Name = "ovalityDef";
            ovalityDef.Size = new Size(114, 27);
            ovalityDef.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(313, 148);
            label2.Name = "label2";
            label2.Size = new Size(249, 20);
            label2.TabIndex = 1;
            label2.Text = "Количество бракованных крышек:";
            // 
            // obduvBatton
            // 
            obduvBatton.BackColor = Color.FromArgb(4, 85, 191);
            obduvBatton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 204);
            obduvBatton.ForeColor = SystemColors.Control;
            obduvBatton.Location = new Point(222, 1392);
            obduvBatton.Margin = new Padding(3, 4, 3, 4);
            obduvBatton.Name = "obduvBatton";
            obduvBatton.Size = new Size(169, 73);
            obduvBatton.TabIndex = 34;
            obduvBatton.Text = "Включить обдув";
            obduvBatton.UseVisualStyleBackColor = false;
            obduvBatton.Click += obduvBatton_Click_1;
            // 
            // panel4
            // 
            panel4.BorderStyle = BorderStyle.Fixed3D;
            panel4.Controls.Add(label33);
            panel4.Controls.Add(textBox1);
            panel4.Controls.Add(label34);
            panel4.Controls.Add(textBox3);
            panel4.Controls.Add(label35);
            panel4.Controls.Add(pictureBox2);
            panel4.Location = new Point(1027, 1392);
            panel4.Margin = new Padding(3, 4, 3, 4);
            panel4.Name = "panel4";
            panel4.Size = new Size(602, 341);
            panel4.TabIndex = 6;
            // 
            // label33
            // 
            label33.AutoSize = true;
            label33.BackColor = Color.Lime;
            label33.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label33.Location = new Point(313, 29);
            label33.Name = "label33";
            label33.Size = new Size(271, 56);
            label33.TabIndex = 5;
            label33.Text = "Определение ВЫВЕРНУТОЙ \r\nКОРОНКИ";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(313, 137);
            textBox1.Margin = new Padding(3, 4, 3, 4);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(114, 27);
            textBox1.TabIndex = 4;
            // 
            // label34
            // 
            label34.AutoSize = true;
            label34.Location = new Point(313, 108);
            label34.Name = "label34";
            label34.Size = new Size(234, 20);
            label34.TabIndex = 3;
            label34.Text = "Время обработки одного кадра:";
            // 
            // textBox3
            // 
            textBox3.Location = new Point(312, 207);
            textBox3.Margin = new Padding(3, 4, 3, 4);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(114, 27);
            textBox3.TabIndex = 2;
            // 
            // label35
            // 
            label35.AutoSize = true;
            label35.Location = new Point(312, 177);
            label35.Name = "label35";
            label35.Size = new Size(249, 20);
            label35.TabIndex = 1;
            label35.Text = "Количество бракованных крышек:";
            // 
            // pictureBox2
            // 
            pictureBox2.BorderStyle = BorderStyle.Fixed3D;
            pictureBox2.Location = new Point(3, 4);
            pictureBox2.Margin = new Padding(3, 4, 3, 4);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(302, 328);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 0;
            pictureBox2.TabStop = false;
            // 
            // panel11
            // 
            panel11.BorderStyle = BorderStyle.Fixed3D;
            panel11.Controls.Add(ratioTb);
            panel11.Controls.Add(radiusTb);
            panel11.Controls.Add(label36);
            panel11.Controls.Add(timeUnderfill);
            panel11.Controls.Add(label37);
            panel11.Controls.Add(underfillDef);
            panel11.Controls.Add(label38);
            panel11.Controls.Add(underfillPictureBox);
            panel11.Location = new Point(403, 1392);
            panel11.Margin = new Padding(3, 4, 3, 4);
            panel11.Name = "panel11";
            panel11.Size = new Size(602, 341);
            panel11.TabIndex = 7;
            // 
            // ratioTb
            // 
            ratioTb.Location = new Point(408, 303);
            ratioTb.Margin = new Padding(3, 4, 3, 4);
            ratioTb.Name = "ratioTb";
            ratioTb.Size = new Size(114, 27);
            ratioTb.TabIndex = 7;
            // 
            // radiusTb
            // 
            radiusTb.Location = new Point(408, 253);
            radiusTb.Margin = new Padding(3, 4, 3, 4);
            radiusTb.Name = "radiusTb";
            radiusTb.Size = new Size(114, 27);
            radiusTb.TabIndex = 6;
            // 
            // label36
            // 
            label36.AutoSize = true;
            label36.BackColor = Color.Lime;
            label36.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label36.Location = new Point(313, 29);
            label36.Name = "label36";
            label36.Size = new Size(247, 28);
            label36.TabIndex = 5;
            label36.Text = "Определение НЕДОЛИВА";
            // 
            // timeUnderfill
            // 
            timeUnderfill.Location = new Point(313, 137);
            timeUnderfill.Margin = new Padding(3, 4, 3, 4);
            timeUnderfill.Name = "timeUnderfill";
            timeUnderfill.Size = new Size(114, 27);
            timeUnderfill.TabIndex = 4;
            // 
            // label37
            // 
            label37.AutoSize = true;
            label37.Location = new Point(313, 108);
            label37.Name = "label37";
            label37.Size = new Size(234, 20);
            label37.TabIndex = 3;
            label37.Text = "Время обработки одного кадра:";
            // 
            // underfillDef
            // 
            underfillDef.Location = new Point(312, 207);
            underfillDef.Margin = new Padding(3, 4, 3, 4);
            underfillDef.Name = "underfillDef";
            underfillDef.Size = new Size(114, 27);
            underfillDef.TabIndex = 2;
            // 
            // label38
            // 
            label38.AutoSize = true;
            label38.Location = new Point(312, 177);
            label38.Name = "label38";
            label38.Size = new Size(249, 20);
            label38.TabIndex = 1;
            label38.Text = "Количество бракованных крышек:";
            // 
            // underfillPictureBox
            // 
            underfillPictureBox.BorderStyle = BorderStyle.Fixed3D;
            underfillPictureBox.Location = new Point(3, 4);
            underfillPictureBox.Margin = new Padding(3, 4, 3, 4);
            underfillPictureBox.Name = "underfillPictureBox";
            underfillPictureBox.Size = new Size(302, 328);
            underfillPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            underfillPictureBox.TabIndex = 0;
            underfillPictureBox.TabStop = false;
            // 
            // panel7
            // 
            panel7.BackColor = SystemColors.ButtonHighlight;
            panel7.BorderStyle = BorderStyle.Fixed3D;
            panel7.Controls.Add(recognizeButton);
            panel7.Location = new Point(12, 951);
            panel7.Margin = new Padding(3, 4, 3, 4);
            panel7.Name = "panel7";
            panel7.Size = new Size(387, 68);
            panel7.TabIndex = 5;
            // 
            // recognizeButton
            // 
            recognizeButton.BackColor = Color.FromArgb(4, 85, 191);
            recognizeButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 204);
            recognizeButton.ForeColor = SystemColors.Control;
            recognizeButton.Location = new Point(26, 11);
            recognizeButton.Margin = new Padding(3, 4, 3, 4);
            recognizeButton.Name = "recognizeButton";
            recognizeButton.Size = new Size(331, 47);
            recognizeButton.TabIndex = 22;
            recognizeButton.Text = "Начать анализ";
            recognizeButton.UseVisualStyleBackColor = false;
            recognizeButton.Click += recognizeButton_Click;
            // 
            // panel5
            // 
            panel5.BackColor = SystemColors.ButtonHighlight;
            panel5.BorderStyle = BorderStyle.Fixed3D;
            panel5.Controls.Add(endStream);
            panel5.Controls.Add(getImageButton);
            panel5.Location = new Point(208, 807);
            panel5.Margin = new Padding(3, 4, 3, 4);
            panel5.Name = "panel5";
            panel5.Size = new Size(191, 136);
            panel5.TabIndex = 4;
            // 
            // endStream
            // 
            endStream.BackColor = Color.FromArgb(4, 85, 191);
            endStream.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 204);
            endStream.ForeColor = SystemColors.Control;
            endStream.Location = new Point(10, 72);
            endStream.Margin = new Padding(3, 4, 3, 4);
            endStream.Name = "endStream";
            endStream.Size = new Size(169, 47);
            endStream.TabIndex = 25;
            endStream.Text = "Закончить стрим";
            endStream.UseVisualStyleBackColor = false;
            endStream.Click += endStream_Click;
            // 
            // getImageButton
            // 
            getImageButton.BackColor = Color.FromArgb(4, 85, 191);
            getImageButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 204);
            getImageButton.ForeColor = SystemColors.Control;
            getImageButton.Location = new Point(10, 11);
            getImageButton.Margin = new Padding(3, 4, 3, 4);
            getImageButton.Name = "getImageButton";
            getImageButton.Size = new Size(169, 56);
            getImageButton.TabIndex = 24;
            getImageButton.Text = "Получить стрим";
            getImageButton.UseVisualStyleBackColor = false;
            getImageButton.Click += getImageButton_Click_1;
            // 
            // panel8
            // 
            panel8.BackColor = SystemColors.ButtonHighlight;
            panel8.BorderStyle = BorderStyle.Fixed3D;
            panel8.Controls.Add(button3);
            panel8.Controls.Add(loadImageButton);
            panel8.Location = new Point(13, 809);
            panel8.Margin = new Padding(3, 4, 3, 4);
            panel8.Name = "panel8";
            panel8.Size = new Size(188, 136);
            panel8.TabIndex = 25;
            // 
            // button3
            // 
            button3.BackColor = Color.FromArgb(4, 85, 191);
            button3.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 204);
            button3.ForeColor = SystemColors.Control;
            button3.Location = new Point(6, 70);
            button3.Margin = new Padding(3, 4, 3, 4);
            button3.Name = "button3";
            button3.Size = new Size(169, 47);
            button3.TabIndex = 23;
            button3.Text = "Очистить";
            button3.UseVisualStyleBackColor = false;
            button3.Click += button3_Click;
            // 
            // loadImageButton
            // 
            loadImageButton.BackColor = Color.FromArgb(4, 85, 191);
            loadImageButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 204);
            loadImageButton.ForeColor = SystemColors.Control;
            loadImageButton.Location = new Point(6, 11);
            loadImageButton.Margin = new Padding(3, 4, 3, 4);
            loadImageButton.Name = "loadImageButton";
            loadImageButton.Size = new Size(169, 54);
            loadImageButton.TabIndex = 22;
            loadImageButton.Text = "Загрузить";
            loadImageButton.UseVisualStyleBackColor = false;
            loadImageButton.Click += loadImageButton_Click;
            // 
            // MainWorkForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1924, 1055);
            Controls.Add(panel11);
            Controls.Add(obduvCB);
            Controls.Add(panel8);
            Controls.Add(obduvBatton);
            Controls.Add(panel7);
            Controls.Add(panel4);
            Controls.Add(panel5);
            Controls.Add(panel3);
            Controls.Add(panel1);
            Margin = new Padding(3, 4, 3, 4);
            Name = "MainWorkForm";
            Text = "Form2";
            FormClosing += Form2_FormClosing;
            tabParameter.ResumeLayout(false);
            tabParSearch.ResumeLayout(false);
            tabParSearch.PerformLayout();
            tabParCamera.ResumeLayout(false);
            tabParCamera.PerformLayout();
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            tabControl6.ResumeLayout(false);
            tabPage7.ResumeLayout(false);
            tabPage7.PerformLayout();
            tabControl5.ResumeLayout(false);
            tabPage6.ResumeLayout(false);
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            panel3.ResumeLayout(false);
            panel14.ResumeLayout(false);
            panel14.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)obloyPb).EndInit();
            panel13.ResumeLayout(false);
            tabControl4.ResumeLayout(false);
            tabPage5.ResumeLayout(false);
            tabPage5.PerformLayout();
            tabControl3.ResumeLayout(false);
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            tabControl2.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            panel12.ResumeLayout(false);
            panel12.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)originPb).EndInit();
            panel10.ResumeLayout(false);
            panel10.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)inpaintPb).EndInit();
            panel9.ResumeLayout(false);
            panel9.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)inclusionPb).EndInit();
            panel6.ResumeLayout(false);
            panel6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)ovalityPb).EndInit();
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            panel11.ResumeLayout(false);
            panel11.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)underfillPictureBox).EndInit();
            panel7.ResumeLayout(false);
            panel5.ResumeLayout(false);
            panel8.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TabControl tabParameter;
        private TabPage tabParSearch;
        private TabPage tabParCamera;
        private Panel panel1;
        private Label label4;
        private Label label3;
        private Label label1;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private Label label9;
        private Label label10;
        private Label label11;
        private Panel panel2;
        private TabControl tabControl1;
        private TextBox gainTb;
        private Label label17;
        private Button saveImageButton;
        private Button applySettingsButton;
        private TextBox exposureTb;
        private Label label20;
        private TextBox widthTb;
        private Label label21;
        private TextBox heightTb;
        private Label label22;
        private Panel panel3;
        private PictureBox originPb;
        private PictureBox inclusionPb;
        private Panel panel7;
        private Button recognizeButton;
        private Panel panel5;
        private Panel panel8;
        private Button button3;
        private Button loadImageButton;
        private Button endStream;
        private Button getImageButton;
        private PictureBox inpaintPb;
        private Panel panel10;
        private TextBox InpaintDef;
        private Label label24;
        private Panel panel9;
        private TextBox inclusionDef;
        private Label label23;
        private Panel panel6;
        private TextBox ovalityDef;
        private Label label2;
        private TextBox inpaintTime;
        private Label label27;
        private TextBox conclusionTime;
        private Label label26;
        private TextBox timeOvality;
        private Label label25;
        private TabPage tabPage1;
        private GroupBox groupBox4;
        private TextBox whiteThresoldTx;
        private Label label29;
        private Label label28;
        private TextBox minSquareInpaint;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private TextBox textBox2;
        private Label label16;
        private TextBox maxSquareInclusion;
        private Label label15;
        private TextBox minSquareInclusion;
        private Label label14;
        private TextBox circleCoefTx;
        private Label label13;
        private GroupBox groupBox1;
        private TextBox ovalityCoef;
        private Label label12;
        private Button redrawRoi;
        private Button drawRoi;
        private Panel panel4;
        private Panel panel11;
        private Label label36;
        private TextBox timeUnderfill;
        private Label label37;
        private TextBox underfillDef;
        private Label label38;
        private PictureBox underfillPictureBox;
        private Label label33;
        private TextBox textBox1;
        private Label label34;
        private TextBox textBox3;
        private Label label35;
        private PictureBox pictureBox2;
        private Label label30;
        private TextBox ratioTb;
        private TextBox radiusTb;
        private Button obduvBatton;
        private TextBox textBox4;
        private Label label18;
        private Label prStatus;
        private Label camStatus;
        private Label label39;
        private Label label19;
        private CheckBox obduvCB;
        private Button button10;
        private Button button11;
        private TextBox textBox5;
        private Label label40;
        private TextBox textBox6;
        private Label label41;
        private Panel panel13;
        private Panel panel12;
        private Label label42;
        private PictureBox ovalityPb;
        private TabControl tabControl2;
        private TabPage tabPage3;
        private TabControl tabControl3;
        private TabPage tabPage4;
        private TabControl tabControl4;
        private TabPage tabPage5;
        private TabControl tabControl5;
        private TabPage tabPage6;
        private TabControl tabControl6;
        private TabPage tabPage7;
        private Button loadSettingsButton;
        private CheckBox ovalityCB;
        private CheckBox inpaintCB;
        private CheckBox inclusionCB;
        private TextBox generalTime;
        private Label label31;
        private Label label43;
        private ComboBox comboBox1;
        private TextBox delayTb;
        private Label label44;
        private TextBox imageProcDelay;
        private Label lan1;
        private Panel panel14;
        private CheckBox obloyCB;
        private Label label32;
        private PictureBox obloyPb;
        private TextBox obloyTime;
        private Label label45;
        private TextBox obloyDef;
        private Label label46;
        private TextBox pixelCount;
        private GroupBox groupBox5;
        private TextBox obloyPixCount;
        private Label label47;
    }
}