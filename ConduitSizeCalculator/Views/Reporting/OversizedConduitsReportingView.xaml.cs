using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Idibri.RevitPlugin.Common;

namespace Idibri.RevitPlugin.ConduitSizeCalculator.Views
{
    public partial class OversizedConduitsReportingView : UserControl
    {
        public OversizedConduitsReportingView()
        {
            InitializeComponent();
        }

        public void OffendingJunctionBoxItem_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ICommand cmd = Helpers.GetCommand("SelectElementCommand", DataContext);
                object param = (sender as FrameworkElement).DataContext;
                if (cmd != null && cmd.CanExecute(param))
                {
                    cmd.Execute(param);
                }
            }
        }
    }
}
