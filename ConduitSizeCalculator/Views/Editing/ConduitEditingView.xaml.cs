using System.Windows;
using System.Windows.Controls;

namespace Idibri.RevitPlugin.ConduitSizeCalculator.Views
{
    public partial class ConduitEditingView : UserControl
    {
        #region Dependency Properties
        #region Row Header
        public static readonly DependencyProperty RowHeaderProperty = DependencyProperty.Register("RowHeader", typeof(string), typeof(ConduitEditingView), new PropertyMetadata(null, OnRowHeaderPropertyChanged));

        private static void OnRowHeaderPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {

        }

        public string RowHeader
        {
            get { return (string)GetValue(RowHeaderProperty); }
            set { SetValue(RowHeaderProperty, value); }
        }
        #endregion

        #region Is Size Free Text
        public static readonly DependencyProperty IsSizeFreeTextProperty = DependencyProperty.Register("IsSizeFreeText", typeof(bool), typeof(ConduitEditingView), new PropertyMetadata(false, OnIsSizeFreeTextPropertyChanged));

        private static void OnIsSizeFreeTextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {

        }

        public bool IsSizeFreeText
        {
            get { return (bool)GetValue(IsSizeFreeTextProperty); }
            set { SetValue(IsSizeFreeTextProperty, value); }
        }
        #endregion
        #endregion

        public ConduitEditingView()
        {
            InitializeComponent();
        }
    }
}
