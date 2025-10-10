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
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            mainPanel.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.лого;
            pictureBox1.Location = new Point(13, 23);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(71, 50);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // mainPanel
            // 
            mainPanel.BackColor = Color.White;
            mainPanel.BorderStyle = BorderStyle.FixedSingle;
            mainPanel.Controls.Add(paramPrConnect);
            mainPanel.Controls.Add(paramCameraConnect);
            mainPanel.Controls.Add(btnRetry);
            mainPanel.Controls.Add(btnExit);
            mainPanel.Controls.Add(btnContinue);
            mainPanel.Controls.Add(prConnectLabel);
            mainPanel.Controls.Add(cameraConectLabel);
            mainPanel.Controls.Add(label1);
            mainPanel.Controls.Add(HeaderInitializeForm);
            mainPanel.Controls.Add(pictureBox1);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new Point(0, 0);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new Size(434, 357);
            mainPanel.TabIndex = 1;
            // 
            // paramPrConnect
            // 
            paramPrConnect.BackColor = Color.White;
            paramPrConnect.FlatAppearance.BorderSize = 0;
            paramPrConnect.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            paramPrConnect.ForeColor = Color.Black;
            paramPrConnect.Location = new Point(85, 175);
            paramPrConnect.Name = "paramPrConnect";
            paramPrConnect.Size = new Size(41, 28);
            paramPrConnect.TabIndex = 41;
            paramPrConnect.Text = "...";
            paramPrConnect.UseVisualStyleBackColor = false;
            paramPrConnect.Click += paramPrConnect_Click;
            // 
            // paramCameraConnect
            // 
            paramCameraConnect.BackColor = Color.White;
            paramCameraConnect.FlatAppearance.BorderSize = 0;
            paramCameraConnect.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            paramCameraConnect.ForeColor = Color.Black;
            paramCameraConnect.Location = new Point(85, 142);
            paramCameraConnect.Name = "paramCameraConnect";
            paramCameraConnect.Size = new Size(41, 28);
            paramCameraConnect.TabIndex = 40;
            paramCameraConnect.Text = "...";
            paramCameraConnect.UseVisualStyleBackColor = false;
            paramCameraConnect.Click += paramCameraConnect_Click;
            // 
            // btnRetry
            // 
            btnRetry.BackColor = Color.White;
            btnRetry.FlatAppearance.BorderSize = 0;
            btnRetry.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            btnRetry.ForeColor = Color.Black;
            btnRetry.Location = new Point(177, 239);
            btnRetry.Name = "btnRetry";
            btnRetry.Size = new Size(65, 51);
            btnRetry.TabIndex = 39;
            btnRetry.Text = "↺";
            btnRetry.UseVisualStyleBackColor = false;
            btnRetry.Click += btnRetry_Click;
            // 
            // btnExit
            // 
            btnExit.BackColor = Color.FromArgb(220, 53, 69);
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            btnExit.ForeColor = Color.White;
            btnExit.Location = new Point(80, 239);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(85, 51);
            btnExit.TabIndex = 38;
            btnExit.Text = "✖";
            btnExit.UseVisualStyleBackColor = false;
            btnExit.Click += btnExit_Click;
            // 
            // btnContinue
            // 
            btnContinue.BackColor = Color.FromArgb(0, 120, 215);
            btnContinue.FlatAppearance.BorderSize = 0;
            btnContinue.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            btnContinue.ForeColor = Color.White;
            btnContinue.Location = new Point(251, 239);
            btnContinue.Name = "btnContinue";
            btnContinue.Size = new Size(85, 51);
            btnContinue.TabIndex = 37;
            btnContinue.Text = "➔";
            btnContinue.UseVisualStyleBackColor = false;
            btnContinue.Click += btnContinue_Click;
            // 
            // prConnectLabel
            // 
            prConnectLabel.AutoSize = true;
            prConnectLabel.Font = new Font("Segoe UI", 9F);
            prConnectLabel.ForeColor = Color.FromArgb(0, 51, 102);
            prConnectLabel.Location = new Point(132, 179);
            prConnectLabel.Name = "prConnectLabel";
            prConnectLabel.Size = new Size(131, 15);
            prConnectLabel.TabIndex = 5;
            prConnectLabel.Text = "Подключение ПР205...";
            // 
            // cameraConectLabel
            // 
            cameraConectLabel.AutoSize = true;
            cameraConectLabel.Font = new Font("Segoe UI", 9F);
            cameraConectLabel.ForeColor = Color.FromArgb(0, 51, 102);
            cameraConectLabel.Location = new Point(132, 146);
            cameraConectLabel.Name = "cameraConectLabel";
            cameraConectLabel.Size = new Size(140, 15);
            cameraConectLabel.TabIndex = 4;
            cameraConectLabel.Text = "Подключение камеры...";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            label1.ForeColor = Color.FromArgb(0, 51, 102);
            label1.Location = new Point(45, 96);
            label1.Name = "label1";
            label1.Size = new Size(331, 32);
            label1.TabIndex = 3;
            label1.Text = "Инициализация устройств";
            // 
            // HeaderInitializeForm
            // 
            HeaderInitializeForm.AutoSize = true;
            HeaderInitializeForm.Font = new Font("Segoe UI", 18F);
            HeaderInitializeForm.ForeColor = Color.FromArgb(0, 51, 102);
            HeaderInitializeForm.Location = new Point(99, 32);
            HeaderInitializeForm.Name = "HeaderInitializeForm";
            HeaderInitializeForm.Size = new Size(322, 32);
            HeaderInitializeForm.TabIndex = 2;
            HeaderInitializeForm.Text = "Kvantron.CapDefect.Detector";
            // 
            // InitializeForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(240, 245, 250);
            ClientSize = new Size(434, 357);
            Controls.Add(mainPanel);
            FormBorderStyle = FormBorderStyle.None;
            Name = "InitializeForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Kvantron Cap Defect Detector";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            mainPanel.ResumeLayout(false);
            mainPanel.PerformLayout();
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
    }
}