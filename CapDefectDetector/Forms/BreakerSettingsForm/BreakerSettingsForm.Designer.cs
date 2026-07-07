namespace CapDefectDetector.Forms.BreakerSettingsForm
{
    partial class BreakerSettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BreakerSettingsForm));
            tableLayoutPanel1 = new TableLayoutPanel();
            label2 = new Label();
            tableLayoutPanel2 = new TableLayoutPanel();
            groupBox2 = new GroupBox();
            tableLayoutPanel4 = new TableLayoutPanel();
            breakerSettingsForm_mmOnStepTb = new TextBox();
            label6 = new Label();
            label7 = new Label();
            distanceFromSensorToCameraLb = new Label();
            distanceFromSensorToBreakerLb = new Label();
            breakerSettingsForm_stepOnMmTb = new TextBox();
            breakerSettingsForm_breakerOffsetTb = new TextBox();
            breakerSettingsForm_cameraOffsetTb = new TextBox();
            groupBox1 = new GroupBox();
            tableLayoutPanel3 = new TableLayoutPanel();
            breakerSettingsForm_distanceFromSensorToBreakerNuD = new NumericUpDown();
            breakerSettingsForm_distanceFromSensorToCameraNuD = new NumericUpDown();
            breakerSettingsForm_encoderBitrateNuD = new NumericUpDown();
            label1 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            breakerSettingsForm_diameterEncoderWheelNuD = new NumericUpDown();
            tableLayoutPanel5 = new TableLayoutPanel();
            tableLayoutPanel6 = new TableLayoutPanel();
            breakerSettingsForm_cancelButton = new Button();
            breakerSettingsForm_okButton = new Button();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            groupBox2.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            groupBox1.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)breakerSettingsForm_distanceFromSensorToBreakerNuD).BeginInit();
            ((System.ComponentModel.ISupportInitialize)breakerSettingsForm_distanceFromSensorToCameraNuD).BeginInit();
            ((System.ComponentModel.ISupportInitialize)breakerSettingsForm_encoderBitrateNuD).BeginInit();
            ((System.ComponentModel.ISupportInitialize)breakerSettingsForm_diameterEncoderWheelNuD).BeginInit();
            tableLayoutPanel5.SuspendLayout();
            tableLayoutPanel6.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(label2, 0, 0);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 1);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel5, 0, 2);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 9.208103F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 79.43723F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 11.4718618F));
            tableLayoutPanel1.Size = new Size(399, 462);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // label2
            // 
            label2.BackColor = Color.FromArgb(4, 85, 191);
            label2.BorderStyle = BorderStyle.Fixed3D;
            label2.Dock = DockStyle.Fill;
            label2.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label2.ForeColor = Color.White;
            label2.Location = new Point(0, 0);
            label2.Margin = new Padding(0);
            label2.Name = "label2";
            label2.Size = new Size(399, 42);
            label2.TabIndex = 2;
            label2.Text = "Рассчет расстояний для отбраковки";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(groupBox2, 0, 1);
            tableLayoutPanel2.Controls.Add(groupBox1, 0, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 45);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Size = new Size(393, 360);
            tableLayoutPanel2.TabIndex = 3;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(tableLayoutPanel4);
            groupBox2.Dock = DockStyle.Fill;
            groupBox2.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            groupBox2.Location = new Point(3, 183);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(387, 174);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Рассчитанные значения";
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.CellBorderStyle = TableLayoutPanelCellBorderStyle.OutsetPartial;
            tableLayoutPanel4.ColumnCount = 2;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel4.Controls.Add(breakerSettingsForm_mmOnStepTb, 1, 0);
            tableLayoutPanel4.Controls.Add(label6, 0, 0);
            tableLayoutPanel4.Controls.Add(label7, 0, 1);
            tableLayoutPanel4.Controls.Add(distanceFromSensorToCameraLb, 0, 2);
            tableLayoutPanel4.Controls.Add(distanceFromSensorToBreakerLb, 0, 3);
            tableLayoutPanel4.Controls.Add(breakerSettingsForm_stepOnMmTb, 1, 1);
            tableLayoutPanel4.Controls.Add(breakerSettingsForm_breakerOffsetTb, 1, 3);
            tableLayoutPanel4.Controls.Add(breakerSettingsForm_cameraOffsetTb, 1, 2);
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(3, 18);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 4;
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel4.Size = new Size(381, 153);
            tableLayoutPanel4.TabIndex = 0;
            // 
            // breakerSettingsForm_mmOnStepTb
            // 
            breakerSettingsForm_mmOnStepTb.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            breakerSettingsForm_mmOnStepTb.Enabled = false;
            breakerSettingsForm_mmOnStepTb.Location = new Point(195, 9);
            breakerSettingsForm_mmOnStepTb.Name = "breakerSettingsForm_mmOnStepTb";
            breakerSettingsForm_mmOnStepTb.Size = new Size(180, 22);
            breakerSettingsForm_mmOnStepTb.TabIndex = 27;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Left;
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 8.25F);
            label6.Location = new Point(6, 13);
            label6.Name = "label6";
            label6.Size = new Size(59, 13);
            label6.TabIndex = 1;
            label6.Text = "Мм/шаги:";
            // 
            // label7
            // 
            label7.Anchor = AnchorStyles.Left;
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 8.25F);
            label7.Location = new Point(6, 50);
            label7.Name = "label7";
            label7.Size = new Size(59, 13);
            label7.TabIndex = 2;
            label7.Text = "Шаги/мм:";
            // 
            // distanceFromSensorToCameraLb
            // 
            distanceFromSensorToCameraLb.Anchor = AnchorStyles.Left;
            distanceFromSensorToCameraLb.AutoSize = true;
            distanceFromSensorToCameraLb.Font = new Font("Segoe UI", 7.8F);
            distanceFromSensorToCameraLb.ForeColor = Color.Black;
            distanceFromSensorToCameraLb.Location = new Point(6, 81);
            distanceFromSensorToCameraLb.Name = "distanceFromSensorToCameraLb";
            distanceFromSensorToCameraLb.Size = new Size(147, 26);
            distanceFromSensorToCameraLb.TabIndex = 21;
            distanceFromSensorToCameraLb.Text = "Расстояние от датчика до камеры, шаги:";
            distanceFromSensorToCameraLb.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // distanceFromSensorToBreakerLb
            // 
            distanceFromSensorToBreakerLb.Anchor = AnchorStyles.Left;
            distanceFromSensorToBreakerLb.AutoSize = true;
            distanceFromSensorToBreakerLb.Font = new Font("Segoe UI", 7.8F);
            distanceFromSensorToBreakerLb.Location = new Point(6, 119);
            distanceFromSensorToBreakerLb.Name = "distanceFromSensorToBreakerLb";
            distanceFromSensorToBreakerLb.Size = new Size(178, 26);
            distanceFromSensorToBreakerLb.TabIndex = 23;
            distanceFromSensorToBreakerLb.Text = "Расстояние от датчика до сдува, шаги:";
            distanceFromSensorToBreakerLb.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // breakerSettingsForm_stepOnMmTb
            // 
            breakerSettingsForm_stepOnMmTb.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            breakerSettingsForm_stepOnMmTb.Enabled = false;
            breakerSettingsForm_stepOnMmTb.Location = new Point(195, 46);
            breakerSettingsForm_stepOnMmTb.Name = "breakerSettingsForm_stepOnMmTb";
            breakerSettingsForm_stepOnMmTb.Size = new Size(180, 22);
            breakerSettingsForm_stepOnMmTb.TabIndex = 24;
            // 
            // breakerSettingsForm_breakerOffsetTb
            // 
            breakerSettingsForm_breakerOffsetTb.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            breakerSettingsForm_breakerOffsetTb.Enabled = false;
            breakerSettingsForm_breakerOffsetTb.Location = new Point(195, 121);
            breakerSettingsForm_breakerOffsetTb.Name = "breakerSettingsForm_breakerOffsetTb";
            breakerSettingsForm_breakerOffsetTb.Size = new Size(180, 22);
            breakerSettingsForm_breakerOffsetTb.TabIndex = 26;
            // 
            // breakerSettingsForm_cameraOffsetTb
            // 
            breakerSettingsForm_cameraOffsetTb.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            breakerSettingsForm_cameraOffsetTb.Enabled = false;
            breakerSettingsForm_cameraOffsetTb.Location = new Point(195, 83);
            breakerSettingsForm_cameraOffsetTb.Name = "breakerSettingsForm_cameraOffsetTb";
            breakerSettingsForm_cameraOffsetTb.Size = new Size(180, 22);
            breakerSettingsForm_cameraOffsetTb.TabIndex = 25;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(tableLayoutPanel3);
            groupBox1.Dock = DockStyle.Fill;
            groupBox1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);
            groupBox1.Location = new Point(3, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(387, 174);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Значения для ввода";
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.CellBorderStyle = TableLayoutPanelCellBorderStyle.OutsetPartial;
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.Controls.Add(breakerSettingsForm_distanceFromSensorToBreakerNuD, 1, 3);
            tableLayoutPanel3.Controls.Add(breakerSettingsForm_distanceFromSensorToCameraNuD, 1, 2);
            tableLayoutPanel3.Controls.Add(breakerSettingsForm_encoderBitrateNuD, 1, 1);
            tableLayoutPanel3.Controls.Add(label1, 0, 0);
            tableLayoutPanel3.Controls.Add(label3, 0, 1);
            tableLayoutPanel3.Controls.Add(label4, 0, 2);
            tableLayoutPanel3.Controls.Add(label5, 0, 3);
            tableLayoutPanel3.Controls.Add(breakerSettingsForm_diameterEncoderWheelNuD, 1, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(3, 19);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 4;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel3.Size = new Size(381, 152);
            tableLayoutPanel3.TabIndex = 0;
            // 
            // breakerSettingsForm_distanceFromSensorToBreakerNuD
            // 
            breakerSettingsForm_distanceFromSensorToBreakerNuD.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            breakerSettingsForm_distanceFromSensorToBreakerNuD.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            breakerSettingsForm_distanceFromSensorToBreakerNuD.Location = new Point(195, 120);
            breakerSettingsForm_distanceFromSensorToBreakerNuD.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            breakerSettingsForm_distanceFromSensorToBreakerNuD.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            breakerSettingsForm_distanceFromSensorToBreakerNuD.Name = "breakerSettingsForm_distanceFromSensorToBreakerNuD";
            breakerSettingsForm_distanceFromSensorToBreakerNuD.Size = new Size(180, 23);
            breakerSettingsForm_distanceFromSensorToBreakerNuD.TabIndex = 7;
            breakerSettingsForm_distanceFromSensorToBreakerNuD.Value = new decimal(new int[] { 1, 0, 0, 0 });
            breakerSettingsForm_distanceFromSensorToBreakerNuD.ValueChanged += breakerSettingsForm_distanceFromSensorToBreakerNuD_ValueChanged;
            // 
            // breakerSettingsForm_distanceFromSensorToCameraNuD
            // 
            breakerSettingsForm_distanceFromSensorToCameraNuD.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            breakerSettingsForm_distanceFromSensorToCameraNuD.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            breakerSettingsForm_distanceFromSensorToCameraNuD.Location = new Point(195, 83);
            breakerSettingsForm_distanceFromSensorToCameraNuD.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            breakerSettingsForm_distanceFromSensorToCameraNuD.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            breakerSettingsForm_distanceFromSensorToCameraNuD.Name = "breakerSettingsForm_distanceFromSensorToCameraNuD";
            breakerSettingsForm_distanceFromSensorToCameraNuD.Size = new Size(180, 22);
            breakerSettingsForm_distanceFromSensorToCameraNuD.TabIndex = 6;
            breakerSettingsForm_distanceFromSensorToCameraNuD.Value = new decimal(new int[] { 1, 0, 0, 0 });
            breakerSettingsForm_distanceFromSensorToCameraNuD.ValueChanged += breakerSettingsForm_distanceFromSensorToCameraNuD_ValueChanged;
            // 
            // breakerSettingsForm_encoderBitrateNuD
            // 
            breakerSettingsForm_encoderBitrateNuD.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            breakerSettingsForm_encoderBitrateNuD.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            breakerSettingsForm_encoderBitrateNuD.Location = new Point(195, 45);
            breakerSettingsForm_encoderBitrateNuD.Maximum = new decimal(new int[] { 15000, 0, 0, 0 });
            breakerSettingsForm_encoderBitrateNuD.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            breakerSettingsForm_encoderBitrateNuD.Name = "breakerSettingsForm_encoderBitrateNuD";
            breakerSettingsForm_encoderBitrateNuD.Size = new Size(180, 23);
            breakerSettingsForm_encoderBitrateNuD.TabIndex = 5;
            breakerSettingsForm_encoderBitrateNuD.Value = new decimal(new int[] { 1, 0, 0, 0 });
            breakerSettingsForm_encoderBitrateNuD.ValueChanged += breakerSettingsForm_encoderBitrateNuD_ValueChanged;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 8.25F);
            label1.Location = new Point(6, 13);
            label1.Name = "label1";
            label1.Size = new Size(171, 13);
            label1.TabIndex = 0;
            label1.Text = "Диаметр колеса энкодера, мм:";
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Left;
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 8.25F);
            label3.Location = new Point(6, 50);
            label3.Name = "label3";
            label3.Size = new Size(180, 13);
            label3.TabIndex = 1;
            label3.Text = "Разрядность энкодера, шаги/об:";
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Left;
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 8.25F);
            label4.Location = new Point(6, 81);
            label4.Name = "label4";
            label4.Size = new Size(147, 26);
            label4.TabIndex = 2;
            label4.Text = "Расстояние от датчика до камеры, мм:";
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Left;
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 8.25F);
            label5.Location = new Point(6, 118);
            label5.Name = "label5";
            label5.Size = new Size(178, 26);
            label5.TabIndex = 3;
            label5.Text = "Расстояние от датчика до сдува, мм:";
            // 
            // breakerSettingsForm_diameterEncoderWheelNuD
            // 
            breakerSettingsForm_diameterEncoderWheelNuD.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            breakerSettingsForm_diameterEncoderWheelNuD.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            breakerSettingsForm_diameterEncoderWheelNuD.Location = new Point(195, 9);
            breakerSettingsForm_diameterEncoderWheelNuD.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            breakerSettingsForm_diameterEncoderWheelNuD.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            breakerSettingsForm_diameterEncoderWheelNuD.Name = "breakerSettingsForm_diameterEncoderWheelNuD";
            breakerSettingsForm_diameterEncoderWheelNuD.Size = new Size(180, 22);
            breakerSettingsForm_diameterEncoderWheelNuD.TabIndex = 4;
            breakerSettingsForm_diameterEncoderWheelNuD.Value = new decimal(new int[] { 1, 0, 0, 0 });
            breakerSettingsForm_diameterEncoderWheelNuD.ValueChanged += breakerSettingsForm_diameterEncoderWheelNuD_ValueChanged;
            // 
            // tableLayoutPanel5
            // 
            tableLayoutPanel5.ColumnCount = 2;
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel5.Controls.Add(tableLayoutPanel6, 1, 0);
            tableLayoutPanel5.Dock = DockStyle.Fill;
            tableLayoutPanel5.Location = new Point(3, 411);
            tableLayoutPanel5.Name = "tableLayoutPanel5";
            tableLayoutPanel5.RowCount = 1;
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel5.Size = new Size(393, 48);
            tableLayoutPanel5.TabIndex = 4;
            // 
            // tableLayoutPanel6
            // 
            tableLayoutPanel6.ColumnCount = 2;
            tableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel6.Controls.Add(breakerSettingsForm_cancelButton, 1, 0);
            tableLayoutPanel6.Controls.Add(breakerSettingsForm_okButton, 0, 0);
            tableLayoutPanel6.Location = new Point(199, 3);
            tableLayoutPanel6.Name = "tableLayoutPanel6";
            tableLayoutPanel6.RowCount = 1;
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel6.Size = new Size(191, 42);
            tableLayoutPanel6.TabIndex = 0;
            // 
            // breakerSettingsForm_cancelButton
            // 
            breakerSettingsForm_cancelButton.BackColor = Color.FromArgb(66, 133, 244);
            breakerSettingsForm_cancelButton.Dock = DockStyle.Fill;
            breakerSettingsForm_cancelButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);
            breakerSettingsForm_cancelButton.ForeColor = Color.Transparent;
            breakerSettingsForm_cancelButton.Location = new Point(98, 3);
            breakerSettingsForm_cancelButton.Name = "breakerSettingsForm_cancelButton";
            breakerSettingsForm_cancelButton.Size = new Size(90, 36);
            breakerSettingsForm_cancelButton.TabIndex = 1;
            breakerSettingsForm_cancelButton.Text = "Отмена";
            breakerSettingsForm_cancelButton.UseVisualStyleBackColor = false;
            breakerSettingsForm_cancelButton.Click += breakerSettingsForm_cancelButton_Click;
            // 
            // breakerSettingsForm_okButton
            // 
            breakerSettingsForm_okButton.BackColor = Color.FromArgb(66, 133, 244);
            breakerSettingsForm_okButton.Dock = DockStyle.Fill;
            breakerSettingsForm_okButton.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            breakerSettingsForm_okButton.ForeColor = Color.Transparent;
            breakerSettingsForm_okButton.Location = new Point(3, 3);
            breakerSettingsForm_okButton.Name = "breakerSettingsForm_okButton";
            breakerSettingsForm_okButton.Size = new Size(89, 36);
            breakerSettingsForm_okButton.TabIndex = 0;
            breakerSettingsForm_okButton.Text = "ОК";
            breakerSettingsForm_okButton.UseVisualStyleBackColor = false;
            breakerSettingsForm_okButton.Click += breakerSettingsForm_okButton_Click;
            // 
            // BreakerSettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(399, 462);
            Controls.Add(tableLayoutPanel1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "BreakerSettingsForm";
            Text = "Рассчет расстояний для отбраковки";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            groupBox1.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)breakerSettingsForm_distanceFromSensorToBreakerNuD).EndInit();
            ((System.ComponentModel.ISupportInitialize)breakerSettingsForm_distanceFromSensorToCameraNuD).EndInit();
            ((System.ComponentModel.ISupportInitialize)breakerSettingsForm_encoderBitrateNuD).EndInit();
            ((System.ComponentModel.ISupportInitialize)breakerSettingsForm_diameterEncoderWheelNuD).EndInit();
            tableLayoutPanel5.ResumeLayout(false);
            tableLayoutPanel6.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Label label2;
        private TableLayoutPanel tableLayoutPanel2;
        private GroupBox groupBox2;
        private GroupBox groupBox1;
        private TableLayoutPanel tableLayoutPanel3;
        private Label label1;
        private Label label3;
        private Label label4;
        private Label label5;
        private NumericUpDown breakerSettingsForm_distanceFromSensorToBreakerNuD;
        private NumericUpDown breakerSettingsForm_distanceFromSensorToCameraNuD;
        private NumericUpDown breakerSettingsForm_encoderBitrateNuD;
        private NumericUpDown breakerSettingsForm_diameterEncoderWheelNuD;
        private TableLayoutPanel tableLayoutPanel4;
        private Label label6;
        private Label label7;
        private Label distanceFromSensorToCameraLb;
        private Label distanceFromSensorToBreakerLb;
        private TextBox textBox1;
        private TextBox textBox3;
        private TextBox breakerSettingsForm_mmOnStepTb;
        private TextBox breakerSettingsForm_stepOnMmTb;
        private TextBox breakerSettingsForm_cameraOffsetTb;
        private TextBox breakerSettingsForm_breakerOffsetTb;
        private TableLayoutPanel tableLayoutPanel5;
        private TableLayoutPanel tableLayoutPanel6;
        private Button breakerSettingsForm_cancelButton;
        private Button breakerSettingsForm_okButton;
    }
}