namespace OTCOptionValuation_BBImporter.GUI
{
    partial class ImportScreen
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportScreen));
            this.PriceDatePicker = new System.Windows.Forms.DateTimePicker();
            this.btnGetData = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.chkRunVol = new System.Windows.Forms.CheckBox();
            this.chkRunRates = new System.Windows.Forms.CheckBox();
            this.chkRunDividends = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chRunMissingVolsOnly = new System.Windows.Forms.CheckBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.ConnectionInfoLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // PriceDatePicker
            // 
            this.PriceDatePicker.Location = new System.Drawing.Point(145, 18);
            this.PriceDatePicker.Name = "PriceDatePicker";
            this.PriceDatePicker.Size = new System.Drawing.Size(165, 20);
            this.PriceDatePicker.TabIndex = 1;
            // 
            // btnGetData
            // 
            this.btnGetData.Image = ((System.Drawing.Image)(resources.GetObject("btnGetData.Image")));
            this.btnGetData.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnGetData.Location = new System.Drawing.Point(10, 128);
            this.btnGetData.Name = "btnGetData";
            this.btnGetData.Size = new System.Drawing.Size(300, 41);
            this.btnGetData.TabIndex = 2;
            this.btnGetData.Text = "Import from Bloomberg";
            this.btnGetData.UseVisualStyleBackColor = true;
            this.btnGetData.Click += new System.EventHandler(this.btnGetData_Click);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial Black", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Image = ((System.Drawing.Image)(resources.GetObject("label1.Image")));
            this.label1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label1.Location = new System.Drawing.Point(7, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(130, 39);
            this.label1.TabIndex = 3;
            this.label1.Text = "Valuation Date";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkRunVol
            // 
            this.chkRunVol.AutoSize = true;
            this.chkRunVol.Checked = true;
            this.chkRunVol.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRunVol.Location = new System.Drawing.Point(14, 55);
            this.chkRunVol.Name = "chkRunVol";
            this.chkRunVol.Size = new System.Drawing.Size(74, 18);
            this.chkRunVol.TabIndex = 5;
            this.chkRunVol.Text = "Volatilities";
            this.chkRunVol.UseVisualStyleBackColor = true;
            // 
            // chkRunRates
            // 
            this.chkRunRates.AutoSize = true;
            this.chkRunRates.Checked = true;
            this.chkRunRates.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRunRates.Location = new System.Drawing.Point(14, 80);
            this.chkRunRates.Name = "chkRunRates";
            this.chkRunRates.Size = new System.Drawing.Size(54, 18);
            this.chkRunRates.TabIndex = 6;
            this.chkRunRates.Text = "Rates";
            this.chkRunRates.UseVisualStyleBackColor = true;
            // 
            // chkRunDividends
            // 
            this.chkRunDividends.AutoSize = true;
            this.chkRunDividends.Checked = true;
            this.chkRunDividends.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRunDividends.Location = new System.Drawing.Point(14, 104);
            this.chkRunDividends.Name = "chkRunDividends";
            this.chkRunDividends.Size = new System.Drawing.Size(73, 18);
            this.chkRunDividends.TabIndex = 7;
            this.chkRunDividends.Text = "Dividends";
            this.chkRunDividends.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 172);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 14);
            this.label2.TabIndex = 8;
            this.label2.Text = "Import Status...";
            // 
            // chRunMissingVolsOnly
            // 
            this.chRunMissingVolsOnly.AutoSize = true;
            this.chRunMissingVolsOnly.Location = new System.Drawing.Point(94, 55);
            this.chRunMissingVolsOnly.Name = "chRunMissingVolsOnly";
            this.chRunMissingVolsOnly.Size = new System.Drawing.Size(138, 18);
            this.chRunMissingVolsOnly.TabIndex = 9;
            this.chRunMissingVolsOnly.Text = "Missing Volatilities Only";
            this.chRunMissingVolsOnly.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.Location = new System.Drawing.Point(10, 189);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(559, 182);
            this.richTextBox1.TabIndex = 10;
            this.richTextBox1.Text = "";
            // 
            // ConnectionInfoLabel
            // 
            this.ConnectionInfoLabel.AutoSize = true;
            this.ConnectionInfoLabel.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ConnectionInfoLabel.Location = new System.Drawing.Point(328, 21);
            this.ConnectionInfoLabel.Name = "ConnectionInfoLabel";
            this.ConnectionInfoLabel.Size = new System.Drawing.Size(47, 15);
            this.ConnectionInfoLabel.TabIndex = 11;
            this.ConnectionInfoLabel.Text = "Not Set";
            // 
            // ImportScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(581, 383);
            this.Controls.Add(this.ConnectionInfoLabel);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.chRunMissingVolsOnly);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.chkRunDividends);
            this.Controls.Add(this.chkRunRates);
            this.Controls.Add(this.chkRunVol);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnGetData);
            this.Controls.Add(this.PriceDatePicker);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "ImportScreen";
            this.Text = "OTC Option Rate/Vol/Div Import";
            this.Load += new System.EventHandler(this.ImportScreen_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DateTimePicker PriceDatePicker;
        private System.Windows.Forms.Button btnGetData;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkRunVol;
        private System.Windows.Forms.CheckBox chkRunRates;
        private System.Windows.Forms.CheckBox chkRunDividends;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chRunMissingVolsOnly;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Label ConnectionInfoLabel;
    }
}