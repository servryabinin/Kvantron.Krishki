using System.Xml.Linq;
using static Guna.UI2.WinForms.Suite.Descriptions;

namespace KrishkiForms
{
    partial class InitializeForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
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
            paramPrConnect.FlatStyle = FlatStyle.Flat;
            paramPrConnect.Location = new Point(85, 175);
            paramPrConnect.Name = "paramPrConnect";
            paramPrConnect.Size = new Size(41, 23);
            paramPrConnect.TabIndex = 41;
            paramPrConnect.Text = "...";
            paramPrConnect.UseVisualStyleBackColor = true;
            paramPrConnect.Click += paramPrConnect_Click;
            // 
            // paramCameraConnect
            // 
            paramCameraConnect.FlatStyle = FlatStyle.Flat;
            paramCameraConnect.Location = new Point(85, 142);
            paramCameraConnect.Name = "paramCameraConnect";
            paramCameraConnect.Size = new Size(41, 23);
            paramCameraConnect.TabIndex = 40;
            paramCameraConnect.Text = "...";
            paramCameraConnect.UseVisualStyleBackColor = true;
            paramCameraConnect.Click += paramCameraConnect_Click;
            // 
            // btnRetry
            // 
            btnRetry.FlatAppearance.BorderSize = 0;
            btnRetry.FlatStyle = FlatStyle.Flat;
            btnRetry.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            btnRetry.Location = new Point(183, 239);
            btnRetry.Name = "btnRetry";
            btnRetry.Size = new Size(53, 51);
            btnRetry.TabIndex = 39;
            btnRetry.Text = "↺";
            btnRetry.UseVisualStyleBackColor = true;
            btnRetry.Click += btnRetry_Click;
            // 
            // btnExit
            // 
            btnExit.BackColor = Color.Tomato;
            btnExit.Font = new Font("Segoe MDL2 Assets", 21.75F, FontStyle.Bold);
            btnExit.ForeColor = Color.White;
            btnExit.Location = new Point(80, 239);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(85, 51);
            btnExit.TabIndex = 38;
            btnExit.Text = "";
            btnExit.UseVisualStyleBackColor = false;
            btnExit.Click += btnExit_Click;
            // 
            // btnContinue
            // 
            btnContinue.BackColor = Color.LimeGreen;
            btnContinue.Font = new Font("Segoe MDL2 Assets", 21.75F, FontStyle.Bold);
            btnContinue.ForeColor = Color.White;
            btnContinue.Location = new Point(251, 239);
            btnContinue.Name = "btnContinue";
            btnContinue.Size = new Size(85, 51);
            btnContinue.TabIndex = 37;
            btnContinue.Text = "";
            btnContinue.UseVisualStyleBackColor = false;
            btnContinue.Click += btnContinue_Click;
            // 
            // prConnectLabel
            // 
            prConnectLabel.AutoSize = true;
            prConnectLabel.ForeColor = SystemColors.ControlDarkDark;
            prConnectLabel.Location = new Point(132, 179);
            prConnectLabel.Name = "prConnectLabel";
            prConnectLabel.Size = new Size(151, 15);
            prConnectLabel.TabIndex = 5;
            prConnectLabel.Text = "[...] Подключение ПР205...";
            // 
            // cameraConectLabel
            // 
            cameraConectLabel.AutoSize = true;
            cameraConectLabel.ForeColor = SystemColors.ControlDarkDark;
            cameraConectLabel.Location = new Point(132, 146);
            cameraConectLabel.Name = "cameraConectLabel";
            cameraConectLabel.Size = new Size(160, 15);
            cameraConectLabel.TabIndex = 4;
            cameraConectLabel.Text = "[...] Подключение камеры...";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label1.Location = new Point(45, 96);
            label1.Name = "label1";
            label1.Size = new Size(331, 32);
            label1.TabIndex = 3;
            label1.Text = "Инициализация устройств";
            // 
            // HeaderInitializeForm
            // 
            HeaderInitializeForm.AutoSize = true;
            HeaderInitializeForm.Font = new Font("Segoe UI", 18F, FontStyle.Regular, GraphicsUnit.Point, 204);
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