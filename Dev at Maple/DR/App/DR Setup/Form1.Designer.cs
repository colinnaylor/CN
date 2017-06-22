namespace DR_Setup {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.SetupButton = new System.Windows.Forms.Button();
            this.CloseButton = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SqlServerName = new System.Windows.Forms.TextBox();
            this.WorkstationButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.ViewScriptCombo = new System.Windows.Forms.ComboBox();
            this.ViewScriptButton = new System.Windows.Forms.Button();
            this.ServerTestButton = new System.Windows.Forms.Button();
            this.StepByStepCheckbox = new System.Windows.Forms.CheckBox();
            this.FileServerTextbox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // SetupButton
            // 
            this.SetupButton.Location = new System.Drawing.Point(134, 12);
            this.SetupButton.Name = "SetupButton";
            this.SetupButton.Size = new System.Drawing.Size(117, 37);
            this.SetupButton.TabIndex = 0;
            this.SetupButton.Text = "Server Setup Script";
            this.SetupButton.UseVisualStyleBackColor = true;
            this.SetupButton.Click += new System.EventHandler(this.SetupButton_Click);
            // 
            // CloseButton
            // 
            this.CloseButton.Location = new System.Drawing.Point(257, 12);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(117, 37);
            this.CloseButton.TabIndex = 1;
            this.CloseButton.Text = "Close DR Script";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox1.Location = new System.Drawing.Point(12, 55);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(972, 575);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = "";
            this.richTextBox1.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(565, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "SQL Server";
            // 
            // SqlServerName
            // 
            this.SqlServerName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SqlServerName.Location = new System.Drawing.Point(633, 7);
            this.SqlServerName.Name = "SqlServerName";
            this.SqlServerName.Size = new System.Drawing.Size(100, 20);
            this.SqlServerName.TabIndex = 4;
            this.SqlServerName.Text = "DRsql2";
            this.SqlServerName.TextChanged += new System.EventHandler(this.SqlServerName_TextChanged);
            // 
            // WorkstationButton
            // 
            this.WorkstationButton.Location = new System.Drawing.Point(12, 12);
            this.WorkstationButton.Name = "WorkstationButton";
            this.WorkstationButton.Size = new System.Drawing.Size(117, 37);
            this.WorkstationButton.TabIndex = 5;
            this.WorkstationButton.Text = "Workstation Setup Script";
            this.WorkstationButton.UseVisualStyleBackColor = true;
            this.WorkstationButton.Click += new System.EventHandler(this.WorkstationButton_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(816, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "View Script";
            // 
            // ViewScriptCombo
            // 
            this.ViewScriptCombo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ViewScriptCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ViewScriptCombo.FormattingEnabled = true;
            this.ViewScriptCombo.Location = new System.Drawing.Point(819, 29);
            this.ViewScriptCombo.Name = "ViewScriptCombo";
            this.ViewScriptCombo.Size = new System.Drawing.Size(114, 21);
            this.ViewScriptCombo.TabIndex = 7;
            // 
            // ViewScriptButton
            // 
            this.ViewScriptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ViewScriptButton.Location = new System.Drawing.Point(939, 29);
            this.ViewScriptButton.Name = "ViewScriptButton";
            this.ViewScriptButton.Size = new System.Drawing.Size(45, 21);
            this.ViewScriptButton.TabIndex = 8;
            this.ViewScriptButton.Text = "&View";
            this.ViewScriptButton.UseVisualStyleBackColor = true;
            this.ViewScriptButton.Click += new System.EventHandler(this.ViewScriptButton_Click);
            // 
            // ServerTestButton
            // 
            this.ServerTestButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ServerTestButton.Location = new System.Drawing.Point(727, 6);
            this.ServerTestButton.Name = "ServerTestButton";
            this.ServerTestButton.Size = new System.Drawing.Size(45, 21);
            this.ServerTestButton.TabIndex = 9;
            this.ServerTestButton.Text = "Test";
            this.ServerTestButton.UseVisualStyleBackColor = true;
            this.ServerTestButton.Click += new System.EventHandler(this.ServerTestButton_Click);
            // 
            // StepByStepCheckbox
            // 
            this.StepByStepCheckbox.AutoSize = true;
            this.StepByStepCheckbox.Location = new System.Drawing.Point(12, 636);
            this.StepByStepCheckbox.Name = "StepByStepCheckbox";
            this.StepByStepCheckbox.Size = new System.Drawing.Size(114, 17);
            this.StepByStepCheckbox.TabIndex = 10;
            this.StepByStepCheckbox.Text = "Step by step mode";
            this.StepByStepCheckbox.UseVisualStyleBackColor = true;
            // 
            // FileServerTextbox
            // 
            this.FileServerTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FileServerTextbox.Location = new System.Drawing.Point(633, 30);
            this.FileServerTextbox.Name = "FileServerTextbox";
            this.FileServerTextbox.Size = new System.Drawing.Size(100, 20);
            this.FileServerTextbox.TabIndex = 12;
            this.FileServerTextbox.Text = "Yak";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(565, 33);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "File Server";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(996, 665);
            this.Controls.Add(this.FileServerTextbox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.StepByStepCheckbox);
            this.Controls.Add(this.ServerTestButton);
            this.Controls.Add(this.ViewScriptButton);
            this.Controls.Add(this.ViewScriptCombo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.WorkstationButton);
            this.Controls.Add(this.SqlServerName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.SetupButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "DR Setup";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button SetupButton;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox SqlServerName;
        private System.Windows.Forms.Button WorkstationButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox ViewScriptCombo;
        private System.Windows.Forms.Button ViewScriptButton;
        private System.Windows.Forms.Button ServerTestButton;
        private System.Windows.Forms.CheckBox StepByStepCheckbox;
        private System.Windows.Forms.TextBox FileServerTextbox;
        private System.Windows.Forms.Label label3;
    }
}

