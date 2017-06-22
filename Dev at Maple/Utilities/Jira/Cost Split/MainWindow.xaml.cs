using Cost_Split.Controller;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cost_Split {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private CostSplitManager costSplits;

        private void MainGrid_Initialized(object sender, EventArgs e) {

        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e) {
            costSplits = new CostSplitManager();

            string monthStart = DateTime.Now.ToString("01 MMM yyyy");
            DateFromPicker.SelectedDate = DateTime.Parse(monthStart);
            DateToPicker.SelectedDate = DateTime.Now.AddDays(-1);
            DateTime firstOfLastMonth = DateTime.Parse("1 " + DateTime.Now.AddMonths(-1).ToString("MMM yyyy"));
            FetchFromDatePicker.SelectedDate = firstOfLastMonth;
            EmployeeTextbox.Text = Environment.UserName;

            if (Properties.Settings.Default.ConfigName != "Release") {

                this.Title += " - " + Properties.Settings.Default.ConfigName + " - " + Database.ServerDbconnection();
            }

        }

        private void QueryButton_Click_1(object sender, RoutedEventArgs e) {
            this.Cursor = Cursors.Wait;

            try {
                costSplits.FetchWorkItems((DateTime)DateFromPicker.SelectedDate, (DateTime)DateToPicker.SelectedDate, EmployeeTextbox.Text);

                ResultGrid.ItemsSource = costSplits.WorkItems;

                foreach(DataGridColumn col in ResultGrid.Columns){
                    if (col.Header.ToString() == "ID") col.Visibility = System.Windows.Visibility.Hidden;
                    if (col.Header.ToString() == "UpdateTime") col.Visibility = System.Windows.Visibility.Hidden;
                }


                CopyButton.Visibility = System.Windows.Visibility.Visible;
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Oops", MessageBoxButton.OK, MessageBoxImage.Error);
            } finally {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void FetchButton_Click_1(object sender, RoutedEventArgs e) {
            this.Cursor = Cursors.Wait;

            try {
                if (FetchFromDatePicker.SelectedDate == null) {
                    MessageBox.Show("Please select a date next to the Fetch button.", "Oops", MessageBoxButton.OK, MessageBoxImage.Error);
                } else {
                    DateTime from = (DateTime)FetchFromDatePicker.SelectedDate;
                    costSplits.AddWorkItemsToCache(from);
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Oops", MessageBoxButton.OK, MessageBoxImage.Error);
            } finally {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void CopyButton_Click_1(object sender, RoutedEventArgs e) {
            ResultGrid.SelectAll();
            ResultGrid.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            ApplicationCommands.Copy.Execute(null, this.ResultGrid);

            ResultGrid.UnselectAll();
        }
    }
}
