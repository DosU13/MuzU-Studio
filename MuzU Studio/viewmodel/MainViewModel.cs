using MuzU;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MuzU.data;
using Windows.UI.Xaml.Controls;
using MuzU.util;

namespace MuzU_Studio.viewmodel
{
    internal class MainViewModel
    {
        private readonly MuzUProject Project;
        private List<TimingSequence> projectSequences => Project.data.TimingSequences;

        public MainViewModel(MuzUProject muzUProject)
        {
            Project = muzUProject;
            Sequences = new ObservableCollection<SequenceViewModel>();
            foreach (var sequence in Project.data.TimingSequences)
            {
                Sequences.Add(new SequenceViewModel(sequence, RemoveSequence));
            }
            Sequences.CollectionChanged += OnSequencesCollectionChanged;
        }

        public string MusicPath { get => Project.data.MusicPath; 
                                  set => Project.data.MusicPath = value; }
        public string MusicDescription { get => Project.data.MusicDescription;
                                         set => Project.data.MusicDescription = value;}
        public long MusicAllign_μs { get => Project.data.MusicAllign_μs;
                                     set => Project.data.MusicAllign_μs = value;}
        public string TimeSignature { get => Project.data.TimeSignature; 
                             internal set => Project.data.TimeSignature = value; }
        public double? BPM {get{
                if (Project.data.MicrosecondsPerQuarterNote == null) return null;
                else return MuzUConverter.GetBPM(Project.data.MicrosecondsPerQuarterNote.Value, TimeSignature);}}

        public int SelectedSequenceIndex { get; set; } = -1;

        internal void TempoChanged(double bPM, string timeSignature)
        {
            long? oldMPQ = Project.data.MicrosecondsPerQuarterNote;
            long newMPQ = MuzUConverter.GetMicrosecondsPerQuarterNote(bPM, timeSignature);
            Project.data.MicrosecondsPerQuarterNote = newMPQ;
            Project.data.TimeSignature = timeSignature;
            if(oldMPQ != null)
            {
                double timeFactor = (double)newMPQ / oldMPQ.Value;
                foreach(var item in Project.data.TimingSequences)
                {
                    foreach(var i in item.TimingItems)
                    {
                        i.Time = (long)(i.Time * timeFactor);
                        i.Length = (long)(i.Length * timeFactor);
                    }
                }
            }
        }

        public SequenceViewModel SelectedSequence => 
            (Sequences.Count > SelectedSequenceIndex && SelectedSequenceIndex>=0) ? Sequences[SelectedSequenceIndex] : null;
        public string SelectedSequenceName => SelectedSequence?.Name??"";
        public ObservableCollection<SequenceViewModel> Sequences { get; }
        private void OnSequencesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    projectSequences.Add((e.NewItems[0] as SequenceViewModel).Data);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    projectSequences.Remove((e.OldItems[0] as SequenceViewModel).Data);
                    break;
            }
        }

        public void AddNewSequence()
        {
            Sequences.Add(new SequenceViewModel(new TimingSequence(), RemoveSequence));
        }

        public async void RemoveSequence(SequenceViewModel sequenceViewModel)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Are you sure you want to delete "+sequenceViewModel.Name+"?";
            dialog.PrimaryButtonText = "Yes";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                if(SelectedSequence == sequenceViewModel) SelectedSequenceIndex = -1;
                Sequences.Remove(sequenceViewModel);
            }
        }

        public long BarLengthMicroSec
        {
            get
            {
                if (Project.data.TimeSignature == "4/4")
                {
                    return (Project.data.MicrosecondsPerQuarterNote ?? 250000L) * 4;
                }
                else return (Project.data.MicrosecondsPerQuarterNote ?? 250000L) * 4;
            }
        }
    }
}
