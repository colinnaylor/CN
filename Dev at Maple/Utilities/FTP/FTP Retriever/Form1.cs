using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Maple;
using FTP_Retriever.Properties;

namespace FTP_Retriever
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Timer formTimer = new Timer();
        FtpRetrieval retriever = null;

        private void Form1_Load(object sender, EventArgs e)
        {
            string ver = Application.ProductVersion.Substring(0, Application.ProductVersion.LastIndexOf("."));
            Maple.Logon logon = new Logon(Application.ProductName, ver, false);
            logon.SetSplash("Loading...");
            logon.Wait(2000);
            this.Text = Application.ProductName + " v" + ver;

            formTimer.Interval = 1000;
            formTimer.Tick += new EventHandler(formTimer_Tick);
            formTimer.Enabled = true;

            ShowlogCheckbox.Checked = Settings.Default.ShowLog;
            if (!ShowlogCheckbox.Checked)
            {
                richTextBox1.Text = "Log display is turned off.";
            }

            logon.Close();
        }

        void formTimer_Tick(object sender, EventArgs e)
        {
            if (retriever == null)
            {
                retriever = new FtpRetrieval();
                retriever.MessageEvent += new MessageEventDelegate(retriever_MessageEvent);
                retriever.StatusEvent += new MessageEventDelegate(retriever_StatusEvent);
                retriever.Start();
            }
            TimeLabel.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        delegate void DoSetStatus(string Status);
        delegate void DoReport(string Status);
        DoSetStatus updateStatusInvoker = null;
        DoReport reportInvoker = null;

        void retriever_StatusEvent(object sender, MessageEventArgs e)
        {
            if (updateStatusInvoker == null)
            {
                updateStatusInvoker = new DoSetStatus(SetStatus);
            }

            this.Invoke(updateStatusInvoker, e.Message);
        }

        void retriever_MessageEvent(object sender, MessageEventArgs e)
        {
            if (reportInvoker == null)
            {
                reportInvoker = new DoReport(Report);
            }

            this.Invoke(reportInvoker, e.Message);
        }

        internal void SetStatus(string data)
        {
            StatusLabel.Text = data;
            StatusLabel.Refresh();
        }

        internal void Report(string data)
        {
            Maple.Logger.Log(data);
            Console.Out.WriteLine(data);
            if (ShowlogCheckbox.Checked)
            {
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.SelectedText = DateTime.Now.ToString("HH:mm:ss") + "  " + data + "\r\n";

                // trim log
                if (richTextBox1.Lines.Length > 1000)
                {
                    List<string> newLines = richTextBox1.Lines.ToList<string>();
                    newLines.RemoveRange(0, 250);
                    richTextBox1.Lines = newLines.ToArray();
                }

                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
                richTextBox1.Refresh();
            }
        }

    }
}
