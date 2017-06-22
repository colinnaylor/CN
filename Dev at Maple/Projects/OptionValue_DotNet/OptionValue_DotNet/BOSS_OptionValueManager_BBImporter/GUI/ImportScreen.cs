using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OTCOptionValuation_BBImporter.GUI
{
    public partial class ImportScreen : Form
    {
        public ImportScreen()
        {
            InitializeComponent();
        }

        private void btnGetData_Click(object sender, EventArgs e)
        {
            ImportManager im = new ImportManager();
            im.StatusChanged += new StatusUpdateDel(im_StatusChanged);
            this.Cursor = Cursors.WaitCursor;
            try {
                im.RunSources(PriceDatePicker.Value.Date,
                        chkRunVol.Checked,
                        chkRunRates.Checked,
                        chkRunDividends.Checked,
                        chRunMissingVolsOnly.Checked);

            } finally {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// display status (marshall back on to UI thread)
        /// </summary>
        /// <param name="status"></param>
        void im_StatusChanged(string status)
        {
            current_status = status;
            Invoke(new MethodInvoker(ShowCurrentStatus));
        }
        private string current_status;
        private Color current_colour = Color.Black;

        void ShowCurrentStatus()
        {
            RichTextBox rtb = richTextBox1;
            if (rtb.Text.Length > 16000) {
                rtb.Text = rtb.Text.Substring(rtb.Text.Length - 8000);
            }
            rtb.SelectionStart = rtb.Text.Length;
            rtb.SelectionColor = current_colour;
            rtb.SelectedText = DateTime.Now.ToString("yyMMdd HH:mm:ss") + "  " + current_status + "\r\n";
            rtb.SelectionStart = rtb.Text.Length;
            rtb.ScrollToCaret();
        }

        private void ImportScreen_Load(object sender, EventArgs e) {
            Data.DataLayer dl = new Data.DataLayer();
            ConnectionInfoLabel.Text = dl.GetConnectionInfo();
#if DEBUG
            ConnectionInfoLabel.Text += "\r\nDEBUG";
#endif
        }
    }
}
