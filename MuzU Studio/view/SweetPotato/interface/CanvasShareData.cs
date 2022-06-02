using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace MuzU_Studio.view.SweetPotato
{
    public interface CanvasShareData
    {
        Canvas Canvas { get; }
        double CanvasWidth { get; }
        double CanvasHeight { get; }
        long MusicDurTicks { get; set; }
        long BoardStartTicks { get; set; }
    }
}
