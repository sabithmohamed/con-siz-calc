using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Idibri.RevitPlugin.ConduitSizeCalculator.Views
{
    public partial class ConduitScheduleEditingView : UserControl
    {
        #region Dependency Properties
        #region Selected Conduits
        public static readonly DependencyProperty SelectedConduitsProperty = DependencyProperty.Register("SelectedConduits", typeof(IList), typeof(ConduitScheduleEditingView), new PropertyMetadata(null, OnSelectedConduitsPropertyChanged));

        private static void OnSelectedConduitsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {

        }

        public IList SelectedConduits
        {
            get { return (IList)GetValue(SelectedConduitsProperty); }
            set { SetValue(SelectedConduitsProperty, value); }
        }
        #endregion
        #endregion

        #region Constructors
        public ConduitScheduleEditingView()
        {
            InitializeComponent();
        }
        #endregion

        #region Event Handlers
        private void TheConduitScheduleDataGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedConduits == null) { return; }

            if (e.RemovedItems != null)
            {
                foreach (object o in e.RemovedItems)
                {
                    SelectedConduits.Remove(o);
                }
            }

            if (e.AddedItems != null)
            {
                foreach (object o in e.AddedItems)
                {
                    SelectedConduits.Add(o);
                }
            }
        }
        #endregion
    }
}
