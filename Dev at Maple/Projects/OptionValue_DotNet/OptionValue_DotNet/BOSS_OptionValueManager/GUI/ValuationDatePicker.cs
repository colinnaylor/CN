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
    /// <summary>
    /// allow the user to select a date to run the valuation for
    /// </summary>
    public partial class ValuationDatePicker : Form
    {
        public event ValuationDateSelectedHandler ValuationDateSelected;
    
        public ValuationDatePicker()
        {
            InitializeComponent();
        }

        /// <summary>
        /// return the date selected and close the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValuationDateCalendar_DateSelected(object sender, DateRangeEventArgs e)
        {
            if (ValuationDateSelected != null)
            {
                ValuationDateSelected(e.Start);
            }
            this.Close();
        }
    }
}
