namespace TabletkiForms
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend3 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea4 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend4 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            originPictureBox = new PictureBox();
            recognizePictureBox = new PictureBox();
            loadImageButton = new Button();
            recognizeButton = new Button();
            label1 = new Label();
            label2 = new Label();
            tabletkiTextBox = new TextBox();
            voidTextBox = new TextBox();
            groupBox1 = new GroupBox();
            button2 = new Button();
            button1 = new Button();
            groupBox2 = new GroupBox();
            groupBox3 = new GroupBox();
            label3 = new Label();
            label4 = new Label();
            brightnessVoid = new TextBox();
            brightnessTabletki = new TextBox();
            chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            groupBox4 = new GroupBox();
            label9 = new Label();
            maxEmptyDistanceTextBox = new TextBox();
            label7 = new Label();
            label8 = new Label();
            textBoxPillWidth = new TextBox();
            textBoxEmptyCell = new TextBox();
            label5 = new Label();
            label6 = new Label();
            textBoxPill = new TextBox();
            textBoxBackground = new TextBox();
            toolTip1 = new ToolTip(components);
            groupBox5 = new GroupBox();
            endStream = new Button();
            getImageButton = new Button();
            button3 = new Button();
            dataGridView1 = new DataGridView();
            chart2 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            dataGridView2 = new DataGridView();
            cameraPictureBox = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)originPictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)recognizePictureBox).BeginInit();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chart1).BeginInit();
            groupBox4.SuspendLayout();
            groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chart2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cameraPictureBox).BeginInit();
            SuspendLayout();
            // 
            // originPictureBox
            // 
            originPictureBox.BackColor = SystemColors.ButtonHighlight;
            originPictureBox.BorderStyle = BorderStyle.Fixed3D;
            originPictureBox.Location = new Point(81, 8);
            originPictureBox.Name = "originPictureBox";
            originPictureBox.Size = new Size(291, 400);
            originPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            originPictureBox.TabIndex = 0;
            originPictureBox.TabStop = false;
            // 
            // recognizePictureBox
            // 
            recognizePictureBox.BackColor = SystemColors.ButtonHighlight;
            recognizePictureBox.BorderStyle = BorderStyle.Fixed3D;
            recognizePictureBox.Location = new Point(455, 8);
            recognizePictureBox.Name = "recognizePictureBox";
            recognizePictureBox.Size = new Size(291, 400);
            recognizePictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            recognizePictureBox.TabIndex = 1;
            recognizePictureBox.TabStop = false;
            // 
            // loadImageButton
            // 
            loadImageButton.BackColor = Color.FromArgb(0, 192, 0);
            loadImageButton.Location = new Point(19, 42);
            loadImageButton.Name = "loadImageButton";
            loadImageButton.Size = new Size(132, 93);
            loadImageButton.TabIndex = 2;
            loadImageButton.Text = "Загрузить";
            loadImageButton.UseVisualStyleBackColor = false;
            loadImageButton.Click += loadImageButton_Click;
            // 
            // recognizeButton
            // 
            recognizeButton.BackColor = Color.FromArgb(192, 192, 0);
            recognizeButton.Location = new Point(6, 20);
            recognizeButton.Name = "recognizeButton";
            recognizeButton.Size = new Size(132, 93);
            recognizeButton.TabIndex = 3;
            recognizeButton.Text = "Найти таблетки методом Хафа";
            recognizeButton.UseVisualStyleBackColor = false;
            recognizeButton.Click += recognizeButton_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(15, 20);
            label1.Name = "label1";
            label1.Size = new Size(127, 15);
            label1.TabIndex = 4;
            label1.Text = "Количество таблеток:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(15, 78);
            label2.Name = "label2";
            label2.Size = new Size(151, 15);
            label2.TabIndex = 5;
            label2.Text = "Количество пустых ячеек:";
            // 
            // tabletkiTextBox
            // 
            tabletkiTextBox.Location = new Point(15, 38);
            tabletkiTextBox.Name = "tabletkiTextBox";
            tabletkiTextBox.Size = new Size(151, 23);
            tabletkiTextBox.TabIndex = 6;
            // 
            // voidTextBox
            // 
            voidTextBox.Location = new Point(15, 96);
            voidTextBox.Name = "voidTextBox";
            voidTextBox.Size = new Size(151, 23);
            voidTextBox.TabIndex = 7;
            // 
            // groupBox1
            // 
            groupBox1.BackColor = SystemColors.ButtonHighlight;
            groupBox1.Controls.Add(recognizeButton);
            groupBox1.Controls.Add(button2);
            groupBox1.Controls.Add(button1);
            groupBox1.Location = new Point(198, 440);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(160, 317);
            groupBox1.TabIndex = 8;
            groupBox1.TabStop = false;
            groupBox1.Text = "Методы поиска таблеток";
            // 
            // button2
            // 
            button2.BackColor = Color.Bisque;
            button2.Location = new Point(6, 118);
            button2.Name = "button2";
            button2.Size = new Size(132, 93);
            button2.TabIndex = 12;
            button2.Text = "Найти таблетки методом изменения интенсивности";
            button2.UseVisualStyleBackColor = false;
            button2.Click += button2_Click;
            // 
            // button1
            // 
            button1.BackColor = Color.FromArgb(255, 192, 192);
            button1.Location = new Point(6, 217);
            button1.Name = "button1";
            button1.Size = new Size(132, 93);
            button1.TabIndex = 4;
            button1.Text = "Построить график изменения интенсивности";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // groupBox2
            // 
            groupBox2.BackColor = SystemColors.ButtonHighlight;
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(label1);
            groupBox2.Controls.Add(voidTextBox);
            groupBox2.Controls.Add(tabletkiTextBox);
            groupBox2.Location = new Point(601, 441);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(200, 131);
            groupBox2.TabIndex = 9;
            groupBox2.TabStop = false;
            groupBox2.Text = "Найденные таблетки";
            // 
            // groupBox3
            // 
            groupBox3.BackColor = SystemColors.ButtonHighlight;
            groupBox3.Controls.Add(label3);
            groupBox3.Controls.Add(label4);
            groupBox3.Controls.Add(brightnessVoid);
            groupBox3.Controls.Add(brightnessTabletki);
            groupBox3.Location = new Point(601, 596);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(200, 131);
            groupBox3.TabIndex = 10;
            groupBox3.TabStop = false;
            groupBox3.Text = "Яркости таблеток";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(15, 78);
            label3.Name = "label3";
            label3.Size = new Size(130, 15);
            label3.TabIndex = 5;
            label3.Text = "Яркость пустых ячеек:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(15, 20);
            label4.Name = "label4";
            label4.Size = new Size(106, 15);
            label4.TabIndex = 4;
            label4.Text = "Яркость таблетки:";
            // 
            // brightnessVoid
            // 
            brightnessVoid.Location = new Point(15, 96);
            brightnessVoid.Name = "brightnessVoid";
            brightnessVoid.Size = new Size(151, 23);
            brightnessVoid.TabIndex = 7;
            // 
            // brightnessTabletki
            // 
            brightnessTabletki.Location = new Point(15, 38);
            brightnessTabletki.Name = "brightnessTabletki";
            brightnessTabletki.Size = new Size(151, 23);
            brightnessTabletki.TabIndex = 6;
            // 
            // chart1
            // 
            chart1.BackColor = Color.Transparent;
            chart1.BorderlineColor = SystemColors.InactiveBorder;
            chartArea3.Name = "ChartArea1";
            chart1.ChartAreas.Add(chartArea3);
            legend3.Name = "Legend1";
            chart1.Legends.Add(legend3);
            chart1.Location = new Point(835, 10);
            chart1.Name = "chart1";
            series3.ChartArea = "ChartArea1";
            series3.Legend = "Legend1";
            series3.Name = "Series1";
            chart1.Series.Add(series3);
            chart1.Size = new Size(428, 188);
            chart1.TabIndex = 11;
            chart1.Text = "chart1";
            // 
            // groupBox4
            // 
            groupBox4.BackColor = SystemColors.ButtonHighlight;
            groupBox4.Controls.Add(label9);
            groupBox4.Controls.Add(maxEmptyDistanceTextBox);
            groupBox4.Controls.Add(label7);
            groupBox4.Controls.Add(label8);
            groupBox4.Controls.Add(textBoxPillWidth);
            groupBox4.Controls.Add(textBoxEmptyCell);
            groupBox4.Controls.Add(label5);
            groupBox4.Controls.Add(label6);
            groupBox4.Controls.Add(textBoxPill);
            groupBox4.Controls.Add(textBoxBackground);
            groupBox4.Location = new Point(372, 441);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(200, 305);
            groupBox4.TabIndex = 11;
            groupBox4.TabStop = false;
            groupBox4.Text = "Параметры поиска";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(15, 255);
            label9.Name = "label9";
            label9.Size = new Size(160, 15);
            label9.TabIndex = 12;
            label9.Text = "Максимальное расстояние:";
            // 
            // maxEmptyDistanceTextBox
            // 
            maxEmptyDistanceTextBox.Location = new Point(15, 273);
            maxEmptyDistanceTextBox.Name = "maxEmptyDistanceTextBox";
            maxEmptyDistanceTextBox.Size = new Size(151, 23);
            maxEmptyDistanceTextBox.TabIndex = 13;
            maxEmptyDistanceTextBox.Text = "60";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(15, 195);
            label7.Name = "label7";
            label7.Size = new Size(107, 15);
            label7.TabIndex = 9;
            label7.Text = "Ширина таблетки:";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(15, 137);
            label8.Name = "label8";
            label8.Size = new Size(119, 15);
            label8.TabIndex = 8;
            label8.Text = "Цвет пустой ячейки:";
            // 
            // textBoxPillWidth
            // 
            textBoxPillWidth.Location = new Point(15, 213);
            textBoxPillWidth.Name = "textBoxPillWidth";
            textBoxPillWidth.Size = new Size(151, 23);
            textBoxPillWidth.TabIndex = 11;
            textBoxPillWidth.Text = "60";
            // 
            // textBoxEmptyCell
            // 
            textBoxEmptyCell.Location = new Point(15, 155);
            textBoxEmptyCell.Name = "textBoxEmptyCell";
            textBoxEmptyCell.Size = new Size(151, 23);
            textBoxEmptyCell.TabIndex = 10;
            textBoxEmptyCell.Text = "237";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(15, 78);
            label5.Name = "label5";
            label5.Size = new Size(88, 15);
            label5.TabIndex = 5;
            label5.Text = "Цвет таблетки:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(15, 20);
            label6.Name = "label6";
            label6.Size = new Size(68, 15);
            label6.TabIndex = 4;
            label6.Text = "Цвет фона:";
            // 
            // textBoxPill
            // 
            textBoxPill.Location = new Point(15, 96);
            textBoxPill.Name = "textBoxPill";
            textBoxPill.Size = new Size(151, 23);
            textBoxPill.TabIndex = 7;
            textBoxPill.Text = "255";
            // 
            // textBoxBackground
            // 
            textBoxBackground.Location = new Point(15, 38);
            textBoxBackground.Name = "textBoxBackground";
            textBoxBackground.Size = new Size(151, 23);
            textBoxBackground.TabIndex = 6;
            textBoxBackground.Text = "246";
            // 
            // groupBox5
            // 
            groupBox5.BackColor = SystemColors.ButtonHighlight;
            groupBox5.Controls.Add(endStream);
            groupBox5.Controls.Add(getImageButton);
            groupBox5.Controls.Add(loadImageButton);
            groupBox5.Controls.Add(button3);
            groupBox5.Location = new Point(24, 437);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(168, 366);
            groupBox5.TabIndex = 10;
            groupBox5.TabStop = false;
            groupBox5.Text = "Загрузка/Очистка изображения";
            // 
            // endStream
            // 
            endStream.BackColor = Color.RosyBrown;
            endStream.Location = new Point(19, 306);
            endStream.Name = "endStream";
            endStream.Size = new Size(132, 45);
            endStream.TabIndex = 5;
            endStream.Text = "Закончить стрим";
            endStream.UseVisualStyleBackColor = false;
            endStream.Click += endStream_Click;
            // 
            // getImageButton
            // 
            getImageButton.BackColor = Color.Yellow;
            getImageButton.Location = new Point(19, 255);
            getImageButton.Name = "getImageButton";
            getImageButton.Size = new Size(132, 45);
            getImageButton.TabIndex = 4;
            getImageButton.Text = "Получить стрим";
            getImageButton.UseVisualStyleBackColor = false;
            getImageButton.Click += getImageButton_Click;
            // 
            // button3
            // 
            button3.BackColor = Color.Red;
            button3.Location = new Point(19, 146);
            button3.Name = "button3";
            button3.Size = new Size(132, 93);
            button3.TabIndex = 3;
            button3.Text = "Очистить";
            button3.UseVisualStyleBackColor = false;
            button3.Click += button3_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(835, 437);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(201, 366);
            dataGridView1.TabIndex = 12;
            // 
            // chart2
            // 
            chart2.BackColor = Color.Transparent;
            chart2.BorderlineColor = SystemColors.InactiveBorder;
            chartArea4.Name = "ChartArea1";
            chart2.ChartAreas.Add(chartArea4);
            legend4.Name = "Legend1";
            chart2.Legends.Add(legend4);
            chart2.Location = new Point(835, 215);
            chart2.Name = "chart2";
            series4.ChartArea = "ChartArea1";
            series4.Legend = "Legend1";
            series4.Name = "Series1";
            chart2.Series.Add(series4);
            chart2.Size = new Size(428, 193);
            chart2.TabIndex = 13;
            chart2.Text = "chart2";
            // 
            // dataGridView2
            // 
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.Location = new Point(1052, 437);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.Size = new Size(201, 366);
            dataGridView2.TabIndex = 14;
            // 
            // cameraPictureBox
            // 
            cameraPictureBox.BackColor = SystemColors.ButtonHighlight;
            cameraPictureBox.BorderStyle = BorderStyle.Fixed3D;
            cameraPictureBox.Location = new Point(1286, 12);
            cameraPictureBox.Name = "cameraPictureBox";
            cameraPictureBox.Size = new Size(291, 400);
            cameraPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            cameraPictureBox.TabIndex = 15;
            cameraPictureBox.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1631, 815);
            Controls.Add(cameraPictureBox);
            Controls.Add(dataGridView2);
            Controls.Add(chart2);
            Controls.Add(dataGridView1);
            Controls.Add(groupBox5);
            Controls.Add(groupBox4);
            Controls.Add(chart1);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(recognizePictureBox);
            Controls.Add(originPictureBox);
            Name = "Form1";
            Text = "Алгоритм Хафа";
            ((System.ComponentModel.ISupportInitialize)originPictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)recognizePictureBox).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)chart1).EndInit();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            groupBox5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)chart2).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            ((System.ComponentModel.ISupportInitialize)cameraPictureBox).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox originPictureBox;
        private PictureBox recognizePictureBox;
        private Button loadImageButton;
        private Button recognizeButton;
        private Label label1;
        private Label label2;
        private TextBox tabletkiTextBox;
        private TextBox voidTextBox;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private Label label3;
        private Label label4;
        private TextBox brightnessVoid;
        private TextBox brightnessTabletki;
        private Button button1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private GroupBox groupBox4;
        private Label label5;
        private Label label6;
        private TextBox textBoxPill;
        private TextBox textBoxBackground;
        private Button button2;
        private ToolTip toolTip1;
        private Label label7;
        private Label label8;
        private TextBox textBoxPillWidth;
        private TextBox textBoxEmptyCell;
        private Label label9;
        private TextBox maxEmptyDistanceTextBox;
        private GroupBox groupBox5;
        private Button button3;
        private DataGridView dataGridView1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart2;
        private DataGridView dataGridView2;
        private PictureBox cameraPictureBox;
        private Button getImageButton;
        private Button endStream;
    }
}
