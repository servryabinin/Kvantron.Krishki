namespace CapDefectDetector
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
            tableLayoutPanel4 = new TableLayoutPanel();
            btnExit = new Button();
            btnRetry = new Button();
            btnContinue = new Button();
            tableLayoutPanel3 = new TableLayoutPanel();
            cameraConectLabel = new Label();
            paramCameraConnect = new Button();
            paramPrConnect = new Button();
            prConnectLabel = new Label();
            label1 = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            HeaderInitializeForm = new Label();
            tableLayoutPanel5 = new TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            tableLayoutPanel4.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel5.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Image = Properties.Resources.лого;
            pictureBox1.Location = new Point(4, 4);
            pictureBox1.Margin = new Padding(4, 4, 4, 4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(133, 100);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.Anchor = AnchorStyles.None;
            tableLayoutPanel4.ColumnCount = 3;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel4.Controls.Add(btnExit, 0, 0);
            tableLayoutPanel4.Controls.Add(btnRetry, 1, 0);
            tableLayoutPanel4.Controls.Add(btnContinue, 2, 0);
            tableLayoutPanel4.Location = new Point(108, 345);
            tableLayoutPanel4.Margin = new Padding(4, 2, 4, 2);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 1;
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.Size = new Size(348, 72);
            tableLayoutPanel4.TabIndex = 45;
            // 
            // btnExit
            // 
            btnExit.BackColor = Color.FromArgb(229, 115, 115);
            btnExit.Dock = DockStyle.Fill;
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            btnExit.ForeColor = Color.White;
            btnExit.Location = new Point(4, 4);
            btnExit.Margin = new Padding(4, 4, 4, 4);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(108, 64);
            btnExit.TabIndex = 38;
            btnExit.Text = "✖";
            btnExit.UseVisualStyleBackColor = false;
            btnExit.Click += btnExit_Click;
            // 
            // btnRetry
            // 
            btnRetry.BackColor = Color.White;
            btnRetry.Dock = DockStyle.Fill;
            btnRetry.FlatAppearance.BorderSize = 0;
            btnRetry.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            btnRetry.ForeColor = Color.Black;
            btnRetry.Location = new Point(120, 4);
            btnRetry.Margin = new Padding(4, 4, 4, 4);
            btnRetry.Name = "btnRetry";
            btnRetry.Size = new Size(108, 64);
            btnRetry.TabIndex = 39;
            btnRetry.Text = "↺";
            btnRetry.UseVisualStyleBackColor = false;
            btnRetry.Click += btnRetry_Click;
            // 
            // btnContinue
            // 
            btnContinue.BackColor = Color.FromArgb(0, 120, 215);
            btnContinue.Dock = DockStyle.Fill;
            btnContinue.FlatAppearance.BorderSize = 0;
            btnContinue.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            btnContinue.ForeColor = Color.White;
            btnContinue.Location = new Point(236, 4);
            btnContinue.Margin = new Padding(4, 4, 4, 4);
            btnContinue.Name = "btnContinue";
            btnContinue.Size = new Size(108, 64);
            btnContinue.TabIndex = 37;
            btnContinue.Text = "➔";
            btnContinue.UseVisualStyleBackColor = false;
            btnContinue.Click += btnContinue_Click;
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
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(4, 169);
            tableLayoutPanel3.Margin = new Padding(4, 2, 4, 2);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 2;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.Size = new Size(557, 136);
            tableLayoutPanel3.TabIndex = 44;
            // 
            // cameraConectLabel
            // 
            cameraConectLabel.Anchor = AnchorStyles.Left;
            cameraConectLabel.AutoSize = true;
            cameraConectLabel.Font = new Font("Segoe UI", 9F);
            cameraConectLabel.ForeColor = Color.FromArgb(4, 85, 191);
            cameraConectLabel.Location = new Point(209, 24);
            cameraConectLabel.Margin = new Padding(4, 0, 4, 0);
            cameraConectLabel.Name = "cameraConectLabel";
            cameraConectLabel.Size = new Size(173, 20);
            cameraConectLabel.TabIndex = 4;
            cameraConectLabel.Text = "Подключение камеры...";
            // 
            // paramCameraConnect
            // 
            paramCameraConnect.Anchor = AnchorStyles.Right;
            paramCameraConnect.BackColor = Color.White;
            paramCameraConnect.FlatAppearance.BorderSize = 0;
            paramCameraConnect.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            paramCameraConnect.ForeColor = Color.Black;
            paramCameraConnect.Location = new Point(119, 14);
            paramCameraConnect.Margin = new Padding(4, 4, 4, 4);
            paramCameraConnect.Name = "paramCameraConnect";
            paramCameraConnect.Size = new Size(82, 39);
            paramCameraConnect.TabIndex = 40;
            paramCameraConnect.Text = "...";
            paramCameraConnect.UseVisualStyleBackColor = false;
            paramCameraConnect.Click += paramCameraConnect_Click;
            // 
            // paramPrConnect
            // 
            paramPrConnect.Anchor = AnchorStyles.Right;
            paramPrConnect.BackColor = Color.White;
            paramPrConnect.FlatAppearance.BorderSize = 0;
            paramPrConnect.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            paramPrConnect.ForeColor = Color.Black;
            paramPrConnect.Location = new Point(119, 82);
            paramPrConnect.Margin = new Padding(4, 4, 4, 4);
            paramPrConnect.Name = "paramPrConnect";
            paramPrConnect.Size = new Size(82, 39);
            paramPrConnect.TabIndex = 41;
            paramPrConnect.Text = "...";
            paramPrConnect.UseVisualStyleBackColor = false;
            paramPrConnect.Click += paramPrConnect_Click;
            // 
            // prConnectLabel
            // 
            prConnectLabel.Anchor = AnchorStyles.Left;
            prConnectLabel.AutoSize = true;
            prConnectLabel.Font = new Font("Segoe UI", 9F);
            prConnectLabel.ForeColor = Color.FromArgb(4, 85, 191);
            prConnectLabel.Location = new Point(209, 92);
            prConnectLabel.Margin = new Padding(4, 0, 4, 0);
            prConnectLabel.Name = "prConnectLabel";
            prConnectLabel.Size = new Size(162, 20);
            prConnectLabel.TabIndex = 5;
            prConnectLabel.Text = "Подключение ПР205...";
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.None;
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label1.ForeColor = Color.Black;
            label1.Location = new Point(112, 122);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(340, 35);
            label1.TabIndex = 3;
            label1.Text = "Инициализация устройств";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 141F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 389F));
            tableLayoutPanel1.Controls.Add(pictureBox1, 0, 0);
            tableLayoutPanel1.Controls.Add(HeaderInitializeForm, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(4, 2);
            tableLayoutPanel1.Margin = new Padding(4, 2, 4, 2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(557, 108);
            tableLayoutPanel1.TabIndex = 42;
            // 
            // HeaderInitializeForm
            // 
            HeaderInitializeForm.Anchor = AnchorStyles.Left;
            HeaderInitializeForm.AutoSize = true;
            HeaderInitializeForm.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 204);
            HeaderInitializeForm.ForeColor = Color.FromArgb(4, 85, 191);
            HeaderInitializeForm.Location = new Point(145, 35);
            HeaderInitializeForm.Margin = new Padding(4, 0, 4, 0);
            HeaderInitializeForm.Name = "HeaderInitializeForm";
            HeaderInitializeForm.Size = new Size(395, 37);
            HeaderInitializeForm.TabIndex = 2;
            HeaderInitializeForm.Text = "Kvantron.CapDefect.Detector";
            // 
            // tableLayoutPanel5
            // 
            tableLayoutPanel5.ColumnCount = 1;
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 25F));
            tableLayoutPanel5.Controls.Add(label1, 0, 1);
            tableLayoutPanel5.Controls.Add(tableLayoutPanel1, 0, 0);
            tableLayoutPanel5.Controls.Add(tableLayoutPanel3, 0, 2);
            tableLayoutPanel5.Controls.Add(tableLayoutPanel4, 0, 3);
            tableLayoutPanel5.Dock = DockStyle.Fill;
            tableLayoutPanel5.Location = new Point(0, 0);
            tableLayoutPanel5.Margin = new Padding(4, 4, 4, 4);
            tableLayoutPanel5.Name = "tableLayoutPanel5";
            tableLayoutPanel5.RowCount = 4;
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 112F));
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 140F));
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 10F));
            tableLayoutPanel5.Size = new Size(565, 455);
            tableLayoutPanel5.TabIndex = 46;
            // 
            // InitializeForm
            // 
            AutoScaleDimensions = new SizeF(120F, 120F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(565, 455);
            Controls.Add(tableLayoutPanel5);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(4, 4, 4, 4);
            Name = "InitializeForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Kvantron Cap Defect Detector";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel5.ResumeLayout(false);
            tableLayoutPanel5.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
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
        private TableLayoutPanel tableLayoutPanel3;
        private TableLayoutPanel tableLayoutPanel4;
        private TableLayoutPanel tableLayoutPanel5;
    }
}