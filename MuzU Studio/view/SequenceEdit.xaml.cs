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
        private SequenceViewModel _sequenceViewModel = null;
        internal SequenceViewModel ViewModelEdit
        {
            get { return _sequenceViewModel; }
            set
            {
                PropertyListView.SelectionChanged -= PropertyListView_SelectionChanged;
                _sequenceViewModel = value;
                Bindings.Update();
                PropertyListView.SelectedIndex = _sequenceViewModel?.SelectedPropertyIndex??-1;
                PropertyListView.SelectionChanged += PropertyListView_SelectionChanged;
            }
        }

        public SequenceEdit()
        {
            this.InitializeComponent();
            PropertyListView.SelectionChanged += PropertyListView_SelectionChanged;
        }

        private void PropertyListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModelEdit.SelectedPropertyIndex = PropertyListView.SelectedIndex;
        }

        private void Merge_Click(object sender, RoutedEventArgs e)
        {
            ViewModelEdit.Merge(MergeAccordingToProperty.SelectedIndex, MergeType.SelectedIndex);
        }

        private void Normalize_Click(object sender, RoutedEventArgs e)
        {
            ViewModelEdit.Normalize(NormalizeTheProperty.SelectedIndex, int.Parse(NormalizeGroupByBeats.Text));
        }
    }
}
