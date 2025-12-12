namespace KrishkiForms
{
    partial class InitializeForm
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
            pictureBox1 = new PictureBox();
            mainPanel = new Panel();
            paramPrConnect = new Button();
            paramCameraConnect = new Button();
            btnRetry = new Button();
            btnExit = new Button();
            btnContinue = new Button();
            prConnectLabel = new Label();
            cameraConectLabel = new Label();
            label1 = new Label();
            HeaderInitializeForm = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            tableLayoutPanel3 = new TableLayoutPanel();
            tableLayoutPanel4 = new TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            mainPanel.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Image = Properties.Resources.лого;
            pictureBox1.Location = new Point(3, 4);
            pictureBox1.Margin = new Padding(3, 4, 3, 4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(82, 77);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // mainPanel
            // 
            mainPanel.BackColor = Color.White;
            mainPanel.BorderStyle = BorderStyle.FixedSingle;
            mainPanel.Controls.Add(tableLayoutPanel4);
            mainPanel.Controls.Add(tableLayoutPanel3);
            mainPanel.Controls.Add(tableLayoutPanel2);
            mainPanel.Controls.Add(tableLayoutPanel1);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new Point(0, 0);
            mainPanel.Margin = new Padding(3, 4, 3, 4);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new Size(496, 476);
            mainPanel.TabIndex = 1;
            // 
            // paramPrConnect
            // 
            paramPrConnect.BackColor = Color.White;
            paramPrConnect.Dock = DockStyle.Right;
            paramPrConnect.FlatAppearance.BorderSize = 0;
            paramPrConnect.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            paramPrConnect.ForeColor = Color.Black;
            paramPrConnect.Location = new Point(124, 55);
            paramPrConnect.Margin = new Padding(3, 4, 3, 4);
            paramPrConnect.Name = "paramPrConnect";
            paramPrConnect.Size = new Size(47, 43);
            paramPrConnect.TabIndex = 41;
            paramPrConnect.Text = "...";
            paramPrConnect.UseVisualStyleBackColor = false;
            paramPrConnect.Click += paramPrConnect_Click;
            // 
            // paramCameraConnect
            // 
            paramCameraConnect.BackColor = Color.White;
            paramCameraConnect.Dock = DockStyle.Right;
            paramCameraConnect.FlatAppearance.BorderSize = 0;
            paramCameraConnect.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            paramCameraConnect.ForeColor = Color.Black;
            paramCameraConnect.Location = new Point(124, 4);
            paramCameraConnect.Margin = new Padding(3, 4, 3, 4);
            paramCameraConnect.Name = "paramCameraConnect";
            paramCameraConnect.Size = new Size(47, 43);
            paramCameraConnect.TabIndex = 40;
            paramCameraConnect.Text = "...";
            paramCameraConnect.UseVisualStyleBackColor = false;
            paramCameraConnect.Click += paramCameraConnect_Click;
            // 
            // btnRetry
            // 
            btnRetry.BackColor = Color.White;
            btnRetry.Dock = DockStyle.Fill;
            btnRetry.FlatAppearance.BorderSize = 0;
            btnRetry.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            btnRetry.ForeColor = Color.Black;
            btnRetry.Location = new Point(116, 4);
            btnRetry.Margin = new Padding(3, 4, 3, 4);
            btnRetry.Name = "btnRetry";
            btnRetry.Size = new Size(107, 75);
            btnRetry.TabIndex = 39;
            btnRetry.Text = "↺";
            btnRetry.UseVisualStyleBackColor = false;
            btnRetry.Click += btnRetry_Click;
            // 
            // btnExit
            // 
            btnExit.BackColor = Color.FromArgb(229, 115, 115);
            btnExit.Dock = DockStyle.Fill;
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            btnExit.ForeColor = Color.White;
            btnExit.Location = new Point(3, 4);
            btnExit.Margin = new Padding(3, 4, 3, 4);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(107, 75);
            btnExit.TabIndex = 38;
            btnExit.Text = "✖";
            btnExit.UseVisualStyleBackColor = false;
            btnExit.Click += btnExit_Click;
            // 
            // btnContinue
            // 
            btnContinue.BackColor = Color.FromArgb(0, 120, 215);
            btnContinue.Dock = DockStyle.Fill;
            btnContinue.FlatAppearance.BorderSize = 0;
            btnContinue.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            btnContinue.ForeColor = Color.White;
            btnContinue.Location = new Point(229, 4);
            btnContinue.Margin = new Padding(3, 4, 3, 4);
            btnContinue.Name = "btnContinue";
            btnContinue.Size = new Size(108, 75);
            btnContinue.TabIndex = 37;
            btnContinue.Text = "➔";
            btnContinue.UseVisualStyleBackColor = false;
            btnContinue.Click += btnContinue_Click;
            // 
            // prConnectLabel
            // 
            prConnectLabel.Anchor = AnchorStyles.Left;
            prConnectLabel.AutoSize = true;
            prConnectLabel.Font = new Font("Segoe UI", 9F);
            prConnectLabel.ForeColor = Color.FromArgb(4, 85, 191);
            prConnectLabel.Location = new Point(177, 66);
            prConnectLabel.Name = "prConnectLabel";
            prConnectLabel.Size = new Size(162, 20);
            prConnectLabel.TabIndex = 5;
            prConnectLabel.Text = "Подключение ПР205...";
            // 
            // cameraConectLabel
            // 
            cameraConectLabel.Anchor = AnchorStyles.Left;
            cameraConectLabel.AutoSize = true;
            cameraConectLabel.Font = new Font("Segoe UI", 9F);
            cameraConectLabel.ForeColor = Color.FromArgb(4, 85, 191);
            cameraConectLabel.Location = new Point(177, 15);
            cameraConectLabel.Name = "cameraConectLabel";
            cameraConectLabel.Size = new Size(173, 20);
            cameraConectLabel.TabIndex = 4;
            cameraConectLabel.Text = "Подключение камеры...";
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.None;
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label1.ForeColor = Color.Black;
            label1.Location = new Point(66, 14);
            label1.Name = "label1";
            label1.Size = new Size(340, 35);
            label1.TabIndex = 3;
            label1.Text = "Инициализация устройств";
            // 
            // HeaderInitializeForm
            // 
            HeaderInitializeForm.Anchor = AnchorStyles.None;
            HeaderInitializeForm.AutoSize = true;
            HeaderInitializeForm.Font = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point, 204);
            HeaderInitializeForm.ForeColor = Color.FromArgb(4, 85, 191);
            HeaderInitializeForm.Location = new Point(99, 25);
            HeaderInitializeForm.Name = "HeaderInitializeForm";
            HeaderInitializeForm.Size = new Size(362, 35);
            HeaderInitializeForm.TabIndex = 2;
            HeaderInitializeForm.Text = "Kvantron.CapDefect.Detector";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 66F));
            tableLayoutPanel1.Controls.Add(pictureBox1, 0, 0);
            tableLayoutPanel1.Controls.Add(HeaderInitializeForm, 1, 0);
            tableLayoutPanel1.Location = new Point(11, 11);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(472, 85);
            tableLayoutPanel1.TabIndex = 42;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(label1, 0, 0);
            tableLayoutPanel2.Location = new Point(11, 102);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.Size = new Size(472, 64);
            tableLayoutPanel2.TabIndex = 43;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36.8644066F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 63.1355934F));
            tableLayoutPanel3.Controls.Add(cameraConectLabel, 1, 0);
            tableLayoutPanel3.Controls.Add(paramCameraConnect, 0, 0);
            tableLayoutPanel3.Controls.Add(paramPrConnect, 0, 1);
            tableLayoutPanel3.Controls.Add(prConnectLabel, 1, 1);
            tableLayoutPanel3.Location = new Point(11, 172);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 2;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.Size = new Size(472, 102);
            tableLayoutPanel3.TabIndex = 44;
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.ColumnCount = 3;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel4.Controls.Add(btnExit, 0, 0);
            tableLayoutPanel4.Controls.Add(btnRetry, 1, 0);
            tableLayoutPanel4.Controls.Add(btnContinue, 2, 0);
            tableLayoutPanel4.Location = new Point(77, 299);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 1;
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.Size = new Size(340, 83);
            tableLayoutPanel4.TabIndex = 45;
            // 
            // InitializeForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(240, 245, 250);
            ClientSize = new Size(496, 476);
            Controls.Add(mainPanel);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(3, 4, 3, 4);
            Name = "InitializeForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Kvantron Cap Defect Detector";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            mainPanel.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            tableLayoutPanel4.ResumeLayout(false);
            ResumeLayout(false);
        }
        #endregion
        private Panel mainPanel;
        private PictureBox pictureBox1;
        private Label label1;
        private Label HeaderInitializeForm;
        private Button btnRetry;
        private Button btnExit;
        private Button btnContinue;
        private Label prConnectLabel;
        private Label cameraConectLabel;
        private Button paramCameraConnect;
        private Button paramPrConnect;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel3;
        private TableLayoutPanel tableLayoutPanel4;
    }
}