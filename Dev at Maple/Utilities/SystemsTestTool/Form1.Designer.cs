namespace SystemsTestTool {
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
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.TestButton = new System.Windows.Forms.Button();
            this.StepByStepCheckbox = new System.Windows.Forms.CheckBox();
            this.ViewScriptButton = new System.Windows.Forms.Button();
            this.LiveTestRadiobutton = new System.Windows.Forms.RadioButton();
            this.DevTestRadiobutton = new System.Windows.Forms.RadioButton();
            this.DrTestRadiobutton = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox1.Location = new System.Drawing.Point(12, 45);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(870, 585);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = "";
            this.richTextBox1.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            // 
            // TestButton
            // 
            this.TestButton.Location = new System.Drawing.Point(12, 12);
            this.TestButton.Name = "TestButton";
            this.TestButton.Size = new System.Drawing.Size(83, 27);
            this.TestButton.TabIndex = 5;
            this.TestButton.Text = "Test";
            this.TestButton.UseVisualStyleBackColor = true;
            this.TestButton.Click += new System.EventHandler(this.TestButton_Click);
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
            // ViewScriptButton
            // 
            this.ViewScriptButton.Location = new System.Drawing.Point(101, 12);
            this.ViewScriptButton.Name = "ViewScriptButton";
            this.ViewScriptButton.Size = new System.Drawing.Size(83, 27);
            this.ViewScriptButton.TabIndex = 11;
            this.ViewScriptButton.Text = "View Script";
            this.ViewScriptButton.UseVisualStyleBackColor = true;
            this.ViewScriptButton.Click += new System.EventHandler(this.ViewScriptButton_Click);
            // 
            // LiveTestRadiobutton
            // 
            this.LiveTestRadiobutton.AutoSize = true;
            this.LiveTestRadiobutton.Checked = true;
            this.LiveTestRadiobutton.Location = new System.Drawing.Point(243, 17);
            this.LiveTestRadiobutton.Name = "LiveTestRadiobutton";
            this.LiveTestRadiobutton.Size = new System.Drawing.Size(69, 17);
            this.LiveTestRadiobutton.TabIndex = 14;
            this.LiveTestRadiobutton.TabStop = true;
            this.LiveTestRadiobutton.Text = "Live Test";
            this.LiveTestRadiobutton.UseVisualStyleBackColor = true;
            // 
            // DevTestRadiobutton
            // 
            this.DevTestRadiobutton.AutoSize = true;
            this.DevTestRadiobutton.Location = new System.Drawing.Point(334, 17);
            this.DevTestRadiobutton.Name = "DevTestRadiobutton";
            this.DevTestRadiobutton.Size = new System.Drawing.Size(69, 17);
            this.DevTestRadiobutton.TabIndex = 15;
            this.DevTestRadiobutton.Text = "Dev Test";
            this.DevTestRadiobutton.UseVisualStyleBackColor = true;
            // 
            // DrTestRadiobutton
            // 
            this.DrTestRadiobutton.AutoSize = true;
            this.DrTestRadiobutton.Location = new System.Drawing.Point(425, 17);
            this.DrTestRadiobutton.Name = "DrTestRadiobutton";
            this.DrTestRadiobutton.Size = new System.Drawing.Size(65, 17);
            this.DrTestRadiobutton.TabIndex = 16;
            this.DrTestRadiobutton.Text = "DR Test";
            this.DrTestRadiobutton.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(894, 665);
            this.Controls.Add(this.DrTestRadiobutton);
            this.Controls.Add(this.DevTestRadiobutton);
            this.Controls.Add(this.LiveTestRadiobutton);
            this.Controls.Add(this.ViewScriptButton);
            this.Controls.Add(this.StepByStepCheckbox);
            this.Controls.Add(this.TestButton);
            this.Controls.Add(this.richTextBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Systems Test Tool";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button TestButton;
        private System.Windows.Forms.CheckBox StepByStepCheckbox;
        private System.Windows.Forms.Button ViewScriptButton;
        private System.Windows.Forms.RadioButton LiveTestRadiobutton;
        private System.Windows.Forms.RadioButton DevTestRadiobutton;
        private System.Windows.Forms.RadioButton DrTestRadiobutton;
    }
}

