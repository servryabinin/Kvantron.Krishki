namespace KrishkiForms
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
            ((System.ComponentModel.ISupportInitialize)portNumericUpDown).BeginInit();
            tableLayoutPanel1.SuspendLayout();
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
            label1.Size = new Size(81, 23);
            label1.TabIndex = 0;
            label1.Text = "IP-адрес:";
            // 
            // ipTextBox
            // 
            ipTextBox.BackColor = Color.White;
            ipTextBox.BorderStyle = BorderStyle.FixedSingle;
            ipTextBox.Font = new Font("Segoe UI", 9F);
            ipTextBox.Location = new Point(175, 4);
            ipTextBox.Margin = new Padding(3, 4, 3, 4);
            ipTextBox.Name = "ipTextBox";
            ipTextBox.Size = new Size(167, 27);
            ipTextBox.TabIndex = 1;
            ipTextBox.Text = "10.10.69.38";
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label2.ForeColor = Color.Black;
            label2.Location = new Point(3, 45);
            label2.Name = "label2";
            label2.Size = new Size(53, 23);
            label2.TabIndex = 2;
            label2.Text = "Порт:";
            // 
            // portNumericUpDown
            // 
            portNumericUpDown.BackColor = Color.White;
            portNumericUpDown.BorderStyle = BorderStyle.FixedSingle;
            portNumericUpDown.Font = new Font("Segoe UI", 9F);
            portNumericUpDown.Location = new Point(175, 42);
            portNumericUpDown.Margin = new Padding(3, 4, 3, 4);
            portNumericUpDown.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            portNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            portNumericUpDown.Name = "portNumericUpDown";
            portNumericUpDown.Size = new Size(167, 27);
            portNumericUpDown.TabIndex = 3;
            portNumericUpDown.Value = new decimal(new int[] { 502, 0, 0, 0 });
            // 
            // okButton
            // 
            okButton.BackColor = Color.FromArgb(66, 133, 244);
            okButton.FlatAppearance.BorderSize = 0;
            okButton.FlatStyle = FlatStyle.Flat;
            okButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            okButton.ForeColor = Color.White;
            okButton.Location = new Point(177, 108);
            okButton.Margin = new Padding(3, 4, 3, 4);
            okButton.Name = "okButton";
            okButton.Size = new Size(86, 37);
            okButton.TabIndex = 4;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = false;
            okButton.Click += okButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.BackColor = Color.FromArgb(229, 115, 115);
            cancelButton.FlatAppearance.BorderSize = 0;
            cancelButton.FlatStyle = FlatStyle.Flat;
            cancelButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            cancelButton.ForeColor = Color.White;
            cancelButton.Location = new Point(270, 108);
            cancelButton.Margin = new Padding(3, 4, 3, 4);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(86, 37);
            cancelButton.TabIndex = 5;
            cancelButton.Text = "Отмена";
            cancelButton.UseVisualStyleBackColor = false;
            cancelButton.Click += cancelButton_Click;
            // 
            // testConnectionButton
            // 
            testConnectionButton.BackColor = Color.FromArgb(66, 133, 244);
            testConnectionButton.FlatAppearance.BorderSize = 0;
            testConnectionButton.FlatStyle = FlatStyle.Flat;
            testConnectionButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            testConnectionButton.ForeColor = Color.White;
            testConnectionButton.Location = new Point(14, 108);
            testConnectionButton.Margin = new Padding(3, 4, 3, 4);
            testConnectionButton.Name = "testConnectionButton";
            testConnectionButton.Size = new Size(157, 37);
            testConnectionButton.TabIndex = 6;
            testConnectionButton.Text = "Тест подключения";
            testConnectionButton.UseVisualStyleBackColor = false;
            testConnectionButton.Click += testConnectionButton_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(label2, 0, 1);
            tableLayoutPanel1.Controls.Add(ipTextBox, 1, 0);
            tableLayoutPanel1.Controls.Add(portNumericUpDown, 1, 1);
            tableLayoutPanel1.Location = new Point(12, 12);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new Size(345, 76);
            tableLayoutPanel1.TabIndex = 7;
            // 
            // ModbusSettingsForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(369, 155);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(testConnectionButton);
            Controls.Add(cancelButton);
            Controls.Add(okButton);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ModbusSettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Настройка подключения к ПР205";
            ((System.ComponentModel.ISupportInitialize)portNumericUpDown).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
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
    }
}