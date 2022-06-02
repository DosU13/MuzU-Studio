using MuzU;
using MuzU.util;
using MuzU_Studio.viewmodel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MuzU_Studio.view
{
    public sealed partial class MusicContentDialog : ContentDialog
    {
        private MainViewModel MainVM;
        internal MusicContentDialog(MainViewModel mainViewModel)
        {
            this.InitializeComponent();
            MainVM = mainViewModel;
            MusicPath = MainVM.MusicPath;
            MusicDescription = MainVM.MusicDescription;
            BPM = MainVM.BPM??120;
            TimeSignature = MainVM.TimeSignature??"4/4";
        }

        private string MusicPath { get; set; }
        private string MusicDescription { get; set; }
        private double BPM { get; set; }
        private string timeSignature;
        private string TimeSignature { get => timeSignature;
            set
            {
                if (!Char.IsDigit(value.First()) || !Char.IsDigit(value.Last()))
                {
                    Bindings.Update();
                    return;
                }
                bool slashAppeared = false;
                foreach (char c in value)
                {
                    if (c == '/' && !slashAppeared) slashAppeared = true;
                    else if (!Char.IsDigit(c))
                    {
                        Bindings.Update();
                        return;
                    }
                }
                int x = int.Parse(value.Split('/')[1]);
                if (!(x != 0 && ((x & (x - 1)) == 0))) //is power of two
                {
                    Bindings.Update();
                    return;
                }
                timeSignature = value;
            } }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        { 
            MainVM.MusicPath = MusicPath;
            MainVM.MusicDescription = MusicDescription;
            if(MainVM.BPM != BPM || MainVM.TimeSignature != TimeSignature)
            {
                MainVM.TempoChanged(BPM, TimeSignature);
            }
        }

        private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void SelectMusic_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".wav");
            try
            {
                StorageFile file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    MusicPath = file.Path;
                    if (MusicDescription == null) Path.GetFileNameWithoutExtension(MusicPath);
                    Bindings.Update();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("HERE: -->>" + ex.ToString());
            }
        }
    }
}
