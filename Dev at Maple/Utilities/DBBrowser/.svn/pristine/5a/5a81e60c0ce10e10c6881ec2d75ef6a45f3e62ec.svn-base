using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Maple;

namespace DBBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly DbBrowserManager _dbBrowserManager = new DbBrowserManager();
        public MainWindow()
        {
            InitializeComponent();

            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            String ver = "{0}.{1}.{2}".Args(v.Major, v.Minor, v.Build);

            Title = Title + "  v " + ver;
            textBox1.Focus();
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<DBObject> list = null;
            dataGrid1.ItemsSource = null;

            if (!_dbBrowserManager.Loaded)
            {
                _dbBrowserManager.Load();
            }
            if (textBox1.Text.Length < 3)
            {
                chkDeep.IsChecked = false;
                dataGrid1.ItemsSource = null;
            }
            else if (_dbBrowserManager.Loaded && textBox1.Text != "Search..." && textBox1.Text.Length > 2)
            {
                chkDeep.IsChecked = false;
                list = _dbBrowserManager.GetFilteredList(textBox1.Text);
            }
            SetDataGrid(list);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _dbBrowserManager.Load();
        }

        private void dataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                DBObject o = (DBObject)e.AddedItems[0];
                string t = _dbBrowserManager.GetObjectDefinition(o);

                richTextBox1.Document.Blocks.Clear();
                richTextBox1.AppendText(t);

                _selectedObject = o;
                _selectedObjecttext = t;
            }
        }

        private DBObject _selectedObject;
        private string _selectedObjecttext;

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            string temppath = String.Format(@"C:\Temp\{0}.sql", _selectedObject);
            File.WriteAllText(temppath, _selectedObjecttext);
            Process.Start(temppath);
            File.Delete(temppath);
        }

        private void chkDeep_Checked(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            dataGrid1.ItemsSource = null;
            List<DBObject> objects = _dbBrowserManager.GetFilteredExtendedList(textBox1.Text, (bool)chkDeepWholeWord.IsChecked, (bool)chkDeepIgnoreComments.IsChecked);
            SetDataGrid(objects);
            Cursor = null;
        }

        private void SetDataGrid(List<DBObject> list)
        {
            if (list == null || list.Count == 0)
            {
                DBObject ob = new DBObject { Name = "No objects found." };
                list = new List<DBObject> { ob };
            }
            dataGrid1.ItemsSource = list;
        }

        private void chkDeep_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void textBox1_GotFocus(object sender, RoutedEventArgs e)
        {
            textBox1.SelectAll();
        }

        private void CopyDataGridToClipboard(object sender, RoutedEventArgs e)
        {
            string ret = "";
            foreach (DBObject obj in dataGrid1.ItemsSource)
            {
                ret += obj.Description + "\t" + obj.Server + "\t" + obj.Database + "\t" + obj.Type + "\r\n";
            }
            Clipboard.Clear();
            Clipboard.SetDataObject(ret);
        }

    }
}
