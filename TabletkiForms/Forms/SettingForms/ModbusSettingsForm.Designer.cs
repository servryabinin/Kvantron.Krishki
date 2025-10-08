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
            label1.Location = new Point(12, 15);
            label1.Name = "label1";
            label1.Size = new Size(62, 15);
            label1.TabIndex = 0;
            label1.Text = "IP-адрес:";
            // 
            // ipTextBox
            // 
            ipTextBox.Location = new Point(80, 12);
            ipTextBox.Name = "ipTextBox";
            ipTextBox.Size = new Size(150, 23);
            ipTextBox.TabIndex = 1;
            ipTextBox.Text = "10.10.69.38";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 44);
            label2.Name = "label2";
            label2.Size = new Size(38, 15);
            label2.TabIndex = 2;
            label2.Text = "Порт:";
            // 
            // portNumericUpDown
            // 
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
            okButton.Location = new Point(155, 81);
            okButton.Name = "okButton";
            okButton.Size = new Size(75, 23);
            okButton.TabIndex = 4;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += okButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.Location = new Point(236, 81);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 23);
            cancelButton.TabIndex = 5;
            cancelButton.Text = "Отмена";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += cancelButton_Click;
            // 
            // testConnectionButton
            // 
            testConnectionButton.Location = new Point(12, 81);
            testConnectionButton.Name = "testConnectionButton";
            testConnectionButton.Size = new Size(137, 23);
            testConnectionButton.TabIndex = 6;
            testConnectionButton.Text = "Тест подключения";
            testConnectionButton.UseVisualStyleBackColor = true;
            testConnectionButton.Click += testConnectionButton_Click;
            // 
            // ModbusSettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(323, 116);
            Controls.Add(testConnectionButton);
            Controls.Add(cancelButton);
            Controls.Add(okButton);
            Controls.Add(portNumericUpDown);
            Controls.Add(label2);
            Controls.Add(ipTextBox);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
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