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
            ((System.ComponentModel.ISupportInitialize)portNumericUpDown).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label1.ForeColor = Color.FromArgb(0, 51, 102);
            label1.Location = new Point(5, 15);
            label1.Name = "label1";
            label1.Size = new Size(73, 19);
            label1.TabIndex = 0;
            label1.Text = "IP-адрес:";
            // 
            // ipTextBox
            // 
            ipTextBox.BackColor = Color.White;
            ipTextBox.BorderStyle = BorderStyle.FixedSingle;
            ipTextBox.Font = new Font("Segoe UI", 9F);
            ipTextBox.Location = new Point(80, 12);
            ipTextBox.Name = "ipTextBox";
            ipTextBox.Size = new Size(150, 23);
            ipTextBox.TabIndex = 1;
            ipTextBox.Text = "10.10.69.38";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label2.ForeColor = Color.FromArgb(0, 51, 102);
            label2.Location = new Point(7, 44);
            label2.Name = "label2";
            label2.Size = new Size(48, 19);
            label2.TabIndex = 2;
            label2.Text = "Порт:";
            // 
            // portNumericUpDown
            // 
            portNumericUpDown.BackColor = Color.White;
            portNumericUpDown.BorderStyle = BorderStyle.FixedSingle;
            portNumericUpDown.Font = new Font("Segoe UI", 9F);
            portNumericUpDown.Location = new Point(80, 42);
            portNumericUpDown.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            portNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            portNumericUpDown.Name = "portNumericUpDown";
            portNumericUpDown.Size = new Size(150, 23);
            portNumericUpDown.TabIndex = 3;
            portNumericUpDown.Value = new decimal(new int[] { 502, 0, 0, 0 });
            // 
            // okButton
            // 
            okButton.BackColor = Color.FromArgb(0, 120, 215);
            okButton.FlatAppearance.BorderSize = 0;
            okButton.FlatStyle = FlatStyle.Flat;
            okButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            okButton.ForeColor = Color.White;
            okButton.Location = new Point(155, 81);
            okButton.Name = "okButton";
            okButton.Size = new Size(75, 28);
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
            cancelButton.Location = new Point(236, 81);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 28);
            cancelButton.TabIndex = 5;
            cancelButton.Text = "Отмена";
            cancelButton.UseVisualStyleBackColor = false;
            cancelButton.Click += cancelButton_Click;
            // 
            // testConnectionButton
            // 
            testConnectionButton.BackColor = Color.FromArgb(0, 120, 215);
            testConnectionButton.FlatAppearance.BorderSize = 0;
            testConnectionButton.FlatStyle = FlatStyle.Flat;
            testConnectionButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            testConnectionButton.ForeColor = Color.White;
            testConnectionButton.Location = new Point(12, 81);
            testConnectionButton.Name = "testConnectionButton";
            testConnectionButton.Size = new Size(137, 28);
            testConnectionButton.TabIndex = 6;
            testConnectionButton.Text = "Тест подключения";
            testConnectionButton.UseVisualStyleBackColor = false;
            testConnectionButton.Click += testConnectionButton_Click;
            // 
            // ModbusSettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(240, 245, 250);
            ClientSize = new Size(323, 116);
            Controls.Add(testConnectionButton);
            Controls.Add(cancelButton);
            Controls.Add(okButton);
            Controls.Add(portNumericUpDown);
            Controls.Add(label2);
            Controls.Add(ipTextBox);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ModbusSettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Настройка подключения к ПР205";
            ((System.ComponentModel.ISupportInitialize)portNumericUpDown).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
        #endregion
        private Label label1;
        private TextBox ipTextBox;
        private Label label2;
        private NumericUpDown portNumericUpDown;
        private Button okButton;
        private Button cancelButton;
        private Button testConnectionButton;
    }
}