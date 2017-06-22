using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using Maple;
using SwiftImporterUI.Model;

namespace SwiftImporterUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var mainvm = new MainViewModel();

            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            String ver = "{0}.{1}.{2}".Args(v.Major, v.Minor, v.Build);

            this.Title = this.Title + "  v {0}".Args(ver);

            mainvm.ProgressChanged += mainvm_ProgressChanged;
            DataContext = mainvm;
            
        }

        void mainvm_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() => progBar.Value = e.ProgressPercentage));
        }

        private void exitMenu_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void aboutMenu_Click(object sender, RoutedEventArgs e)
        {
            SQLServer db = new SQLServer(SwiftDataLayer.Dsn);
            string connStr = db.ConnectionString;

            MessageBox.Show("Connected to " + connStr, "About");
        }
    }
}
