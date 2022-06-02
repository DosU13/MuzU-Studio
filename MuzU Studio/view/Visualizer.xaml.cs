using MuzU.data;
using MuzU_Studio.view.SweetPotato;
using MuzU_Studio.viewmodel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MuzU_Studio.view
{
    public sealed partial class Visualizer : UserControl
    {
        private MainViewModel _mainVM = null;
        internal MainViewModel MainVM { 
            get => _mainVM;
            set { _mainVM = value; InitAll(); }
        }
        internal CanvasSweetPotato MusicPosShareData;

        public Visualizer()
        {
            this.InitializeComponent();
        }

        LinearGradientBrush brush;

        private void InitAll()
        {
            CompositionTarget.Rendering += CompositionTarget_Rendering;

            GradientStop left = new GradientStop();
            left.Offset = 0;
            left.Color = Color.FromArgb(0, 255, 20, 147);
            GradientStop mid = new GradientStop();
            mid.Offset = 0.5;
            mid.Color = Color.FromArgb(255, 255, 20, 147);
            GradientStop right = new GradientStop();
            right.Offset = 1;
            right.Color = Color.FromArgb(0, 255, 20, 147);
            GradientStopCollection stops = new GradientStopCollection();
            stops.Add(left);
            stops.Add(mid);
            stops.Add(right);
            brush = new LinearGradientBrush(stops, 0);

            Bindings.Update();
        }

        private long MusicPosTicks => MusicPosShareData.MusicPosShareData.MusicPosTicks;
        double CanvasWidth => VizCanvas.ActualWidth;
        double CanvasHeight => VizCanvas.ActualHeight;
        // /CanvasDataShare

        private RectangleGeometry RectangularBounds
        {
            get
            {
                RectangleGeometry r = new RectangleGeometry();
                r.Rect = new Rect(0, 0, CanvasWidth, CanvasHeight);
                return r;
            }
        }

        private SequenceViewModel Sequence => MainVM.SelectedSequence;
        private void CompositionTarget_Rendering(object sender, object e)
        {
            if (MainVM == null) return;
            if(Sequence == null) return;
            VizCanvas.Children.Clear();
            foreach (var item in Sequence.GetNormValuesAtTimeWithDur(MusicPosTicks/10))
            {
                double w = 50 + 50 * (((MusicPosTicks % 10 / 10.0 )+item.Key)%1.0);
                Rectangle r = new Rectangle();
                r.Width = w;
                r.Height = CanvasHeight;
                r.Fill = Resources["Laser"] as LinearGradientBrush;
                double o = item.Value/50000.0;
                if (o > 1) o = 1;
                //Fill.Opacity = 1 - o * 0.01; // here make something
                VizCanvas.Children.Add(r);
                Canvas.SetLeft(r, (CanvasWidth-100)*item.Key + 50 - w/2);
            }
        }

        private void bestCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            Bindings.Update();
        }
    }
}
