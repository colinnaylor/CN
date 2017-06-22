using System.ComponentModel;
using System.Windows.Forms;

namespace BBfieldValueRetriever {
    partial class Form1 {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.StartButton = new System.Windows.Forms.Button();
            this.ActiveLabel = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.LastCheckLabel = new System.Windows.Forms.Label();
            this.StatusSetLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.HitsWarningTextbox = new System.Windows.Forms.TextBox();
            this.HitsLimitTextbox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.HitsTodayTextbox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ActiveImage = new System.Windows.Forms.PictureBox();
            this.InvisibilityLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.ActiveImage)).BeginInit();
            this.SuspendLayout();
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(12, 12);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(75, 23);
            this.StartButton.TabIndex = 0;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // ActiveLabel
            // 
            this.ActiveLabel.AutoSize = true;
            this.ActiveLabel.Location = new System.Drawing.Point(93, 17);
            this.ActiveLabel.Name = "ActiveLabel";
            this.ActiveLabel.Size = new System.Drawing.Size(24, 13);
            this.ActiveLabel.TabIndex = 1;
            this.ActiveLabel.Text = "Idle";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox1.Location = new System.Drawing.Point(12, 71);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(588, 190);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = "";
            // 
            // LastCheckLabel
            // 
            this.LastCheckLabel.AutoSize = true;
            this.LastCheckLabel.Location = new System.Drawing.Point(149, 17);
            this.LastCheckLabel.Name = "LastCheckLabel";
            this.LastCheckLabel.Size = new System.Drawing.Size(34, 13);
            this.LastCheckLabel.TabIndex = 3;
            this.LastCheckLabel.Text = "never";
            // 
            // StatusSetLabel
            // 
            this.StatusSetLabel.AutoSize = true;
            this.StatusSetLabel.Location = new System.Drawing.Point(301, 17);
            this.StatusSetLabel.Name = "StatusSetLabel";
            this.StatusSetLabel.Size = new System.Drawing.Size(34, 13);
            this.StatusSetLabel.TabIndex = 4;
            this.StatusSetLabel.Text = "never";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Hits Warning";
            // 
            // HitsWarningTextbox
            // 
            this.HitsWarningTextbox.Location = new System.Drawing.Point(88, 45);
            this.HitsWarningTextbox.Name = "HitsWarningTextbox";
            this.HitsWarningTextbox.ReadOnly = true;
            this.HitsWarningTextbox.Size = new System.Drawing.Size(81, 20);
            this.HitsWarningTextbox.TabIndex = 6;
            this.HitsWarningTextbox.Text = "0";
            // 
            // HitsLimitTextbox
            // 
            this.HitsLimitTextbox.Location = new System.Drawing.Point(253, 45);
            this.HitsLimitTextbox.Name = "HitsLimitTextbox";
            this.HitsLimitTextbox.ReadOnly = true;
            this.HitsLimitTextbox.Size = new System.Drawing.Size(81, 20);
            this.HitsLimitTextbox.TabIndex = 8;
            this.HitsLimitTextbox.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(196, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Hits Limit";
            // 
            // HitsTodayTextbox
            // 
            this.HitsTodayTextbox.Location = new System.Drawing.Point(425, 45);
            this.HitsTodayTextbox.Name = "HitsTodayTextbox";
            this.HitsTodayTextbox.ReadOnly = true;
            this.HitsTodayTextbox.Size = new System.Drawing.Size(81, 20);
            this.HitsTodayTextbox.TabIndex = 10;
            this.HitsTodayTextbox.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(358, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Hits Today";
            // 
            // ActiveImage
            // 
            this.ActiveImage.Location = new System.Drawing.Point(580, 12);
            this.ActiveImage.Name = "ActiveImage";
            this.ActiveImage.Size = new System.Drawing.Size(20, 20);
            this.ActiveImage.TabIndex = 11;
            this.ActiveImage.TabStop = false;
            // 
            // InvisibilityLabel
            // 
            this.InvisibilityLabel.AutoSize = true;
            this.InvisibilityLabel.Location = new System.Drawing.Point(391, 87);
            this.InvisibilityLabel.Name = "InvisibilityLabel";
            this.InvisibilityLabel.Size = new System.Drawing.Size(104, 13);
            this.InvisibilityLabel.TabIndex = 12;
            this.InvisibilityLabel.Text = "Waiting for invisibility";
            this.InvisibilityLabel.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(612, 273);
            this.Controls.Add(this.InvisibilityLabel);
            this.Controls.Add(this.ActiveImage);
            this.Controls.Add(this.HitsTodayTextbox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.HitsLimitTextbox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.HitsWarningTextbox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.StatusSetLabel);
            this.Controls.Add(this.LastCheckLabel);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.ActiveLabel);
            this.Controls.Add(this.StartButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ActiveImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button StartButton;
        private Label ActiveLabel;
        private RichTextBox richTextBox1;
        private Label LastCheckLabel;
        private Label StatusSetLabel;
        private Label label1;
        private TextBox HitsWarningTextbox;
        private TextBox HitsLimitTextbox;
        private Label label2;
        private TextBox HitsTodayTextbox;
        private Label label3;
        private PictureBox ActiveImage;
        private Label InvisibilityLabel;
    }
}

