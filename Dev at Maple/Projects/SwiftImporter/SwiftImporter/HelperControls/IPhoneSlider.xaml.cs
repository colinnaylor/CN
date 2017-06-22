using System;
using System.Windows;
using System.Windows.Controls;

namespace SwiftImporterUI.HelperControls
{
    /// <summary>
    /// Interaction logic for IPhoneSlider.xaml
    /// </summary>
    public partial class IPhoneSlider : UserControl
    {
        public IPhoneSlider()
        {
            InitializeComponent();
        }

        public string Label
        {
            get { return (string)mainChkBox.Content; }
            set { mainChkBox.Content = value; }
        }

        public static readonly DependencyProperty IsOnProperty =
            DependencyProperty.Register("IsOn", typeof(Boolean), typeof(IPhoneSlider));


        /// <summary>
        /// Specifies whether the slider is on or off
        /// </summary>
        public bool IsOn
        {
            get { return (bool)GetValue(IsOnProperty); }
            set { SetValue(IsOnProperty, value); }
        }
    }
}
