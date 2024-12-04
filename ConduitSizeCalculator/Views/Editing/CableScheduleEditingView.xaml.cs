using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Idibri.RevitPlugin.ConduitSizeCalculator.Views
{
    public partial class CableScheduleEditingView : UserControl
    {
        #region Dependency Properties
        #region Selected Cables
        public static readonly DependencyProperty SelectedCablesProperty = DependencyProperty.Register("SelectedCables", typeof(IList), typeof(CableScheduleEditingView), new PropertyMetadata(null, OnSelectedCablesPropertyChanged));

        private static void OnSelectedCablesPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {

        }

        public IList SelectedCables
        {
            get { return (IList)GetValue(SelectedCablesProperty); }
            set { SetValue(SelectedCablesProperty, value); }
        }
        #endregion
        #endregion

        #region Constructors
        public CableScheduleEditingView()
        {
            InitializeComponent();
        }
        #endregion

        #region
        private void TheCableScheduleDataGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedCables == null) { return; }

            if (e.RemovedItems != null)
            {
                foreach (object o in e.RemovedItems)
                {
                    SelectedCables.Remove(o);
                }
            }

            if (e.AddedItems != null)
            {
                foreach (object o in e.AddedItems)
                {
                    SelectedCables.Add(o);
                }
            }
        }
        #endregion
    }
}
