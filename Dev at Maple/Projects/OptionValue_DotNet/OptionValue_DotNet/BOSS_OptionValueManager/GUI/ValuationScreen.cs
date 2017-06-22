using Maple;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BOSS_OptionValueManager.GUI
{
    public partial class ValuationScreen : Form
    {
        public ValuationScreen()
        {
            InitializeComponent();
        }

        ValuationManager vm;

        /// <summary>
        /// set the valuation date...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            SetupSourceFormatting();
            SetValuationDate(Utilities.GetDefaultValuationDate());
            AddControls();

            string ver = Application.ProductVersion.Substring(0, Application.ProductVersion.LastIndexOf("."));
            Maple.Logon logon = new Logon(Application.ProductName, ver, false);
            logon.SetSplash("Loading...");
            logon.Wait(2000);
            this.Text = "OTC Option Price Valuation Manager  v" + ver;
            logon.Close();
        }

        DateTimePicker exDatePicker;
        private void AddControls()
        {
            exDatePicker = new DateTimePicker();
            ToolStripControlHost exDateHost = new ToolStripControlHost(exDatePicker, "exDate");
            toolStrip2.Items.Insert(8, exDateHost);
        }

        private void LoadStripButton_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try {
                vm.Load();
                ValuationGridView.DataSource = vm.Options;
                ValuationsPage.Text = string.Format("Valuations({0})", vm.Options.Count);

                FormatValuationRows();

                LoadManualOverrides();

                LoadBloombergData();
            } catch (Exception ex) {
                this.Cursor = Cursors.Default;
                MessageBox.Show(ex.Message, "An Error Occurred", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            } finally {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// load the source chars/colors from config...
        /// </summary>
        public void SetupSourceFormatting()
        {
            ManualColorLabel.Text = Utilities.GetSourceCharacter(InputSourceData.InputSource.Override);
            BloombergColorLabel.Text = Utilities.GetSourceCharacter(InputSourceData.InputSource.Bloomberg);
            BOSSColorLabel.Text = Utilities.GetSourceCharacter(InputSourceData.InputSource.BOSS);
            MissingColorLabel.Text = Utilities.GetSourceCharacter(InputSourceData.InputSource.Missing);

            ManualColorLabel.ForeColor = Utilities.GetSourceColour(InputSourceData.InputSource.Override);
            BloombergColorLabel.ForeColor = Utilities.GetSourceColour(InputSourceData.InputSource.Bloomberg);
            BOSSColorLabel.ForeColor = Utilities.GetSourceColour(InputSourceData.InputSource.BOSS);
            MissingColorLabel.ForeColor = Utilities.GetSourceColour(InputSourceData.InputSource.Missing);
        }

        /// <summary>
        /// show the input source details in the tooltip, change colors etc...
        /// </summary>
        public void FormatValuationRows()
        {
            foreach (DataGridViewRow row in ValuationGridView.Rows)
            {
                BOSSOption o = (BOSSOption)row.DataBoundItem;                

                //show the input source details in the tooltip, change colors etc...
                row.Cells["VolatilitySourceCharacter"].ToolTipText = o.VolatilitySource.ToString();
                row.Cells["RateSourceCharacter"].ToolTipText = o.RateSource.ToString();
                row.Cells["DividendSourceCharacter"].ToolTipText = o.DividendSource.ToString();                

                //apply source colors
                row.Cells["VolatilitySourceCharacter"].Style.ForeColor = Utilities.GetSourceColour(o.VolatilitySource.Source);
                row.Cells["RateSourceCharacter"].Style.ForeColor = Utilities.GetSourceColour(o.RateSource.Source);
                row.Cells["DividendSourceCharacter"].Style.ForeColor = Utilities.GetSourceColour(o.DividendSource.Source);                
                
                //set staus colour                
                row.Cells["StatusCharacterColumn"].Style.ForeColor = Utilities.GetStatusColour(o.OptionValue > 0);                

            }
            
        }

        /// <summary>
        /// display the manual overrides used
        /// </summary>
        private void LoadManualOverrides()
        {
            using (Data.DataLayer dl = new Data.DataLayer())
            {
                OverridesGridView.DataSource = dl.GetManualOverrides();
                OverridesPage.Text = string.Format("Structured Trade Rates({0})", OverridesGridView.RowCount);
            }
        }

        /// <summary>
        /// display the bloomberg vols/rates used
        /// </summary>
        private void LoadBloombergData()
        {
            using (Data.DataLayer dl = new Data.DataLayer())
            {
                BloombergVolsGridView.DataSource = dl.GetBloombergData_Volatilities(vm.ValuationDate);
                BloombergVolsPage.Text = string.Format("Bloomberg Vols({0})", BloombergVolsGridView.RowCount);

                BloombergRatesGridView.DataSource = dl.GetBloombergData_Rates(vm.ValuationDate);
                BloombergRatesPage.Text = string.Format("Bloomberg Rates({0})", BloombergRatesGridView.RowCount);
            }
        }

        private void SaveStripButton_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Are you sure you want to save the valuations for " + vm.ValuationDate.ToShortDateString(),"Confirm Save",MessageBoxButtons.YesNo)== DialogResult.Yes)
                vm.Save();
        }

        private void ExportStripButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to export prices to BOSS for " + vm.ValuationDate.ToShortDateString(), "Confirm Export Prices to BOSS", MessageBoxButtons.YesNo) == DialogResult.Yes)
                vm.Export();
        }

        public void UpdateStatus(String Status)
        {
            StatusLabel.Text = Status;
        }

        #region ValuationDate
        /// <summary>
        /// set the global valuation date and update the screen
        /// </summary>
        /// <param name="ValuationDate"></param>
        private void SetValuationDate(DateTime ValuationDate)
        {
            vm = new ValuationManager(ValuationDate);
            vm.StatusChanged += new StatusUpdateDel(UpdateStatus);
            ValuationDateStripButton.Text = vm.ValuationDate.ToString("MMMM dd, yyyy");
        }

        /// <summary>
        /// let the user select a valuationdate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValuationDateStripButton_Click(object sender, EventArgs e)
        {
            ValuationDatePicker vdp = new ValuationDatePicker();
            vdp.ValuationDateSelected += new ValuationDateSelectedHandler(vdp_ValuationDateSelected);
            vdp.Show();
        }

        /// <summary>
        /// the user has selected a valuation date
        /// </summary>
        /// <param name="ValuationDate"></param>
        void vdp_ValuationDateSelected(DateTime ValuationDate)
        {
            SetValuationDate(ValuationDate);
        } 
        #endregion

        /// <summary>
        /// Display the selected override details and allow them to save...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OverridesGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (OverridesGridView.SelectedRows.Count != 0)
            {
                DataRowView r = (DataRowView)OverridesGridView.SelectedRows[0].DataBoundItem;
                SecurityIDTextBox.Text = r["SecurityID"].ToString();
                VolTextBox.Text = r["VolatilityOverride"].ToString();
                RateTextBox.Text = r["RateOverride"].ToString();

                string divData = r["DividendOverride"].ToString();

                exDatePicker.Value = DateTime.ParseExact(divData.Substring(0, 8), "dd/MM/yy", null);
                DividendTextBox.Text = divData.Substring(9).TrimEnd(")".ToCharArray());
            }
        }

        /// <summary>
        /// update/add override values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveOverrideStripButton_Click(object sender, EventArgs e)
        {
            using (Data.DataLayer dl = new Data.DataLayer())
            {
                try
                {
                    int id = int.Parse(SecurityIDTextBox.Text);

                    double vol = 0;
                    if(VolTextBox.Text!="")
                         vol = double.Parse(VolTextBox.Text);

                    double rate = 0;
                    if(RateTextBox.Text!="")
                        rate = double.Parse(RateTextBox.Text);

                    double div = double.Parse(DividendTextBox.Text); // must be double

                    // now we need to put the div in the format which includes ex date i.e. exDate(divRate).   
                    // Not sure why, but other things rely on this format
                    // The format is expected in DataLayer.GetDividends_ManualOverride()
                    string exDateAndDivRate = exDatePicker.Value.ToString("dd/MM/yy") + "(" + DividendTextBox.Text + ")";

                    dl.SaveManualOverride(id, vol, rate, exDateAndDivRate);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not save override details: \r\n" + ex.Message, "Save overrides");
                }

                LoadManualOverrides();
            }            
        }

        /// <summary>
        /// delete existing override
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteOverrideStripButton_Click(object sender, EventArgs e)
        {
            using (Data.DataLayer dl = new Data.DataLayer())
            {
                try
                {
                    int id = int.Parse(SecurityIDTextBox.Text);

                    dl.DeleteManualOverride(id);
                }
                catch (Exception)
                {
                    MessageBox.Show("Could not delete override details", "Delete overrides");
                }

                LoadManualOverrides();
            }  
        }

        private void CopyVolsRatesBtn_Click(object sender, EventArgs e)
        {
            using (Data.DataLayer dl = new Data.DataLayer())
            {
                try
                {
                    DateTime PriceDateFrom = FromDTP.Value.Date;
                    DateTime PriceDateTo = ToDTP.Value.Date;

                    dl.CopyVolatilitiesAndRates(PriceDateFrom, PriceDateTo);
                }
                catch (Exception)
                {
                    MessageBox.Show("Could not copy vols/rates", "Copy Vols/Rates");
                }

                MessageBox.Show("Vols/Rates copied - re-run the valuation", "Copy Vols/Rates");
            }  
        }

        private void OptionTestButton_Click(object sender, EventArgs e) {
            string value = "";
            try {
                BOSSOption option = new BOSSOption() {
                    SecurityID = 0,
                    SecurityName = "Test Security",
                    UnderlyingSecurityName = "Underlying Test",
                    UnderlyingPrice = 0.99,
                    UnderlyingVolatility = 0.3,
                    Rate = 0.04,
                    Strike = 1.1,
                    Maturity = DateTime.Parse("31 March 2012"),
                    Type = OptionValue_DotNet.Option.OptionType.Call,
                    Style = OptionValue_DotNet.Option.OptionStyle.European,
                    FirstTraded = DateTime.Parse("3 Jan 2012")

                };

                option.CalculateOptionValue(DateTime.Parse("1 Feb 2012"));

                value = option.OptionValue.ToString().Substring(0,8);
            } catch (Exception ex) {
                value = ex.Message;
            } finally {
                if (value == "0.014459") {
                    string detail = "Strike = 1.1\r\n";
                    detail += "Underlying Price = 0.99\r\n";
                    detail += "Rate = 4%\r\n";
                    detail += "Vol = 30% = \r\n";
                    detail += "Maturity = 31 Mar 2012\r\n";
                    detail += "European Call\r\n";
                    detail += "Value date = 1 Feb 2012\r\n\r\n";
                    detail += "Calculated option price is " + value;

                    MessageBox.Show("Option valuation is working correctly\r\n\r\n" + detail);
                } else {
                    MessageBox.Show("Option valuation failed. [" + value + "]");
                }
            }
        }

    }
}
