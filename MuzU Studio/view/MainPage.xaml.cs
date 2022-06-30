using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using MuzU;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using MuzU.data;
using MuzU_Studio.Model;
using MuzU_Studio.viewmodel;
using Windows.UI.Core.Preview;
using Windows.Storage.AccessCache;
using MuzU_Studio.util;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MuzU_Studio
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IRefresh
    {
        public MainPage()
        {
            this.InitializeComponent();

            SequenceEdit.IRefresh = this;
            _ = LoadLocalSettingsAsync();
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnWindowClose;
        }

        public MuzUProject _project;
        public MuzUProject Project { 
            get => _project;
            set
            {
                _project = value;
                MainVM = new MainViewModel(Project);
                SweetPotato.MainVM = MainVM;
                Visualizer.MainVM = MainVM;
                Visualizer.MusicPosShareData = SweetPotato;
                //SequenceEdit.BeatLengthShareData = this;

                SweetPotato.SequenceVM = MainVM.SelectedSequence;
                SequenceEdit.SequenceVM = MainVM.SelectedSequence;
                Bindings.Update();
            } 
        }
        private IStorageFile projectFile = null;
        private MainViewModel MainVM;
        private bool existProject => Project != null;
        private bool existProjectFile => projectFile != null;
        private string WindowTitle => projectFile?.Name ?? "";
        private void SequenceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SweetPotato.SequenceVM = MainVM.SelectedSequence;
            SequenceEdit.SequenceVM = MainVM.SelectedSequence;
            Bindings.Update();
        }

        private async void NewEmpty_Click(object sender, RoutedEventArgs e)
        {
            if (existProject) if(!(await SaveWorkDialog())) return;
            Project = new MuzUProject();
            projectFile = null;
        }

        private async void NewMidiSimple_Click(object sender, RoutedEventArgs e)
        {
            if (existProject) if (!(await SaveWorkDialog())) return;
            await PickMidiAndImport(true, true);
            projectFile = null;
        }

        private async void NewMidiRaw_Click(object sender, RoutedEventArgs e)
        {
            if (existProject) if (!(await SaveWorkDialog())) return;
            await PickMidiAndImport(true, false);
            projectFile = null;
        }

        private async void AddMidiSimple_Click(object sender, RoutedEventArgs e)
        {
            if (!existProject) return;
            await PickMidiAndImport(false, true);
            Project = Project;
        }

        private async void AddMidiRaw_Click(object sender, RoutedEventArgs e)
        {
            if (!existProject) return;
            await PickMidiAndImport(false, false);
            Project = Project;
        }

        private async void Open_Click(object sender, RoutedEventArgs e)
        {
            if (existProject) if (!(await SaveWorkDialog())) return;
            await LoadWithFilePicker();
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (projectFile != null) await SaveToFile(projectFile);
            else await SaveWithFilePicker();
        }

        private async void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            await SaveWithFilePicker();
        }

        private void ListViewModelAddNew_Click(object sender, RoutedEventArgs e)
        {
            if(MainVM!=null) MainVM.AddNewSequence();
        }

        private async Task<bool> SaveWorkDialog()
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Save your work?";
            dialog.PrimaryButtonText = "Save";
            dialog.SecondaryButtonText = "Don't Save";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                if (projectFile != null) return await SaveToFile(projectFile);
                else return await SaveWithFilePicker();
            }
            else if (result == ContentDialogResult.Secondary) return true;
            else if (result == ContentDialogResult.None) return false;
            return false;
        }

        private async Task<bool> SaveWithFilePicker()
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeChoices.Add("MuzU file", new List<string>() { ".muzu" });
            picker.SuggestedFileName = Project.data.Name+".muzu";
            try
            {
                StorageFile file = await picker.PickSaveFileAsync();
                if (file != null)
                {
                    return await SaveToFile(file);
                }
            }
            catch (Exception ex)
            { Debug.WriteLine(ex); }
            return false;
        }

        private async Task<bool> SaveToFile(IStorageFile file)
        {
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                Project.Save(stream);
                stream.Close();
                if (projectFile == null) projectFile = file;
                return true;
            }
        }

        private async Task<bool> LoadWithFilePicker()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".muzu");
            try
            {
                StorageFile file = await picker.PickSingleFileAsync();
                return await LoadFromFile(file);
            }
            catch (Exception ex)
            {
                Console.WriteLine("HERE: -->>" + ex.ToString());
            }
            return false;
        }

        private async Task<bool> LoadFromFile(IStorageFile file)
        {
            if (file != null)
            {
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    Project = new MuzUProject(stream.AsStream());
                    projectFile = file;
                    return true;
                }
            }
            return false;
        }

        private async Task PickMidiAndImport(bool IsNew, bool IsSimple)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".midi");
            picker.FileTypeFilter.Add(".mid");

            var file = await picker.PickSingleFileAsync();
            if (file == null) return; // pick cancelled
            
            using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
            {
                if (IsNew)
                {
                    MuzUProject newProject = MidiImporter.Import(fileStream.AsStream(), file.DisplayName, IsSimple);
                    Project = newProject;
                    projectFile = null;
                }
                else
                {
                    Project.data.TimingSequences.AddRange(MidiImporter.ImportTimingSequences(
                        MidiFile.Read(fileStream.AsStream()), file.DisplayName, IsSimple));
                }
            }
            Bindings.Update();
        }

        private async void OnWindowClose(object sender,  SystemNavigationCloseRequestedPreviewEventArgs args)
        {
            args.Handled = true;
            if (existProject) if (!(await SaveWorkDialog())) return;
            SaveLocalSettings();
            App.Current.Exit();
        }

        private void SaveLocalSettings()
        {
            if (projectFile == null) ApplicationData.Current.LocalSettings.Values["ProjectFileFutureAccessToken"] = null;
            else
            {
                string faToken = StorageApplicationPermissions.FutureAccessList.Add(projectFile);
                ApplicationData.Current.LocalSettings.Values["ProjectFileFutureAccessToken"] = faToken;
            }
        }

        private async Task LoadLocalSettingsAsync()
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("ProjectFileFutureAccessToken"))
            {
                string faToken = ApplicationData.Current.LocalSettings.Values["ProjectFileFutureAccessToken"].ToString();
                IStorageFile _projectFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(faToken);
                await LoadFromFile(_projectFile);
            }
        }

        public void Refresh()
        {
            if (MainVM == null) return;
            SweetPotato.MainVM = MainVM;
            Visualizer.MainVM = MainVM;
            Bindings.Update();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Refresh();
        }
    }
}
