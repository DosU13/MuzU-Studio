using MuzU.data;
using MuzU_Studio.viewmodel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace MuzU_Studio.view.SweetPotato
{
    public class NotesController
    {
        private SequenceViewModel SequenceVM;
        private CanvasShareData ShareData;

        internal NotesController(SequenceViewModel sequenceVM, CanvasShareData shareData)
        {
            SequenceVM = sequenceVM;
            ShareData = shareData;
            InitNotes();
        }

        private List<TimingItem> TimingItems => SequenceVM.Data.TimingItems;
        private int PropertyIndex => SequenceVM.SelectedPropertyIndex;

        private static int TimebarHeight => CanvasSweetPotato.TimebarHeight;
        private static long TicksPerPixel => CanvasSweetPotato.TicksPerPixel;
        // ShareData
        private Canvas SweetCanvas => ShareData.Canvas;
        private double CanvasWidth => ShareData.CanvasWidth;
        private double CanvasHeight => ShareData.CanvasHeight - TimebarHeight;
        private long MusicDurTicks => ShareData.MusicDurTicks;
        private long BoardStartTicks => ShareData.BoardStartTicks;

        private LiveNote[] LiveNotes = new LiveNote[0];
        private double minValue;
        private double maxValue;
        private void InitNotes()
        {
            if (SequenceVM == null || TimingItems.Count == 0)
            {
                LiveNotes = new LiveNote[0];
                return;
            }
            
            maxValue = TimingItems.Max(i => i.Values[PropertyIndex]);
            minValue = TimingItems.Min(i => i.Values[PropertyIndex]);
            LiveNotes = new LiveNote[TimingItems.Count];
            for (int i = 0; i < TimingItems.Count; i++)
            {
                TimingItem item = SequenceVM.Data.TimingItems[i];
                LiveNotes[i] = new LiveNote(item);
            }
        }

        private double MinNoteHeight = 20;
        private double HeightPerInteger {get{
                double r = CanvasHeight / (maxValue - minValue + 1);
                if (r > MinNoteHeight) return MinNoteHeight;
                return r;
            }}
        internal double NumberToTop(double number)
        {
            return (number - minValue) * HeightPerInteger;
        }
        private long TopToInteger(double top)
        {
            return (long)(top / HeightPerInteger + minValue);
        }

        internal class LiveNote
        {
            internal TimingItem TimingItem;
            internal Rectangle Rect;
            internal Point Pos = new Point(0, 0);
            internal bool Selected = false;
            internal TextBlock Syllable;
            internal LiveNote(TimingItem timingItem)
            {
                TimingItem = timingItem;
                Rect = new Rectangle();
                Syllable = new TextBlock();
                Rect.Fill = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0x7B, 0x19));
                Rect.Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0x46, 0x1d, 0x00));
                Rect.StrokeThickness = 2;
                Pos.X = TimingItem.Time*10 / TicksPerPixel; // properly convert microsecond to ticks
                Rect.Width = TimingItem.Length.Value*10 / TicksPerPixel; //TODO It can be null 
            }

            public bool IsClicked(Point pressPos)
            {
                return Pos.X <= pressPos.X && pressPos.X <= Pos.X + Rect.Width &&
                        Pos.Y <= pressPos.Y && pressPos.Y <= Pos.Y + Rect.Height;
            }

            public void UpdateLayouts(double posY, double height)
            {
                Pos.Y = posY;
                Rect.Height = height;
            }

            private bool IsVisible = false;
            public void UpdateToCanvas(Canvas canvas,double boardStart, double boardEnd)
            {
                if (boardStart <= Pos.X + Rect.Width && Pos.X <= boardEnd && !IsVisible)
                {
                    canvas.Children.Add(Rect);
                    canvas.Children.Add(Syllable);
                    IsVisible = true;
                }
                else if ((boardStart > Pos.X + Rect.Width || Pos.X > boardEnd) && IsVisible)
                {
                    canvas.Children.Remove(Rect);
                    canvas.Children.Remove(Syllable);
                    IsVisible = false;
                }
                if (IsVisible)
                {
                    Canvas.SetLeft(Rect, Pos.X - boardStart);
                    Canvas.SetTop(Rect, Pos.Y + TimebarHeight);
                    Canvas.SetZIndex(Rect, 1);
                    Canvas.SetLeft(Syllable, Pos.X - boardStart);
                    Canvas.SetTop(Syllable, Pos.Y + TimebarHeight);
                    Canvas.SetZIndex(Syllable, 1);
                }
            }
        }

        private double _lastHeightPerInteger = 0;
        private int _lastPropertyIndex = 0;
        internal void Render()
        {
            if (SequenceVM == null || TimingItems.Count == 0) return;
            if (_lastHeightPerInteger != HeightPerInteger || _lastPropertyIndex != PropertyIndex)
            {
                if (_lastPropertyIndex != PropertyIndex)
                {
                    maxValue = TimingItems.Max(i => i.Values[PropertyIndex]);
                    minValue = TimingItems.Min(i => i.Values[PropertyIndex]);
                }
                foreach (var target in LiveNotes)
                {
                    target.UpdateLayouts(NumberToTop(target.TimingItem.Values[PropertyIndex]), HeightPerInteger);
                }
            }
            _lastHeightPerInteger = HeightPerInteger;
            _lastPropertyIndex = PropertyIndex;
            foreach (var target in LiveNotes)
            {
                target.UpdateToCanvas(SweetCanvas, (double)(BoardStartTicks)/TicksPerPixel,
                    (double)BoardStartTicks / TicksPerPixel + CanvasWidth);
            }
            if (SequenceVM.LyricsExist)
            {
                for (int i = 0; i < LiveNotes.Length && i < SequenceVM.Syllables.Length; i++)
                {
                    LiveNotes[i].Syllable.Text = SequenceVM.Syllables[i];
                }
            }
        }

        internal void PointerPress(Point pointer)
        { }

        internal void PointerDrag(Point pointer)
        {
        }

        internal void PointerRelease()
        {
            
        }

        internal void Dispose()
        {
            foreach (var target in LiveNotes) SweetCanvas.Children.Remove(target.Rect);
        }
    }
}
