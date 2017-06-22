namespace ZeroCouponGenerator {
    partial class Form1 {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.Testbutton = new System.Windows.Forms.Button();
            this.ReportBox = new System.Windows.Forms.RichTextBox();
            this.CurrencyCombo = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // Testbutton
            // 
            this.Testbutton.Location = new System.Drawing.Point(12, 12);
            this.Testbutton.Name = "Testbutton";
            this.Testbutton.Size = new System.Drawing.Size(94, 32);
            this.Testbutton.TabIndex = 0;
            this.Testbutton.Text = "Test";
            this.Testbutton.UseVisualStyleBackColor = true;
            this.Testbutton.Click += new System.EventHandler(this.Testbutton_Click);
            // 
            // ReportBox
            // 
            this.ReportBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ReportBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ReportBox.Location = new System.Drawing.Point(12, 50);
            this.ReportBox.Name = "ReportBox";
            this.ReportBox.Size = new System.Drawing.Size(520, 502);
            this.ReportBox.TabIndex = 1;
            this.ReportBox.Text = "";
            // 
            // CurrencyCombo
            // 
            this.CurrencyCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CurrencyCombo.FormattingEnabled = true;
            this.CurrencyCombo.Location = new System.Drawing.Point(112, 19);
            this.CurrencyCombo.Name = "CurrencyCombo";
            this.CurrencyCombo.Size = new System.Drawing.Size(97, 21);
            this.CurrencyCombo.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(544, 564);
            this.Controls.Add(this.CurrencyCombo);
            this.Controls.Add(this.ReportBox);
            this.Controls.Add(this.Testbutton);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Testbutton;
        private System.Windows.Forms.RichTextBox ReportBox;
        private System.Windows.Forms.ComboBox CurrencyCombo;
    }
}

