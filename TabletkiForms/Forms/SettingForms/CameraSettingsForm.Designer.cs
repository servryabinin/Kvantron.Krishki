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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
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
            refreshButton = new Button();
            ((System.ComponentModel.ISupportInitialize)camerasDataGridView).BeginInit();
            SuspendLayout();
            // 
            // camerasDataGridView
            // 
            camerasDataGridView.AllowUserToAddRows = false;
            camerasDataGridView.AllowUserToDeleteRows = false;
            camerasDataGridView.BackgroundColor = Color.WhiteSmoke;
            camerasDataGridView.BorderStyle = BorderStyle.None;
            camerasDataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(0, 120, 215);
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = Color.White;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            camerasDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            camerasDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            camerasDataGridView.Columns.AddRange(new DataGridViewColumn[] { ConnectColumn, ModelColumn, SerialNumberColumn, IPAddressColumn, ConnectionTypeColumn, AvailabilityColumn });
            camerasDataGridView.EnableHeadersVisualStyles = false;
            camerasDataGridView.GridColor = Color.FromArgb(200, 200, 200);
            camerasDataGridView.Location = new Point(14, 36);
            camerasDataGridView.Margin = new Padding(3, 4, 3, 4);
            camerasDataGridView.MultiSelect = false;
            camerasDataGridView.Name = "camerasDataGridView";
            camerasDataGridView.RowHeadersVisible = false;
            camerasDataGridView.RowHeadersWidth = 51;
            camerasDataGridView.RowTemplate.DefaultCellStyle.BackColor = Color.White;
            camerasDataGridView.RowTemplate.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            camerasDataGridView.RowTemplate.DefaultCellStyle.SelectionForeColor = Color.White;
            camerasDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            camerasDataGridView.Size = new Size(815, 267);
            camerasDataGridView.TabIndex = 0;
            camerasDataGridView.CellContentClick += camerasDataGridView_CellContentClick;
            camerasDataGridView.CellFormatting += camerasDataGridView_CellFormatting;
            camerasDataGridView.SelectionChanged += camerasDataGridView_SelectionChanged;
            // 
            // ConnectColumn
            // 
            ConnectColumn.HeaderText = "Подключиться";
            ConnectColumn.MinimumWidth = 6;
            ConnectColumn.Name = "ConnectColumn";
            ConnectColumn.Width = 125;
            // 
            // ModelColumn
            // 
            ModelColumn.HeaderText = "Модель";
            ModelColumn.MinimumWidth = 6;
            ModelColumn.Name = "ModelColumn";
            ModelColumn.ReadOnly = true;
            ModelColumn.Width = 150;
            // 
            // SerialNumberColumn
            // 
            SerialNumberColumn.HeaderText = "Серийный номер";
            SerialNumberColumn.MinimumWidth = 6;
            SerialNumberColumn.Name = "SerialNumberColumn";
            SerialNumberColumn.ReadOnly = true;
            SerialNumberColumn.Width = 150;
            // 
            // IPAddressColumn
            // 
            IPAddressColumn.HeaderText = "IP-адрес / Тип";
            IPAddressColumn.MinimumWidth = 6;
            IPAddressColumn.Name = "IPAddressColumn";
            IPAddressColumn.ReadOnly = true;
            IPAddressColumn.Width = 120;
            // 
            // ConnectionTypeColumn
            // 
            ConnectionTypeColumn.HeaderText = "Тип подключения";
            ConnectionTypeColumn.MinimumWidth = 6;
            ConnectionTypeColumn.Name = "ConnectionTypeColumn";
            ConnectionTypeColumn.ReadOnly = true;
            ConnectionTypeColumn.Width = 120;
            // 
            // AvailabilityColumn
            // 
            AvailabilityColumn.HeaderText = "Доступ к подключению";
            AvailabilityColumn.MinimumWidth = 6;
            AvailabilityColumn.Name = "AvailabilityColumn";
            AvailabilityColumn.ReadOnly = true;
            AvailabilityColumn.Width = 150;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label1.ForeColor = Color.Black;
            label1.Location = new Point(14, 12);
            label1.Name = "label1";
            label1.Size = new Size(177, 23);
            label1.TabIndex = 1;
            label1.Text = "Доступные камеры:";
            // 
            // manualSNTextBox
            // 
            manualSNTextBox.BackColor = Color.White;
            manualSNTextBox.BorderStyle = BorderStyle.FixedSingle;
            manualSNTextBox.Font = new Font("Segoe UI", 9F);
            manualSNTextBox.Location = new Point(14, 340);
            manualSNTextBox.Margin = new Padding(3, 4, 3, 4);
            manualSNTextBox.Name = "manualSNTextBox";
            manualSNTextBox.PlaceholderText = "Введите серийный номер камеры";
            manualSNTextBox.Size = new Size(815, 27);
            manualSNTextBox.TabIndex = 2;
            manualSNTextBox.TextChanged += manualSNTextBox_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label2.ForeColor = Color.Black;
            label2.Location = new Point(14, 309);
            label2.Name = "label2";
            label2.Size = new Size(349, 23);
            label2.TabIndex = 3;
            label2.Text = "Или введите серийный номер вручную:";
            // 
            // okButton
            // 
            okButton.BackColor = Color.FromArgb(0, 120, 215);
            okButton.FlatAppearance.BorderSize = 0;
            okButton.FlatStyle = FlatStyle.Flat;
            okButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            okButton.ForeColor = Color.White;
            okButton.Location = new Point(734, 385);
            okButton.Margin = new Padding(3, 4, 3, 4);
            okButton.Name = "okButton";
            okButton.Size = new Size(95, 37);
            okButton.TabIndex = 4;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = false;
            okButton.Click += okButton_Click;
            // 
            // refreshButton
            // 
            refreshButton.BackColor = Color.FromArgb(0, 120, 215);
            refreshButton.FlatAppearance.BorderSize = 0;
            refreshButton.FlatStyle = FlatStyle.Flat;
            refreshButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            refreshButton.ForeColor = Color.White;
            refreshButton.Location = new Point(14, 385);
            refreshButton.Margin = new Padding(3, 4, 3, 4);
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new Size(95, 37);
            refreshButton.TabIndex = 6;
            refreshButton.Text = "Обновить";
            refreshButton.UseVisualStyleBackColor = false;
            refreshButton.Click += refreshButton_Click;
            // 
            // CameraSettingsForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(839, 432);
            Controls.Add(refreshButton);
            Controls.Add(okButton);
            Controls.Add(label2);
            Controls.Add(manualSNTextBox);
            Controls.Add(label1);
            Controls.Add(camerasDataGridView);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CameraSettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Настройка подключения к камере";
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
        private Button refreshButton;
        private DataGridViewCheckBoxColumn ConnectColumn;
        private DataGridViewTextBoxColumn ModelColumn;
        private DataGridViewTextBoxColumn SerialNumberColumn;
        private DataGridViewTextBoxColumn IPAddressColumn;
        private DataGridViewTextBoxColumn ConnectionTypeColumn;
        private DataGridViewTextBoxColumn AvailabilityColumn;
    }
}