using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BBfieldValueRetriever.Control;
using BBfieldValueRetriever.Properties;
using Maple;
namespace BBfieldValueRetriever
{
    public partial class Form1 : Form
    {
        private Timer _visTimer;

        /// <summary>
        /// Marks this instance to read from the development server
        /// </summary>

        public Form1(string[] args)
        {
            InitializeComponent();

            if (args.Length > 0)
            {
                if (args[0].ToLower() == "invisible")
                {
                    InvisibilityLabel.Visible = true;
                    _visTimer = new Timer();
                    _visTimer.Interval = 15000;
                    _visTimer.Tick += visTimer_Tick;
                    _visTimer.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Used to make the app invisible when running on a user machine.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void visTimer_Tick(object sender, EventArgs e)
        {
            _visTimer.Enabled = false;
            Visible = false;
        }

        private BergController _bbgController;

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                string ver = Application.ProductVersion.Substring(0, Application.ProductVersion.LastIndexOf(".", StringComparison.Ordinal));
                Text = Application.ProductName + " v" + ver;

                Left = 100;
                Text = string.Format("{0} ({1} Version)", Text, Settings.Default.EnvName);
                NLogger.Instance.Info(" {2} Version {0} started with ID {1}.", ver, Process.GetCurrentProcess().Id, Settings.Default.EnvName);

                NLogger.Instance.Info("Database name {0} mapped to {1} using Maple Database Settings", Settings.Default.DSN, new Maple.SQLServer(Settings.Default.DSN).ConnectionString);
                NLogger.Instance.Info("Maple Database Mappings: {0} , {1}", Maple.SharedDSN.GetDsnFileNameLocal(), Maple.SharedDSN.GetDsnFileNameShared());


                StartButton_Click(null, null);
            }
            catch (Exception ex)
            {
                NLogger.Instance.Error(ex.ToString());
                Application.Exit();
            }
        }

        private void bbg_MessageEvent(object sender, MessageEventArgs e)
        {
            if (HitsTodayTextbox.InvokeRequired)
            {
                HitsTodayTextbox.Invoke((Action<object, MessageEventArgs>)bbg_MessageEvent, sender, e);
            }
            else
            {
                if (!e.Message.Trim().StartsWith("checking at", StringComparison.OrdinalIgnoreCase)
                    && !e.Message.Trim().StartsWith("next status", StringComparison.OrdinalIgnoreCase))
                    NLogger.Instance.Info(e.Message);
                switch (e.Message.ToLower())
                {
                    case "started":
                        ActiveLabel.Text = "Active";
                        StartButton.Text = "Stop";
                        ActiveLabel.ForeColor = Color.Green;
                        break;

                    case "stopped":
                        ActiveLabel.Text = "Idle";
                        StartButton.Text = "Start";
                        ActiveLabel.ForeColor = Color.Black;
                        break;

                    default:
                        if (e.Message.Length > 10 && e.Message.Substring(0, 11).ToLower() == "checking at")
                        {
                            SetLastAction("Last checked " + e.Message.Substring(9));
                        }
                        else if (e.Message.Length > 9 && e.Message.Substring(0, 10).ToLower() == "processing")
                        {
                            Report(e.Message, Color.Black);
                            ActiveImage.BackColor = Color.Green;
                        }
                        else if (e.Message.Length > 8 && e.Message.Substring(0, 9).ToLower() == "completed")
                        {
                            ActiveImage.BackColor = BackColor;
                            Report(e.Message, Color.Black);
                        }
                        else if (e.Message.Length > 10 && e.Message.Substring(0, 11).ToLower() == "next status")
                        {
                            SetStatusLabel(e.Message);
                        }
                        else if (e.Message.Length > 11 && e.Message.Substring(0, 4).ToLower() == "hits")
                        {
                            switch (e.Message.Substring(0, 12).ToLower())
                            {
                                case "hitswarning ":
                                    HitsWarningTextbox.Text = e.Message.Substring(12);
                                    break;

                                case "hitslimit   ":
                                    HitsLimitTextbox.Text = e.Message.Substring(12);
                                    break;

                                case "hitstotal   ":
                                    HitsTodayTextbox.Text = e.Message.Substring(12);
                                    break;
                            }
                        }
                        else
                        {
                            Report(e.Message, Color.Black);
                        }
                        break;
                }
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (StartButton.Text == "Start")
            {
                _bbgController = new BergController();
                _bbgController.ApiController.MessageEvent += bbg_MessageEvent;
                _bbgController.MessageEvent += bbg_MessageEvent;
                _bbgController.Start();
            }
            else
            {
                _bbgController.Stop();
                _bbgController.ApiController.MessageEvent -= bbg_MessageEvent;
                _bbgController.MessageEvent -= bbg_MessageEvent;
                _bbgController = null;
            }
        }

        private delegate void ReportDelegate(string data, Color colour);

        private ReportDelegate _reportDelegate;

        private delegate void SetLastActionDelegate(string data);

        private SetLastActionDelegate _setLastAction;
        private SetLastActionDelegate _setStatusTime;

        private void Report(string data, Color colour)
        {
            if (_reportDelegate == null)
            {
                _reportDelegate = Report;
            }

            if (richTextBox1.InvokeRequired)
            {
                Invoke(_reportDelegate, data, colour);
            }
            else
            {
                RichTextBox rtb = richTextBox1;
                if (rtb.Text.Length > 4000)
                {
                    rtb.Text = rtb.Text.Substring(rtb.Text.Length - 4000);
                }
                rtb.SelectionStart = rtb.Text.Length;
                rtb.SelectionColor = colour;
                rtb.SelectedText = DateTime.Now.ToString("yyMMdd HH:mm:ss") + "  " + data + "\r\n";
                rtb.SelectionStart = rtb.Text.Length;
                rtb.ScrollToCaret();
            }
        }

        private void SetStatusLabel(string data)
        {
            if (_setStatusTime == null)
            {
                _setStatusTime = SetStatusLabel;
            }

            if (StatusSetLabel.InvokeRequired)
            {
                Invoke(_setStatusTime, data);
            }
            else
            {
                StatusSetLabel.Text = data;
            }
        }

        private void SetLastAction(string data)
        {
            if (_setLastAction == null)
            {
                _setLastAction = SetLastAction;
            }

            if (LastCheckLabel.InvokeRequired)
            {
                Invoke(_setLastAction, data);
            }
            else
            {
                LastCheckLabel.Text = data;
            }
        }
    }
}