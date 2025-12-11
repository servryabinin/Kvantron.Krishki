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
            pictureBox1.Location = new Point(15, 31);
            pictureBox1.Margin = new Padding(3, 4, 3, 4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(81, 67);
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
            mainPanel.Margin = new Padding(3, 4, 3, 4);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new Size(496, 476);
            mainPanel.TabIndex = 1;
            // 
            // paramPrConnect
            // 
            paramPrConnect.BackColor = Color.White;
            paramPrConnect.FlatAppearance.BorderSize = 0;
            paramPrConnect.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            paramPrConnect.ForeColor = Color.Black;
            paramPrConnect.Location = new Point(97, 233);
            paramPrConnect.Margin = new Padding(3, 4, 3, 4);
            paramPrConnect.Name = "paramPrConnect";
            paramPrConnect.Size = new Size(47, 37);
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
            paramCameraConnect.Location = new Point(97, 189);
            paramCameraConnect.Margin = new Padding(3, 4, 3, 4);
            paramCameraConnect.Name = "paramCameraConnect";
            paramCameraConnect.Size = new Size(47, 37);
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
            btnRetry.Location = new Point(202, 319);
            btnRetry.Margin = new Padding(3, 4, 3, 4);
            btnRetry.Name = "btnRetry";
            btnRetry.Size = new Size(74, 68);
            btnRetry.TabIndex = 39;
            btnRetry.Text = "↺";
            btnRetry.UseVisualStyleBackColor = false;
            btnRetry.Click += btnRetry_Click;
            // 
            // btnExit
            // 
            btnExit.BackColor = Color.FromArgb(229, 115, 115);
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            btnExit.ForeColor = Color.White;
            btnExit.Location = new Point(91, 319);
            btnExit.Margin = new Padding(3, 4, 3, 4);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(97, 68);
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
            btnContinue.Location = new Point(287, 319);
            btnContinue.Margin = new Padding(3, 4, 3, 4);
            btnContinue.Name = "btnContinue";
            btnContinue.Size = new Size(97, 68);
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
            prConnectLabel.Location = new Point(151, 239);
            prConnectLabel.Name = "prConnectLabel";
            prConnectLabel.Size = new Size(162, 20);
            prConnectLabel.TabIndex = 5;
            prConnectLabel.Text = "Подключение ПР205...";
            // 
            // cameraConectLabel
            // 
            cameraConectLabel.AutoSize = true;
            cameraConectLabel.Font = new Font("Segoe UI", 9F);
            cameraConectLabel.ForeColor = Color.FromArgb(0, 51, 102);
            cameraConectLabel.Location = new Point(151, 195);
            cameraConectLabel.Name = "cameraConectLabel";
            cameraConectLabel.Size = new Size(173, 20);
            cameraConectLabel.TabIndex = 4;
            cameraConectLabel.Text = "Подключение камеры...";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            label1.ForeColor = Color.FromArgb(0, 51, 102);
            label1.Location = new Point(51, 128);
            label1.Name = "label1";
            label1.Size = new Size(410, 41);
            label1.TabIndex = 3;
            label1.Text = "Инициализация устройств";
            // 
            // HeaderInitializeForm
            // 
            HeaderInitializeForm.AutoSize = true;
            HeaderInitializeForm.Font = new Font("Segoe UI", 16.2F, FontStyle.Regular, GraphicsUnit.Point, 204);
            HeaderInitializeForm.ForeColor = Color.FromArgb(0, 51, 102);
            HeaderInitializeForm.Location = new Point(113, 43);
            HeaderInitializeForm.Name = "HeaderInitializeForm";
            HeaderInitializeForm.Size = new Size(373, 38);
            HeaderInitializeForm.TabIndex = 2;
            HeaderInitializeForm.Text = "Kvantron.CapDefect.Detector";
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