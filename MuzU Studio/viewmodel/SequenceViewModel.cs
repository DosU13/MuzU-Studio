using MuzU.data;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace MuzU_Studio.viewmodel
{
    internal class SequenceViewModel: BindableBase
    {
        public readonly TimingSequence Data;
        public SequenceViewModel(TimingSequence timingSequence, System.Action<SequenceViewModel> removeSequence)
        {
            Data = timingSequence;
            remove = removeSequence;
            Lyrics = Data.Lyrics;
        }

        public string Name { get => Data.UniqueName; set => Data.UniqueName = value; }

        private System.Action<SequenceViewModel> remove;

        public void Remove() { remove.Invoke(this); }

        public List<TimingTemplateProperty> Properties => Data.TimingTemplate.Properties;
        internal bool LengthEnabled
        {
            get => Data.TimingTemplate.LengthEnabled;
            set => Data.TimingTemplate.LengthEnabled = value;
        }
        private int _selectedPropertyIndex = 0;
        public int SelectedPropertyIndex
        {
            get
            {
                return _selectedPropertyIndex;
            }
            set
            {
                _selectedPropertyIndex = value;
                Debug.WriteLine("Selection CHanged " + value);
            }
        }

        private List<TimingItem> TimingItems => Data.TimingItems;
        private ValueType SelectedPropertyType => Properties[SelectedPropertyIndex].Type;

        public IEnumerable<KeyValuePair<double, long>> GetNormValuesAtTimeWithDur(long timeMicroSec)
        {
            double minValue = TimingItems.Min(i => (double)i.Values[SelectedPropertyIndex]);
            double maxValue = TimingItems.Max(i => (double)i.Values[SelectedPropertyIndex]);
            return TimingItems.Where(i => i.Time <= timeMicroSec && timeMicroSec < i.Time + i.Length.Value)
                .Select(i => KeyValuePair.Create((i.Values[SelectedPropertyIndex] - minValue) / (maxValue - minValue),
                                timeMicroSec-i.Time));
        }

        internal void Merge(int propertyIndex, int mergeType)
        {
            if (IsMelodic())
            {
                _ = (new MessageDialog("Is already Melodic")).ShowAsync();
                return;
            }
            Data.TimingTemplate.Properties.Add(new TimingTemplateProperty("Chord", ValueType.Integer));

            if (mergeType != 2)
            {
                foreach (var g in TimingItems.GroupBy(it => it.Time))
                {
                    double v;
                    if (mergeType == 0)
                        v = g.Max(it => (double)it.Values[propertyIndex]);
                    else 
                        v = g.Min(it => (double)it.Values[propertyIndex]);
                    foreach (TimingItem it in g)
                    {
                        it.Values.Add(g.LongCount());
                        if (it.Values[propertyIndex] != v) TimingItems.Remove(it);
                    }
                }
            }
            else
            {
                foreach (var t in Data.TimingTemplate.Properties) t.Type = ValueType.Decimal;
                List<TimingItem> oldTimingItems = new List<TimingItem>(TimingItems);
                Data.TimingItems = new List<TimingItem>();
                foreach (var g in oldTimingItems.GroupBy(it => it.Time))
                {
                    var newTi = new TimingItem();
                    newTi.Time = g.First().Time;
                    newTi.Length = (long)g.Where(it=>it.Length!=null).Average(it => it.Length.Value);
                    for(int i = 0; i < g.First().Values.Count(); i++)
                    {
                        newTi.Values.Add(g.Average(it => (double)it.Values[i]));
                    }
                    newTi.Values.Add(g.Count());
                    TimingItems.Add(newTi);
                }
            }
        }

        private bool IsMelodic()
        {
            for(int i = 0; i < TimingItems.Count-1; i++)
            {
                if(TimingItems[i].Time == TimingItems[i + 1].Time) return false;
            }
            return true;
        }

        internal void Normalize(int selectedIndex, int v)
        {
            throw new System.NotImplementedException();
        }

        internal bool LyricsExist => Lyrics != null;

        private string _lyrics = null;
        internal string Lyrics { get => _lyrics;
            set
            {
                if(value == "") SetProperty(ref _lyrics, null);
                else SetProperty(ref _lyrics, value);
                OnPropertyChanged(nameof(LyricsExist));
                Data.Lyrics = _lyrics;
                UpdateSyllables();
            }
        }

        internal void CreateLyrics() => Lyrics = "Here paste and edit the lyrics";

        internal string[] Syllables;
        private void UpdateSyllables()
        {
            if(Lyrics==null || Lyrics==""){ Syllables = new string[0]; return; }
            string lyrics = Lyrics;
            Debug.WriteLine("Here Lyrics to Syllables");
            lyrics = Regex.Replace(lyrics, @"\s", "");
            Syllables = lyrics.Split('$');
        }

        internal void AddAfterWhiteSpace()
        {
            var lyrics = Lyrics;
            for(int i=0; i<lyrics.Length-1; i++)
            {
                if (char.IsWhiteSpace(lyrics[i]) && !char.IsWhiteSpace(lyrics[i+1]))
                {
                    lyrics = lyrics.Insert(i + 1, "$");
                }
            }
            Lyrics = lyrics;
        }
    }
}
