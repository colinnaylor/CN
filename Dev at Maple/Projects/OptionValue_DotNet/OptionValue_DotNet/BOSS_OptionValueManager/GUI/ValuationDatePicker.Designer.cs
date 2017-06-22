namespace BOSS_OptionValueManager.GUI
{
    partial class ValuationDatePicker
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
            this.ValuationDateCalendar = new System.Windows.Forms.MonthCalendar();
            this.SuspendLayout();
            // 
            // ValuationDateCalendar
            // 
            this.ValuationDateCalendar.Location = new System.Drawing.Point(-1, -1);
            this.ValuationDateCalendar.MaxSelectionCount = 1;
            this.ValuationDateCalendar.Name = "ValuationDateCalendar";
            this.ValuationDateCalendar.TabIndex = 0;
            this.ValuationDateCalendar.DateSelected += new System.Windows.Forms.DateRangeEventHandler(this.ValuationDateCalendar_DateSelected);
            // 
            // ValuationDatePicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(172, 156);
            this.Controls.Add(this.ValuationDateCalendar);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ValuationDatePicker";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MonthCalendar ValuationDateCalendar;

    }
}