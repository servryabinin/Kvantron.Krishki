namespace CapDefectDetector
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
            label1 = new Label();
            manualSNTextBox = new TextBox();
            label2 = new Label();
            okButton = new Button();
            refreshButton = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            ConnectColumn = new DataGridViewCheckBoxColumn();
            ModelColumn = new DataGridViewTextBoxColumn();
            SerialNumberColumn = new DataGridViewTextBoxColumn();
            IPAddressColumn = new DataGridViewTextBoxColumn();
            ConnectionTypeColumn = new DataGridViewTextBoxColumn();
            AvailabilityColumn = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)camerasDataGridView).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
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
            camerasDataGridView.Dock = DockStyle.Fill;
            camerasDataGridView.EnableHeadersVisualStyles = false;
            camerasDataGridView.GridColor = Color.FromArgb(200, 200, 200);
            camerasDataGridView.Location = new Point(3, 24);
            camerasDataGridView.MultiSelect = false;
            camerasDataGridView.Name = "camerasDataGridView";
            camerasDataGridView.RowHeadersVisible = false;
            camerasDataGridView.RowHeadersWidth = 51;
            camerasDataGridView.RowTemplate.DefaultCellStyle.BackColor = Color.White;
            camerasDataGridView.RowTemplate.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            camerasDataGridView.RowTemplate.DefaultCellStyle.SelectionForeColor = Color.White;
            camerasDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            camerasDataGridView.Size = new Size(792, 183);
            camerasDataGridView.TabIndex = 0;
            camerasDataGridView.CellContentClick += camerasDataGridView_CellContentClick;
            camerasDataGridView.CellFormatting += camerasDataGridView_CellFormatting;
            camerasDataGridView.SelectionChanged += camerasDataGridView_SelectionChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Left;
            label1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label1.ForeColor = Color.Black;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(150, 21);
            label1.TabIndex = 1;
            label1.Text = "Доступные камеры:";
            // 
            // manualSNTextBox
            // 
            manualSNTextBox.BackColor = Color.White;
            manualSNTextBox.BorderStyle = BorderStyle.FixedSingle;
            manualSNTextBox.Dock = DockStyle.Fill;
            manualSNTextBox.Font = new Font("Segoe UI", 9F);
            manualSNTextBox.Location = new Point(3, 234);
            manualSNTextBox.Name = "manualSNTextBox";
            manualSNTextBox.PlaceholderText = "Введите серийный номер камеры";
            manualSNTextBox.Size = new Size(792, 23);
            manualSNTextBox.TabIndex = 2;
            manualSNTextBox.TextChanged += manualSNTextBox_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label2.ForeColor = Color.Black;
            label2.Location = new Point(3, 210);
            label2.Name = "label2";
            label2.Size = new Size(292, 19);
            label2.TabIndex = 3;
            label2.Text = "Или введите серийный номер вручную:";
            // 
            // okButton
            // 
            okButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            okButton.BackColor = Color.FromArgb(0, 120, 215);
            okButton.FlatAppearance.BorderSize = 0;
            okButton.FlatStyle = FlatStyle.Flat;
            okButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            okButton.ForeColor = Color.White;
            okButton.Location = new Point(706, 3);
            okButton.Name = "okButton";
            okButton.Size = new Size(83, 27);
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
            refreshButton.Location = new Point(3, 3);
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new Size(83, 27);
            refreshButton.TabIndex = 6;
            refreshButton.Text = "Обновить";
            refreshButton.UseVisualStyleBackColor = false;
            refreshButton.Click += refreshButton_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 4);
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(camerasDataGridView, 0, 1);
            tableLayoutPanel1.Controls.Add(label2, 0, 2);
            tableLayoutPanel1.Controls.Add(manualSNTextBox, 0, 3);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 5;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 7F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 63F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 7F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 13F));
            tableLayoutPanel1.Size = new Size(798, 300);
            tableLayoutPanel1.TabIndex = 7;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 2;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Controls.Add(refreshButton, 0, 0);
            tableLayoutPanel2.Controls.Add(okButton, 1, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 264);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.Size = new Size(792, 33);
            tableLayoutPanel2.TabIndex = 8;
            // 
            // ConnectColumn
            // 
            ConnectColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            ConnectColumn.HeaderText = "Подключиться";
            ConnectColumn.MinimumWidth = 6;
            ConnectColumn.Name = "ConnectColumn";
            // 
            // ModelColumn
            // 
            ModelColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            ModelColumn.HeaderText = "Модель";
            ModelColumn.MinimumWidth = 6;
            ModelColumn.Name = "ModelColumn";
            ModelColumn.ReadOnly = true;
            // 
            // SerialNumberColumn
            // 
            SerialNumberColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            SerialNumberColumn.HeaderText = "Серийный номер";
            SerialNumberColumn.MinimumWidth = 6;
            SerialNumberColumn.Name = "SerialNumberColumn";
            SerialNumberColumn.ReadOnly = true;
            // 
            // IPAddressColumn
            // 
            IPAddressColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            IPAddressColumn.HeaderText = "IP-адрес / Тип";
            IPAddressColumn.MinimumWidth = 6;
            IPAddressColumn.Name = "IPAddressColumn";
            IPAddressColumn.ReadOnly = true;
            // 
            // ConnectionTypeColumn
            // 
            ConnectionTypeColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            ConnectionTypeColumn.HeaderText = "Тип подключения";
            ConnectionTypeColumn.MinimumWidth = 6;
            ConnectionTypeColumn.Name = "ConnectionTypeColumn";
            ConnectionTypeColumn.ReadOnly = true;
            // 
            // AvailabilityColumn
            // 
            AvailabilityColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            AvailabilityColumn.HeaderText = "Доступ к подключению";
            AvailabilityColumn.MinimumWidth = 6;
            AvailabilityColumn.Name = "AvailabilityColumn";
            AvailabilityColumn.ReadOnly = true;
            // 
            // CameraSettingsForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(798, 300);
            Controls.Add(tableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CameraSettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Настройка подключения к камере";
            FormClosing += CameraSettingsForm_FormClosing;
            ((System.ComponentModel.ISupportInitialize)camerasDataGridView).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            ResumeLayout(false);
        }
        #endregion
        private DataGridView camerasDataGridView;
        private Label label1;
        private TextBox manualSNTextBox;
        private Label label2;
        private Button okButton;
        private Button refreshButton;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private DataGridViewCheckBoxColumn ConnectColumn;
        private DataGridViewTextBoxColumn ModelColumn;
        private DataGridViewTextBoxColumn SerialNumberColumn;
        private DataGridViewTextBoxColumn IPAddressColumn;
        private DataGridViewTextBoxColumn ConnectionTypeColumn;
        private DataGridViewTextBoxColumn AvailabilityColumn;
    }
}