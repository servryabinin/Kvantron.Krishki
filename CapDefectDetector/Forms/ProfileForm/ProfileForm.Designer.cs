namespace CapDefectDetector.Forms
{
    partial class ProfileForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProfileForm));
            label1 = new Label();
            label2 = new Label();
            profileCmB = new ComboBox();
            profilePasswordTb = new TextBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            okProfileBt = new Button();
            cancelProfileFormBt = new Button();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Font = new Font("Segoe UI", 7.8F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(136, 36);
            label1.TabIndex = 0;
            label1.Text = "Профиль:";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Fill;
            label2.Font = new Font("Segoe UI", 7.8F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label2.Location = new Point(3, 36);
            label2.Name = "label2";
            label2.Size = new Size(136, 34);
            label2.TabIndex = 1;
            label2.Text = "Пароль:";
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // profileCmB
            // 
            profileCmB.Anchor = AnchorStyles.None;
            profileCmB.Font = new Font("Segoe UI", 7.8F, FontStyle.Regular, GraphicsUnit.Point, 204);
            profileCmB.FormattingEnabled = true;
            profileCmB.Items.AddRange(new object[] { "Администратор", "Оператор" });
            profileCmB.Location = new Point(150, 7);
            profileCmB.Margin = new Padding(3, 2, 3, 2);
            profileCmB.Name = "profileCmB";
            profileCmB.Size = new Size(126, 20);
            profileCmB.TabIndex = 2;
            // 
            // profilePasswordTb
            // 
            profilePasswordTb.Anchor = AnchorStyles.None;
            profilePasswordTb.Font = new Font("Segoe UI", 7.8F, FontStyle.Regular, GraphicsUnit.Point, 204);
            profilePasswordTb.Location = new Point(150, 42);
            profilePasswordTb.Margin = new Padding(3, 2, 3, 2);
            profilePasswordTb.Name = "profilePasswordTb";
            profilePasswordTb.Size = new Size(126, 21);
            profilePasswordTb.TabIndex = 3;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(okProfileBt, 0, 2);
            tableLayoutPanel1.Controls.Add(cancelProfileFormBt, 1, 2);
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(profilePasswordTb, 1, 1);
            tableLayoutPanel1.Controls.Add(label2, 0, 1);
            tableLayoutPanel1.Controls.Add(profileCmB, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(3, 2, 3, 2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 30.1470585F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 28.67647F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 40.4411774F));
            tableLayoutPanel1.Size = new Size(284, 121);
            tableLayoutPanel1.TabIndex = 4;
            // 
            // okProfileBt
            // 
            okProfileBt.BackColor = Color.FromArgb(4, 85, 191);
            okProfileBt.Dock = DockStyle.Fill;
            okProfileBt.FlatAppearance.BorderSize = 0;
            okProfileBt.Font = new Font("Segoe UI", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 204);
            okProfileBt.ForeColor = Color.White;
            okProfileBt.Location = new Point(3, 72);
            okProfileBt.Margin = new Padding(3, 2, 3, 2);
            okProfileBt.Name = "okProfileBt";
            okProfileBt.Size = new Size(136, 47);
            okProfileBt.TabIndex = 5;
            okProfileBt.Text = "Ок";
            okProfileBt.UseVisualStyleBackColor = false;
            okProfileBt.Click += okProfileBt_Click;
            // 
            // cancelProfileFormBt
            // 
            cancelProfileFormBt.BackColor = Color.FromArgb(66, 133, 244);
            cancelProfileFormBt.Dock = DockStyle.Fill;
            cancelProfileFormBt.FlatAppearance.BorderSize = 0;
            cancelProfileFormBt.Font = new Font("Segoe UI", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 204);
            cancelProfileFormBt.ForeColor = Color.White;
            cancelProfileFormBt.Location = new Point(145, 72);
            cancelProfileFormBt.Margin = new Padding(3, 2, 3, 2);
            cancelProfileFormBt.Name = "cancelProfileFormBt";
            cancelProfileFormBt.Size = new Size(136, 47);
            cancelProfileFormBt.TabIndex = 5;
            cancelProfileFormBt.Text = "Отмена";
            cancelProfileFormBt.UseVisualStyleBackColor = false;
            cancelProfileFormBt.Click += cancelProfileFormBt_Click;
            // 
            // ProfileForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(284, 121);
            Controls.Add(tableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 2, 3, 2);
            Name = "ProfileForm";
            Text = "Смена профиля";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label label1;
        private Label label2;
        private ComboBox profileCmB;
        private TextBox profilePasswordTb;
        private TableLayoutPanel tableLayoutPanel1;
        private Button okProfileBt;
        private Button cancelProfileFormBt;
    }
}