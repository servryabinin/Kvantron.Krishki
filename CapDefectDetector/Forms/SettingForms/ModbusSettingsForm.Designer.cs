namespace CapDefectDetector
{
    partial class ModbusSettingsForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModbusSettingsForm));
            label1 = new Label();
            ipTextBox = new TextBox();
            label2 = new Label();
            portNumericUpDown = new NumericUpDown();
            okButton = new Button();
            cancelButton = new Button();
            testConnectionButton = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            tableLayoutPanel3 = new TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)portNumericUpDown).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label1.ForeColor = Color.Black;
            label1.Location = new Point(3, 7);
            label1.Name = "label1";
            label1.Size = new Size(66, 19);
            label1.TabIndex = 0;
            label1.Text = "IP-адрес:";
            // 
            // ipTextBox
            // 
            ipTextBox.BackColor = Color.White;
            ipTextBox.BorderStyle = BorderStyle.FixedSingle;
            ipTextBox.Font = new Font("Segoe UI", 9F);
            ipTextBox.Location = new Point(141, 3);
            ipTextBox.Name = "ipTextBox";
            ipTextBox.Size = new Size(130, 23);
            ipTextBox.TabIndex = 1;
            ipTextBox.Text = "10.10.69.38";
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label2.ForeColor = Color.Black;
            label2.Location = new Point(3, 40);
            label2.Name = "label2";
            label2.Size = new Size(44, 19);
            label2.TabIndex = 2;
            label2.Text = "Порт:";
            // 
            // portNumericUpDown
            // 
            portNumericUpDown.BackColor = Color.White;
            portNumericUpDown.BorderStyle = BorderStyle.FixedSingle;
            portNumericUpDown.Font = new Font("Segoe UI", 9F);
            portNumericUpDown.Location = new Point(141, 36);
            portNumericUpDown.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            portNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            portNumericUpDown.Name = "portNumericUpDown";
            portNumericUpDown.Size = new Size(130, 23);
            portNumericUpDown.TabIndex = 3;
            portNumericUpDown.Value = new decimal(new int[] { 502, 0, 0, 0 });
            // 
            // okButton
            // 
            okButton.BackColor = Color.FromArgb(66, 133, 244);
            okButton.Dock = DockStyle.Fill;
            okButton.FlatAppearance.BorderSize = 0;
            okButton.FlatStyle = FlatStyle.Flat;
            okButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            okButton.ForeColor = Color.White;
            okButton.Location = new Point(141, 3);
            okButton.Name = "okButton";
            okButton.Size = new Size(56, 26);
            okButton.TabIndex = 4;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = false;
            okButton.Click += okButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.BackColor = Color.FromArgb(229, 115, 115);
            cancelButton.Dock = DockStyle.Fill;
            cancelButton.FlatAppearance.BorderSize = 0;
            cancelButton.FlatStyle = FlatStyle.Flat;
            cancelButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            cancelButton.ForeColor = Color.White;
            cancelButton.Location = new Point(203, 3);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(65, 26);
            cancelButton.TabIndex = 5;
            cancelButton.Text = "Отмена";
            cancelButton.UseVisualStyleBackColor = false;
            cancelButton.Click += cancelButton_Click;
            // 
            // testConnectionButton
            // 
            testConnectionButton.BackColor = Color.FromArgb(66, 133, 244);
            testConnectionButton.Dock = DockStyle.Fill;
            testConnectionButton.FlatAppearance.BorderSize = 0;
            testConnectionButton.FlatStyle = FlatStyle.Flat;
            testConnectionButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            testConnectionButton.ForeColor = Color.White;
            testConnectionButton.Location = new Point(3, 3);
            testConnectionButton.Name = "testConnectionButton";
            testConnectionButton.Size = new Size(132, 26);
            testConnectionButton.TabIndex = 6;
            testConnectionButton.Text = "Тест подключения";
            testConnectionButton.UseVisualStyleBackColor = false;
            testConnectionButton.Click += testConnectionButton_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50.3649635F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 49.6350365F));
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(label2, 0, 1);
            tableLayoutPanel1.Controls.Add(ipTextBox, 1, 0);
            tableLayoutPanel1.Controls.Add(portNumericUpDown, 1, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(3, 2);
            tableLayoutPanel1.Margin = new Padding(3, 2, 3, 2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new Size(274, 67);
            tableLayoutPanel1.TabIndex = 7;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.Controls.Add(tableLayoutPanel3, 0, 1);
            tableLayoutPanel2.Controls.Add(tableLayoutPanel1, 0, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(0, 0);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 65.15151F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 34.848484F));
            tableLayoutPanel2.Size = new Size(280, 109);
            tableLayoutPanel2.TabIndex = 8;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 3;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50.92251F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22.87823F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25.8302574F));
            tableLayoutPanel3.Controls.Add(testConnectionButton, 0, 0);
            tableLayoutPanel3.Controls.Add(okButton, 1, 0);
            tableLayoutPanel3.Controls.Add(cancelButton, 2, 0);
            tableLayoutPanel3.Dock = DockStyle.Left;
            tableLayoutPanel3.Location = new Point(3, 74);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel3.Size = new Size(271, 32);
            tableLayoutPanel3.TabIndex = 9;
            // 
            // ModbusSettingsForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(280, 109);
            Controls.Add(tableLayoutPanel2);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ModbusSettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Настройка подключения к ПР205";
            ((System.ComponentModel.ISupportInitialize)portNumericUpDown).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            ResumeLayout(false);
        }
        #endregion
        private Label label1;
        private TextBox ipTextBox;
        private Label label2;
        private NumericUpDown portNumericUpDown;
        private Button okButton;
        private Button cancelButton;
        private Button testConnectionButton;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel3;
    }
}