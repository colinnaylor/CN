namespace Microsoft.Samples.SqlServer
{
    partial class CNDBScript
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CNDBScript));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.ConnectCommandButton = new System.Windows.Forms.Button();
            this.ServerNamesComboBox = new System.Windows.Forms.ComboBox();
            this.ScriptDBBtn = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.localScriptPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.VSSProjTxt = new System.Windows.Forms.TextBox();
            this.VSSPwdTxt = new System.Windows.Forms.TextBox();
            this.VSSLoginTxt = new System.Windows.Forms.TextBox();
            this.VSSIniPath = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.GetServer = new System.Windows.Forms.Button();
            this.ServerNameLabel = new System.Windows.Forms.Label();
            this.DBListView = new System.Windows.Forms.ListView();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ScriptGroupBox = new System.Windows.Forms.GroupBox();
            this.SelectedCountLabel = new System.Windows.Forms.Label();
            this.ScriptOnlyCheckbox = new System.Windows.Forms.CheckBox();
            this.Output = new System.Windows.Forms.RichTextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.ScriptGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2,
            this.toolStripStatusLabel3});
            this.statusStrip1.Location = new System.Drawing.Point(0, 556);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip1.Size = new System.Drawing.Size(767, 22);
            this.statusStrip1.TabIndex = 31;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(118, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(118, 17);
            this.toolStripStatusLabel2.Text = "toolStripStatusLabel2";
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(516, 17);
            this.toolStripStatusLabel3.Spring = true;
            this.toolStripStatusLabel3.Text = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ConnectCommandButton
            // 
            this.ConnectCommandButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ConnectCommandButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConnectCommandButton.Location = new System.Drawing.Point(208, 70);
            this.ConnectCommandButton.Name = "ConnectCommandButton";
            this.ConnectCommandButton.Size = new System.Drawing.Size(99, 23);
            this.ConnectCommandButton.TabIndex = 28;
            this.ConnectCommandButton.Text = "Connect";
            this.ConnectCommandButton.Click += new System.EventHandler(this.ConnectCommandButton_Click);
            // 
            // ServerNamesComboBox
            // 
            this.ServerNamesComboBox.FormattingEnabled = true;
            this.ServerNamesComboBox.Location = new System.Drawing.Point(95, 16);
            this.ServerNamesComboBox.Margin = new System.Windows.Forms.Padding(0, 3, 3, 1);
            this.ServerNamesComboBox.Name = "ServerNamesComboBox";
            this.ServerNamesComboBox.Size = new System.Drawing.Size(213, 21);
            this.ServerNamesComboBox.Sorted = true;
            this.ServerNamesComboBox.TabIndex = 16;
            this.ServerNamesComboBox.Text = "(Local)";
            // 
            // ScriptDBBtn
            // 
            this.ScriptDBBtn.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ScriptDBBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ScriptDBBtn.Location = new System.Drawing.Point(278, 344);
            this.ScriptDBBtn.Name = "ScriptDBBtn";
            this.ScriptDBBtn.Size = new System.Drawing.Size(121, 24);
            this.ScriptDBBtn.TabIndex = 30;
            this.ScriptDBBtn.Text = "Begin";
            this.ScriptDBBtn.UseVisualStyleBackColor = false;
            this.ScriptDBBtn.Click += new System.EventHandler(this.ScriptDBBtn_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox1.Controls.Add(this.localScriptPath);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.VSSProjTxt);
            this.groupBox1.Controls.Add(this.VSSPwdTxt);
            this.groupBox1.Controls.Add(this.VSSLoginTxt);
            this.groupBox1.Controls.Add(this.VSSIniPath);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Location = new System.Drawing.Point(6, 201);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(400, 138);
            this.groupBox1.TabIndex = 27;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Visual Source Safe";
            // 
            // localScriptPath
            // 
            this.localScriptPath.Location = new System.Drawing.Point(85, 112);
            this.localScriptPath.Name = "localScriptPath";
            this.localScriptPath.Size = new System.Drawing.Size(224, 20);
            this.localScriptPath.TabIndex = 35;
            this.localScriptPath.Text = "c:\\temp";
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.SystemColors.Control;
            this.label1.Location = new System.Drawing.Point(9, 113);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 16);
            this.label1.TabIndex = 34;
            this.label1.Text = "Local Folder";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // VSSProjTxt
            // 
            this.VSSProjTxt.Location = new System.Drawing.Point(85, 85);
            this.VSSProjTxt.Name = "VSSProjTxt";
            this.VSSProjTxt.Size = new System.Drawing.Size(224, 20);
            this.VSSProjTxt.TabIndex = 33;
            this.VSSProjTxt.Text = "$/SQL/";
            // 
            // VSSPwdTxt
            // 
            this.VSSPwdTxt.Location = new System.Drawing.Point(85, 61);
            this.VSSPwdTxt.Name = "VSSPwdTxt";
            this.VSSPwdTxt.PasswordChar = '*';
            this.VSSPwdTxt.Size = new System.Drawing.Size(126, 20);
            this.VSSPwdTxt.TabIndex = 31;
            // 
            // VSSLoginTxt
            // 
            this.VSSLoginTxt.Location = new System.Drawing.Point(85, 37);
            this.VSSLoginTxt.Name = "VSSLoginTxt";
            this.VSSLoginTxt.Size = new System.Drawing.Size(126, 20);
            this.VSSLoginTxt.TabIndex = 29;
            this.VSSLoginTxt.Text = "AppServer";
            // 
            // VSSIniPath
            // 
            this.VSSIniPath.Location = new System.Drawing.Point(85, 15);
            this.VSSIniPath.Name = "VSSIniPath";
            this.VSSIniPath.Size = new System.Drawing.Size(226, 20);
            this.VSSIniPath.TabIndex = 26;
            this.VSSIniPath.Text = "S:\\SCCS_2009\\srcsafe.ini";
            // 
            // label9
            // 
            this.label9.BackColor = System.Drawing.SystemColors.Control;
            this.label9.Location = new System.Drawing.Point(9, 87);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(76, 16);
            this.label9.TabIndex = 32;
            this.label9.Text = "Project path";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label9.Click += new System.EventHandler(this.label9_Click);
            // 
            // label8
            // 
            this.label8.BackColor = System.Drawing.SystemColors.Control;
            this.label8.Location = new System.Drawing.Point(9, 63);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(60, 16);
            this.label8.TabIndex = 30;
            this.label8.Text = "Password";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label8.Click += new System.EventHandler(this.label8_Click);
            // 
            // label7
            // 
            this.label7.BackColor = System.Drawing.SystemColors.Control;
            this.label7.Location = new System.Drawing.Point(9, 39);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(44, 16);
            this.label7.TabIndex = 28;
            this.label7.Text = "Login";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // button1
            // 
            this.button1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button1.Location = new System.Drawing.Point(315, 15);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(78, 22);
            this.button1.TabIndex = 27;
            this.button1.Text = "Browse ...";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label6
            // 
            this.label6.BackColor = System.Drawing.SystemColors.Control;
            this.label6.Location = new System.Drawing.Point(9, 17);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(66, 16);
            this.label6.TabIndex = 25;
            this.label6.Text = "Ini file path";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // GetServer
            // 
            this.GetServer.Location = new System.Drawing.Point(156, 41);
            this.GetServer.Name = "GetServer";
            this.GetServer.Size = new System.Drawing.Size(151, 23);
            this.GetServer.TabIndex = 30;
            this.GetServer.Text = "Get Server List";
            this.GetServer.UseVisualStyleBackColor = true;
            this.GetServer.Click += new System.EventHandler(this.GetServer_Click);
            // 
            // ServerNameLabel
            // 
            this.ServerNameLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ServerNameLabel.Location = new System.Drawing.Point(6, 19);
            this.ServerNameLabel.Margin = new System.Windows.Forms.Padding(3, 3, 1, 1);
            this.ServerNameLabel.Name = "ServerNameLabel";
            this.ServerNameLabel.Size = new System.Drawing.Size(88, 23);
            this.ServerNameLabel.TabIndex = 15;
            this.ServerNameLabel.Text = "&Server name:";
            // 
            // DBListView
            // 
            this.DBListView.CheckBoxes = true;
            this.DBListView.FullRowSelect = true;
            this.DBListView.GridLines = true;
            this.DBListView.Location = new System.Drawing.Point(6, 19);
            this.DBListView.Name = "DBListView";
            this.DBListView.Size = new System.Drawing.Size(400, 176);
            this.DBListView.TabIndex = 1;
            this.DBListView.UseCompatibleStateImageBehavior = false;
            this.DBListView.View = System.Windows.Forms.View.Details;
            this.DBListView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.DBListView_ItemChecked);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.GetServer);
            this.groupBox2.Controls.Add(this.ServerNameLabel);
            this.groupBox2.Controls.Add(this.ServerNamesComboBox);
            this.groupBox2.Controls.Add(this.ConnectCommandButton);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(320, 385);
            this.groupBox2.TabIndex = 33;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Connection";
            // 
            // ScriptGroupBox
            // 
            this.ScriptGroupBox.Controls.Add(this.SelectedCountLabel);
            this.ScriptGroupBox.Controls.Add(this.ScriptOnlyCheckbox);
            this.ScriptGroupBox.Controls.Add(this.DBListView);
            this.ScriptGroupBox.Controls.Add(this.groupBox1);
            this.ScriptGroupBox.Controls.Add(this.ScriptDBBtn);
            this.ScriptGroupBox.Location = new System.Drawing.Point(338, 12);
            this.ScriptGroupBox.Name = "ScriptGroupBox";
            this.ScriptGroupBox.Size = new System.Drawing.Size(415, 385);
            this.ScriptGroupBox.TabIndex = 34;
            this.ScriptGroupBox.TabStop = false;
            this.ScriptGroupBox.Text = "Script and Add to Sourcesafe";
            // 
            // SelectedCountLabel
            // 
            this.SelectedCountLabel.AutoSize = true;
            this.SelectedCountLabel.Location = new System.Drawing.Point(15, 350);
            this.SelectedCountLabel.Name = "SelectedCountLabel";
            this.SelectedCountLabel.Size = new System.Drawing.Size(79, 13);
            this.SelectedCountLabel.TabIndex = 32;
            this.SelectedCountLabel.Text = "DBs selected 0";
            // 
            // ScriptOnlyCheckbox
            // 
            this.ScriptOnlyCheckbox.AutoSize = true;
            this.ScriptOnlyCheckbox.Location = new System.Drawing.Point(195, 349);
            this.ScriptOnlyCheckbox.Name = "ScriptOnlyCheckbox";
            this.ScriptOnlyCheckbox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.ScriptOnlyCheckbox.Size = new System.Drawing.Size(77, 17);
            this.ScriptOnlyCheckbox.TabIndex = 31;
            this.ScriptOnlyCheckbox.Text = "Script Only";
            this.ScriptOnlyCheckbox.UseVisualStyleBackColor = true;
            // 
            // Output
            // 
            this.Output.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Output.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Output.Location = new System.Drawing.Point(12, 403);
            this.Output.Name = "Output";
            this.Output.Size = new System.Drawing.Size(741, 150);
            this.Output.TabIndex = 35;
            this.Output.Text = "";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // CNDBScript
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(767, 578);
            this.Controls.Add(this.Output);
            this.Controls.Add(this.ScriptGroupBox);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.statusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CNDBScript";
            this.Text = "CN Script Database";
            this.Load += new System.EventHandler(this.DBScript_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ScriptGroupBox.ResumeLayout(false);
            this.ScriptGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Button ScriptDBBtn;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox VSSProjTxt;
        private System.Windows.Forms.TextBox VSSPwdTxt;
        private System.Windows.Forms.TextBox VSSLoginTxt;
        private System.Windows.Forms.TextBox VSSIniPath;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button GetServer;
        private System.Windows.Forms.ComboBox ServerNamesComboBox;
        private System.Windows.Forms.Button ConnectCommandButton;
        private System.Windows.Forms.Label ServerNameLabel;
        private System.Windows.Forms.ListView DBListView;
        private System.Windows.Forms.TextBox localScriptPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox ScriptGroupBox;
        private System.Windows.Forms.CheckBox ScriptOnlyCheckbox;
        private System.Windows.Forms.Label SelectedCountLabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.RichTextBox Output;
        private System.Windows.Forms.Timer timer1;
    }
}