using System.Windows;
using System.Windows.Controls;

namespace Idibri.RevitPlugin.ConduitSizeCalculator.Views
{
    public partial class BoxEditingView : UserControl
    {
        public BoxEditingView()
        {
            InitializeComponent();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb.IsChecked == null)
            {
                cb.IsChecked = false; // Force it to toggle only between true and false
            }
        }

    }
}
