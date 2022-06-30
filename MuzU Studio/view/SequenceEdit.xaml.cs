using MuzU_Studio.util;
using MuzU_Studio.viewmodel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MuzU_Studio.view
{
    public sealed partial class SequenceEdit : UserControl
    {
        private SequenceViewModel _sequenceVM = null;
        internal SequenceViewModel SequenceVM
        {
            get { return _sequenceVM; }
            set
            {
                PropertyListView.SelectionChanged -= PropertyListView_SelectionChanged;
                _sequenceVM = value;
                Bindings.Update();
                PropertyListView.SelectedIndex = _sequenceVM?.SelectedPropertyIndex??-1;
                PropertyListView.SelectionChanged += PropertyListView_SelectionChanged;
            }
        }

        public SequenceEdit()
        {
            this.InitializeComponent();
            PropertyListView.SelectionChanged += PropertyListView_SelectionChanged;
        }

        internal IRefresh IRefresh;

        private void PropertyListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SequenceVM.SelectedPropertyIndex = PropertyListView.SelectedIndex;
        }

        private void Merge_Click(object sender, RoutedEventArgs e)
        {
            SequenceVM.Merge(MergeAccordingToProperty.SelectedIndex, MergeType.SelectedIndex);
            IRefresh.Refresh();
        }

        private void Normalize_Click(object sender, RoutedEventArgs e)
        {
            SequenceVM.Normalize(NormalizeTheProperty.SelectedIndex, int.Parse(NormalizeGroupByBeats.Text));
            IRefresh.Refresh();
        }
    }
}
