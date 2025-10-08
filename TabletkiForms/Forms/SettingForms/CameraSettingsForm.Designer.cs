namespace KrishkiForms
{
    partial class CameraSettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CameraSettingsForm));
            camerasDataGridView = new DataGridView();
            ConnectColumn = new DataGridViewCheckBoxColumn();
            ModelColumn = new DataGridViewTextBoxColumn();
            SerialNumberColumn = new DataGridViewTextBoxColumn();
            IPAddressColumn = new DataGridViewTextBoxColumn();
            ConnectionTypeColumn = new DataGridViewTextBoxColumn();
            AvailabilityColumn = new DataGridViewTextBoxColumn();
            label1 = new Label();
            manualSNTextBox = new TextBox();
            label2 = new Label();
            okButton = new Button();
            cancelButton = new Button();
            refreshButton = new Button();
            ((System.ComponentModel.ISupportInitialize)camerasDataGridView).BeginInit();
            SuspendLayout();
            // 
            // camerasDataGridView
            // 
            camerasDataGridView.AllowUserToAddRows = false;
            camerasDataGridView.AllowUserToDeleteRows = false;
            camerasDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            camerasDataGridView.Columns.AddRange(new DataGridViewColumn[] { ConnectColumn, ModelColumn, SerialNumberColumn, IPAddressColumn, ConnectionTypeColumn, AvailabilityColumn });
            camerasDataGridView.Location = new Point(12, 27);
            camerasDataGridView.MultiSelect = false;
            camerasDataGridView.Name = "camerasDataGridView";
            camerasDataGridView.RowHeadersVisible = false;
            camerasDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            camerasDataGridView.Size = new Size(744, 200);
            camerasDataGridView.TabIndex = 0;
            camerasDataGridView.CellContentClick += camerasDataGridView_CellContentClick;
            camerasDataGridView.CellFormatting += camerasDataGridView_CellFormatting;
            camerasDataGridView.SelectionChanged += camerasDataGridView_SelectionChanged;
            // 
            // ConnectColumn
            // 
            ConnectColumn.HeaderText = "Подключиться";
            ConnectColumn.Name = "ConnectColumn";
            ConnectColumn.Width = 100;
            // 
            // ModelColumn
            // 
            ModelColumn.HeaderText = "Модель";
            ModelColumn.Name = "ModelColumn";
            ModelColumn.ReadOnly = true;
            ModelColumn.Width = 150;
            // 
            // SerialNumberColumn
            // 
            SerialNumberColumn.HeaderText = "Серийный номер";
            SerialNumberColumn.Name = "SerialNumberColumn";
            SerialNumberColumn.ReadOnly = true;
            SerialNumberColumn.Width = 150;
            // 
            // IPAddressColumn
            // 
            IPAddressColumn.HeaderText = "IP-адрес / Тип";
            IPAddressColumn.Name = "IPAddressColumn";
            IPAddressColumn.ReadOnly = true;
            IPAddressColumn.Width = 120;
            // 
            // ConnectionTypeColumn
            // 
            ConnectionTypeColumn.HeaderText = "Тип подключения";
            ConnectionTypeColumn.Name = "ConnectionTypeColumn";
            ConnectionTypeColumn.ReadOnly = true;
            ConnectionTypeColumn.Width = 120;
            // 
            // AvailabilityColumn
            // 
            AvailabilityColumn.HeaderText = "Доступ к подключению";
            AvailabilityColumn.Name = "AvailabilityColumn";
            AvailabilityColumn.ReadOnly = true;
            AvailabilityColumn.Width = 150;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F);
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(119, 15);
            label1.TabIndex = 1;
            label1.Text = "Доступные камеры:";
            // 
            // manualSNTextBox
            // 
            manualSNTextBox.Font = new Font("Segoe UI", 9F);
            manualSNTextBox.Location = new Point(12, 250);
            manualSNTextBox.Name = "manualSNTextBox";
            manualSNTextBox.PlaceholderText = "Введите серийный номер камеры";
            manualSNTextBox.Size = new Size(744, 23);
            manualSNTextBox.TabIndex = 2;
            manualSNTextBox.TextChanged += manualSNTextBox_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F);
            label2.Location = new Point(12, 232);
            label2.Name = "label2";
            label2.Size = new Size(209, 15);
            label2.TabIndex = 3;
            label2.Text = "Или введите серийный номер вручную:";
            // 
            // okButton
            // 
            okButton.BackColor = Color.Lime;
            okButton.Font = new Font("Segoe UI", 9F);
            okButton.Location = new Point(681, 289);
            okButton.Name = "okButton";
            okButton.Size = new Size(75, 23);
            okButton.TabIndex = 4;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = false;
            okButton.Click += okButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.BackColor = Color.OrangeRed;
            cancelButton.Font = new Font("Segoe UI", 9F);
            cancelButton.Location = new Point(600, 289);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 23);
            cancelButton.TabIndex = 5;
            cancelButton.Text = "Отмена";
            cancelButton.UseVisualStyleBackColor = false;
            cancelButton.Click += cancelButton_Click;
            // 
            // refreshButton
            // 
            refreshButton.BackColor = Color.FromArgb(255, 128, 0);
            refreshButton.Font = new Font("Segoe UI", 9F);
            refreshButton.Location = new Point(12, 289);
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new Size(75, 23);
            refreshButton.TabIndex = 6;
            refreshButton.Text = "Обновить";
            refreshButton.UseVisualStyleBackColor = false;
            refreshButton.Click += refreshButton_Click;
            // 
            // CameraSettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(768, 324);
            Controls.Add(refreshButton);
            Controls.Add(cancelButton);
            Controls.Add(okButton);
            Controls.Add(label2);
            Controls.Add(manualSNTextBox);
            Controls.Add(label1);
            Controls.Add(camerasDataGridView);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CameraSettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Настройка подключения к камере";
            FormClosing += CameraSettingsForm_FormClosing;
            ((System.ComponentModel.ISupportInitialize)camerasDataGridView).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView camerasDataGridView;
        private Label label1;
        private TextBox manualSNTextBox;
        private Label label2;
        private Button okButton;
        private Button cancelButton;
        private Button refreshButton;
        private DataGridViewCheckBoxColumn ConnectColumn;
        private DataGridViewTextBoxColumn ModelColumn;
        private DataGridViewTextBoxColumn SerialNumberColumn;
        private DataGridViewTextBoxColumn IPAddressColumn;
        private DataGridViewTextBoxColumn ConnectionTypeColumn;
        private DataGridViewTextBoxColumn AvailabilityColumn;
    }
}