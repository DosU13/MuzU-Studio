using MuzU.data;
using MuzU_Studio.viewmodel;
using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using System.Collections.Generic;
using MuzU_Studio.view.SweetPotato;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MuzU_Studio.view
{
    public sealed partial class CanvasSweetPotato : UserControl, CanvasShareData
    {
        private BoardMusicController BoardMusicCtrl = null;
        private MainViewModel mainVM = null;
        internal MainViewModel MainVM { 
            get => mainVM;
            set { mainVM = value;
                  BoardMusicCtrl?.Dispose();
                  playBtn.Content = "▶";
                  BoardMusicCtrl = new BoardMusicController(value, this);
                  SequenceVM = value.SelectedSequence;
                  Bindings.Update();
            } }

        private NotesController NotesCtrl = null;
        internal SequenceViewModel SequenceVM { set { NotesCtrl?.Dispose();
                                                      NotesCtrl = new NotesController(value, this); } } 

        public CanvasSweetPotato()
        {
            this.InitializeComponent();
            InitAll();
        }

        private void InitAll()
        {
            Window.Current.CoreWindow.PointerPressed += windows_PointerPressed;
            Window.Current.CoreWindow.PointerMoved += windows_PointerMoved;
            Window.Current.CoreWindow.PointerReleased += windows_PointerReleased;
            CompositionTarget.Rendering += CompositionTarget_Rendering;
            Bindings.Update();
        }

        public MusicPosShareData MusicPosShareData => BoardMusicCtrl;
        public static int TimebarHeight => 50;
        public static long TicksPerPixel => 100000;
        // CanvasDataShare
        Canvas CanvasShareData.Canvas => bestCanvas;
        double CanvasShareData.CanvasWidth => canvasContainer.ActualWidth; 
        double CanvasShareData.CanvasHeight => canvasContainer.ActualHeight; 
        private long _musicDurTicks = 1000000;
        long CanvasShareData.MusicDurTicks { get => _musicDurTicks; 
                             set => _musicDurTicks = value; }
        private long _boardStartTicks = 0;
        long CanvasShareData.BoardStartTicks { get => _boardStartTicks;
                                               set => _boardStartTicks = value;}
        // /CanvasDataShare

        private void playBtn_Click(object sender, RoutedEventArgs e)
        {
            if (BoardMusicCtrl == null) return;
            if (BoardMusicCtrl.IsPlaying()) playBtn.Content = "▶";
            else playBtn.Content = "| |";
            BoardMusicCtrl.ToggleMusic();
            //UpdateCompositionRendering();
        }

        private async void Music_Click(object sender, RoutedEventArgs e)
        {
            MusicContentDialog dialog = new MusicContentDialog(MainVM);
            await dialog.ShowAsync();
            MainVM = MainVM;
        }

        private double MusicAllign
        {
            get => MainVM?.MusicAllign_μs/1000000.0??0;
            set { if (MainVM != null) MainVM.MusicAllign_μs = (long)(value * 1000000);
                  MainVM = MainVM;}}

        private RectangleGeometry RectangularBounds
        {
            get { 
                RectangleGeometry r = new RectangleGeometry();
                r.Rect = new Rect(0, 0, canvasContainer.ActualWidth, canvasContainer.ActualHeight);
                return r;
            }
        }

        private bool _isCompositionRendering = false;
        private void UpdateCompositionRendering()
        {
            if (BoardMusicCtrl.IsPlaying() || isPressed)
            {
                if (!_isCompositionRendering)
                {
                    CompositionTarget.Rendering += CompositionTarget_Rendering;
                    _isCompositionRendering = true;
                }
            }
            else if (_isCompositionRendering)
            {
                CompositionTarget.Rendering -= CompositionTarget_Rendering;
                _isCompositionRendering = false;
            }
            Debug.WriteLine("RENDERING " + _isCompositionRendering);
        }

        private void CompositionTarget_Rendering(object sender = null, object e = null)
        {
            BoardMusicCtrl?.Render();
            NotesCtrl?.Render();
        }


        private Point pointerPosWhenPressed;
        private bool isPressed = false;
        private bool isDragging = false;
        private int dragDisTrashHold = 10;
        private long timeOnPress = 0;
        private long dragTimeTrashHold = 3000000;
        private Point pointer = new Point();

        private Point? _bestCanvasPressPoint = null;
        private Point? _windowPressPoint = null;
        private void bestCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _bestCanvasPressPoint = e.GetCurrentPoint(bestCanvas).Position;
            pointer = e.GetCurrentPoint(bestCanvas).Position;
            pointerPosWhenPressed = pointer;

            isPressed = true;
            timeOnPress = DateTime.Now.Ticks;
            BoardMusicCtrl?.PointerPress(pointer);
            NotesCtrl?.PointerPress(pointer);
            //UpdateCompositionRendering();
        }

        private void windows_PointerPressed(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs e)
        {
            _windowPressPoint = e.CurrentPoint.Position;
        }
        
        private void bestCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if(_bestCanvasPressPoint==null || _windowPressPoint==null) 
                pointer = e.GetCurrentPoint(bestCanvas).Position;
        }
            
        private void windows_PointerMoved(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs e)
        {
            if (_bestCanvasPressPoint != null && _windowPressPoint != null)
                pointer = PointSubtract(e.CurrentPoint.Position, 
                    PointSubtract(_windowPressPoint.Value, _bestCanvasPressPoint.Value));
            if (isPressed && !isDragging)
            {
                Point dragTranslation = PointSubtract(pointer, pointerPosWhenPressed);
                if (Math.Sqrt(Math.Pow(dragTranslation.X, 2) + Math.Pow(dragTranslation.Y, 2)) > dragDisTrashHold) isDragging = true;
                if (DateTime.Now.Ticks - timeOnPress > dragTimeTrashHold) isDragging = true;
            }
            else if (isDragging)
            {
                BoardMusicCtrl?.PointerDrag(pointer);
                NotesCtrl?.PointerDrag(pointer);
            }
        }

        private void windows_PointerReleased(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs e)
        {
            BoardMusicCtrl?.PointerRelease();
            NotesCtrl?.PointerRelease();
            isPressed = false;
            isDragging = false;
            //UpdateCompositionRendering();
            //CompositionTarget_Rendering();
        }

        private Point PointSubtract(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }

        private void bestCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            Bindings.Update();
        }
    }
}
