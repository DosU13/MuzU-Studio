using MuzU_Studio.viewmodel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace MuzU_Studio.view.SweetPotato
{
    public class BoardMusicController: MusicPosShareData
    {
        private MainViewModel MainVM;
        private CanvasShareData ShareData;

        internal BoardMusicController(MainViewModel mainViewModel, CanvasShareData canvasShareData)
        {
            MainVM = mainViewModel;
            ShareData = canvasShareData;
            InitSliders();
            InitMediaPlayer();
        }

        private MediaPlayer mediaPlayer;
        private MediaTimelineController timelineController;

        private MediaSource mediaSource;

        private async void InitMediaPlayer()
        {
            try
            {
                mediaPlayer = new MediaPlayer();
                timelineController = new MediaTimelineController();

                StorageFile file = await StorageFile.GetFileFromPathAsync(MainVM.MusicPath);
                if (file == null)
                {
                    Debug.WriteLine("audio doesn't found: " + MainVM.MusicPath);
                    return;
                }

                mediaSource = MediaSource.CreateFromStorageFile(file);
                mediaSource.OpenOperationCompleted += MediaSource_OpenOperationCompleted;
                mediaPlayer.Source = mediaSource;

                mediaPlayer.CommandManager.IsEnabled = false;
                mediaPlayer.TimelineController = timelineController;
                timelineController.Pause();
                //timelineController.PositionChanged += TimelineController_PositionChanged;
            }
            catch (Exception e) { Debug.WriteLine("Here our exception " + e.Message); }
        }

        private long MusicAllignTicks => MainVM.MusicAllign_μs * 10;
        long MusicPosShareData.MusicPosTicks => MusicPosTicks;
        private long MusicPosTicks
        {
            get
            {
                if (timelineController == null) return 0;
                return timelineController.Position.Ticks + MusicAllignTicks;
            }
            set
            {
                timelineController.Position = new TimeSpan(value-MusicAllignTicks);
            }
        }

        public void ToggleMusic()
        {
            if (IsPlaying()) timelineController.Pause();
            else timelineController.Resume();
        }
        public bool IsPlaying() => timelineController.State == MediaTimelineControllerState.Running;

        private void MediaSource_OpenOperationCompleted(MediaSource sender, MediaSourceOpenOperationCompletedEventArgs args)
        {
            ShareData.MusicDurTicks = sender.Duration.GetValueOrDefault().Ticks + MusicAllignTicks;
        }

        //Drawing


        public class LiveSlider
        {
            public Rectangle Rect;
            public double PosX = 0;
            public double PosY = 0;
            public bool Selected = false;

            public bool IsClicked(Point pressPos)
            {
                return PosX <= pressPos.X && pressPos.X <= PosX + Rect.Width &&
                        PosY <= pressPos.Y && pressPos.Y <= PosY + Rect.Height;
            }

            public LiveSlider(double posY, double height, SolidColorBrush color)
            {
                PosY = posY;
                Rect = new Rectangle();
                Rect.Fill = color;
                Rect.Height = height;
            }

            public double Width { set => Rect.Width = value; }

            public void UpdateLayouts(double posX, double width)
            {
                PosX = posX;
                Width = width;
            }

            public void UpdateToCanvas()
            {
                Canvas.SetLeft(Rect, PosX);
                Canvas.SetTop(Rect, PosY);
            }
        }

        // ShareData
        private Canvas SweetCanvas => ShareData.Canvas;
        private double CanvasWidth => ShareData.CanvasWidth;
        private double CanvasHeight => ShareData.CanvasHeight;
        private long BoardStartTicks{ get => ShareData.BoardStartTicks;
                                      set => ShareData.BoardStartTicks = value; }
        private long MusicDurTicks => ShareData.MusicDurTicks;
        private long TicksPerPixel => CanvasSweetPotato.TicksPerPixel;

        private double LensWidth => CanvasWidth * TicksPerPixel / MusicDurTicks * CanvasWidth;
        private int sliderLensWidth = 2;
        private int sliderHeadHeight = 20;
        private int lensHeight => CanvasSweetPotato.TimebarHeight - sliderHeadHeight;
        private int sliderHeadWidth = 20;
        private int sliderTailWidth = 2;

        private double leftOfSliderWhenPressed;
        private LiveSlider dragLiveSlider;

        private LiveSlider lensBack;
        private LiveSlider lens;
        private LiveSlider lensSlider;
        private LiveSlider sliderHead;
        private LiveSlider sliderHeadBack;
        private LiveSlider sliderTail;

        private LiveSlider[] LiveSliders;

        private void InitSliders()
        {
            lensBack = new LiveSlider(0, lensHeight, new SolidColorBrush(Color.FromArgb(255, 50, 50, 50)));
            lens = new LiveSlider(0, lensHeight, new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0x44, 0xD3)));
            lens.Rect.Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x44, 0xD3));
            lensSlider = new LiveSlider(0, lensHeight, new SolidColorBrush(Color.FromArgb(0xFF, 0xFE, 0x47, 0x16)));
            sliderHead = new LiveSlider(lensHeight, sliderHeadHeight, new SolidColorBrush(Color.FromArgb(0xFF, 0xFE, 0x47, 0x16)));
            sliderHeadBack = new LiveSlider(lensHeight, sliderHeadHeight, new SolidColorBrush(Color.FromArgb(255, 100, 100, 100)));
            sliderTail = new LiveSlider(lensHeight + sliderHeadHeight, 100, new SolidColorBrush(Color.FromArgb(0xFF, 0xFE, 0x47, 0x16)));

            LiveSliders = new LiveSlider[] { lensBack, lens, lensSlider, sliderHeadBack, sliderHead, sliderTail };
            foreach (var target in LiveSliders) SweetCanvas.Children.Add(target.Rect);
            Canvas.SetZIndex(sliderTail.Rect, 2);
        }

        private List<Line> Lines = new List<Line>();
        public void Render()
        {
            if (BoardStartTicks < 0) BoardStartTicks = 0;
            if (BoardStartTicks > MusicDurTicks - CanvasWidth * TicksPerPixel)
                BoardStartTicks = MusicDurTicks - (long)(CanvasWidth * TicksPerPixel);

            lensBack.Width = CanvasWidth;
            if (LensWidth != double.PositiveInfinity) lens.UpdateLayouts(
                CanvasWidth * BoardStartTicks / MusicDurTicks, LensWidth);
            lensSlider.UpdateLayouts(CanvasWidth * MusicPosTicks / MusicDurTicks, sliderLensWidth);
            double sliderX = (double)(MusicPosTicks - BoardStartTicks) / TicksPerPixel;
            sliderHead.UpdateLayouts(sliderX - sliderHeadWidth / 2, sliderHeadWidth);
            sliderHeadBack.UpdateLayouts(0, CanvasWidth);
            sliderTail.UpdateLayouts(sliderX - sliderTailWidth / 2, sliderTailWidth);
            double sliderTailHeight = CanvasHeight - lensHeight - sliderHeadHeight;
            if (sliderTailHeight < 0) sliderTailHeight = 0;
            sliderTail.Rect.Height = sliderTailHeight;

            foreach (var target in LiveSliders) target.UpdateToCanvas();

            var barLineClr = new SolidColorBrush(Color.FromArgb(255, 90, 90, 90));
            var beatLineClr = new SolidColorBrush(Color.FromArgb(255, 110, 110, 110));
            foreach (var l in Lines) SweetCanvas.Children.Remove(l);
            Lines = new List<Line>();
            long barLength = MainVM.BarLengthMicroSec*10;
            long bar = - BoardStartTicks % barLength;
            while (bar < CanvasWidth * TicksPerPixel)
            {
                Lines.Add(new Line()
                {
                    X1 = (double)bar / TicksPerPixel,
                    X2 = (double)bar / TicksPerPixel,
                    Y1 = CanvasSweetPotato.TimebarHeight,
                    Y2 = CanvasHeight,
                    Stroke = barLineClr});
                Lines.Add(new Line() {
                    X1 = (bar + barLength * 0.25) / TicksPerPixel,
                    X2 = (bar + barLength * 0.25) / TicksPerPixel,
                    Y1 = CanvasSweetPotato.TimebarHeight,
                    Y2 = CanvasHeight,
                    Stroke = beatLineClr });
                Lines.Add(new Line() {
                    X1 = (bar + barLength * 0.5) / TicksPerPixel,
                    X2 = (bar + barLength * 0.5) / TicksPerPixel,
                    Y1 = CanvasSweetPotato.TimebarHeight,
                    Y2 = CanvasHeight,
                    Stroke = beatLineClr});
                Lines.Add(new Line()
                {
                    X1 = (bar + barLength * 0.75) / TicksPerPixel,
                    X2 = (bar + barLength * 0.75) / TicksPerPixel,
                    Y1 = CanvasSweetPotato.TimebarHeight,
                    Y2 = CanvasHeight,
                    Stroke = beatLineClr,
                });
                bar += barLength;
            }
            foreach (var line in Lines) SweetCanvas.Children.Add(line);
        }

        public void PointerPress(Point pos)
        {
            for (int i = LiveSliders.Length - 1; i >= 0; i--)
            {
                var target = LiveSliders[i];
                if (target.IsClicked(pos))
                {
                    dragLiveSlider = target;
                    if (target == lensSlider) dragLiveSlider = lens;
                    leftOfSliderWhenPressed = pos.X - dragLiveSlider.PosX;
                    break;
                }
            }

            PointerPressDrag(pos);
        }

        public void PointerDrag(Point pos)
        {
            PointerPressDrag(pos);
        }

        private void PointerPressDrag(Point pos)
        {
            if (dragLiveSlider == null) return;
            if (dragLiveSlider == lens)
            {
                double lensX = pos.X - leftOfSliderWhenPressed;
                BoardStartTicks = (long)(lensX / CanvasWidth * MusicDurTicks);
            }
            else if (dragLiveSlider == lensBack)
            {
                double lensMidX = pos.X - LensWidth / 2;
                BoardStartTicks = (long)(lensMidX / CanvasWidth * MusicDurTicks);
            }
            else if (dragLiveSlider == sliderHead)
            {
                double sliderMidX = pos.X - leftOfSliderWhenPressed + sliderHeadWidth / 2;
                MusicPosTicks = (long)(sliderMidX * TicksPerPixel) + BoardStartTicks;
            }
            else if (dragLiveSlider == sliderHeadBack)
            {
                double sliderMidX = pos.X;
                MusicPosTicks = (long)(sliderMidX * TicksPerPixel) + BoardStartTicks;
            }
        }

        internal void PointerRelease()
        {
            dragLiveSlider = null;
        }

        private Point PointSubtract(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }

        internal void Dispose()
        {
            mediaPlayer.Dispose();
            foreach(var target in LiveSliders) SweetCanvas.Children.Remove(target.Rect);
            foreach (var target in Lines) SweetCanvas.Children.Remove(target);
        }
    }
}
