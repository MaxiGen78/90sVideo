using System;
using System.ComponentModel;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Interop;
using System.Windows.Forms;
using WindowsControl= System.Windows.Controls;

namespace WPF90sVideo
{
    
    public partial class Window
    {
        public ObservableCollection<VideoListData> VideoList = new ObservableCollection<VideoListData>();
        string artistName;
        string songTitle;
        string ButtonState;
        bool filler = false;
        private bool _inStateChange;
        bool VidLooseFocus = true;
        bool ControlLooseFocus = true;
        int posIndex;
        System.Timers.Timer MouseHideTimer = new System.Timers.Timer(3000);
        
        public Window()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            Closing += Window_Closing;
            Window mainWindow = System.Windows.Application.Current.Windows.OfType<Window>().Single();

            //Mouse cursor hiding
            MouseHideTimer.AutoReset = false;
            MouseHideTimer.Elapsed += delegate { MouseExt.SafeOverrideCursor(System.Windows.Input.Cursors.None); };
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                //Preparing incomplete downloads for removal upon application next start
                if (SharedVariables.FileSizeToDelete != new FileInfo(SharedVariables.FileName).Length)
                {
                    Properties.Settings.Default.FileNameToDelete = SharedVariables.FileName;
                }
            }
            catch
            {
            }
            //Saving window size and location (WindowHandling.cs)
            Properties.Settings.Default.MainWindowPlacement = this.GetPlacement();
            Properties.Settings.Default.Save();
        }

        //Saving window size and location (WindowHandling.cs)
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.SetPlacement(Properties.Settings.Default.MainWindowPlacement);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
                //First run after install, making controls visible
                if (Properties.Settings.Default.FirstRun == true)
                {
                    VideoPlayer.Volume = 0.6;
                    Properties.Settings.Default.VolumeValue = VideoPlayer.Volume;
                    Properties.Settings.Default.LeftSide_Pin = "Checked";
                    Properties.Settings.Default.FolderInfo_Pin = "Checked";
                    Properties.Settings.Default.FirstRun = false;
                }

                //Removal of the incomplete download
                SharedVariables.FileNameToDelete = Properties.Settings.Default.FileNameToDelete;
                if (SharedVariables.FileNameToDelete != null)
                {
                    try
                    {
                        new FileInfo(SharedVariables.FileNameToDelete).Delete();
                    }
                    catch
                    {
                    }
                }

                //Prevent video freeze when moving window between monitors
                var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
                if (hwndSource != null)
                {
                    var hwndTarget = hwndSource.CompositionTarget;
                    if (hwndTarget != null) hwndTarget.RenderMode = RenderMode.SoftwareOnly;
                }

                //Restoring variables from settings
                VideoPlayer.Volume = Properties.Settings.Default.VolumeValue;
                LeftSide.Opacity = 0;
                Controls.Opacity = 0;
                SharedVariables.LeftSide_Pin = Properties.Settings.Default.LeftSide_Pin;
                if (SharedVariables.LeftSide_Pin == "Checked")
                {
                    WindowsControl.CheckBox leftSideCheckBox = Controls.FindName("LeftSide_Pin") as WindowsControl.CheckBox;
                    leftSideCheckBox.IsChecked = true;
                    LeftSide.Opacity = 0.6;
                }

                SharedVariables.FolderInfo_Pin = Properties.Settings.Default.FolderInfo_Pin;
                if (SharedVariables.FolderInfo_Pin == "Checked")
                {
                    WindowsControl.CheckBox folderInfoCheckBox = Controls.FindName("FolderInfo_Pin") as WindowsControl.CheckBox;
                    folderInfoCheckBox.IsChecked = true;
                    Controls.Opacity = 0.6;
                }

                SharedVariables.StationChecked = Properties.Settings.Default.StationChecked;
                if (SharedVariables.StationChecked == "")
                {
                    SharedVariables.StationChecked = "Hits";
                }
                WindowsControl.RadioButton rb = Controls.FindName(SharedVariables.StationChecked) as WindowsControl.RadioButton;
                rb.IsChecked = true;

                SharedVariables.path = Properties.Settings.Default.FolderPathSettings;
                if (SharedVariables.path == "")
                {
                    FolderBrowserDialog fbd = new FolderBrowserDialog();
                    fbd.Description = "Set Video Folder";
                    fbd.ShowDialog();
                    SharedVariables.path = fbd.SelectedPath;
                    Properties.Settings.Default.FolderPathSettings = SharedVariables.path;
                    if (SharedVariables.path =="") { System.Windows.Application.Current.Shutdown(); }
                }

                try
                {
                    SharedVariables.SetFolderSize = Properties.Settings.Default.FolderSizeSettings;
                }
                catch (NullReferenceException)
                {
                    Properties.Settings.Default.FolderSizeSettings = 1000000000;
                }
                SharedVariables.SetFolderSize = Properties.Settings.Default.FolderSizeSettings;

                SharedVariables.CurrentFolderSize = new DirectoryInfo(SharedVariables.path).GetFiles("*.*", SearchOption.TopDirectoryOnly).Sum(file => file.Length);
                string folderSize = SharedVariables.CurrentFolderSize.ToSize(MyExtension.SizeUnits.MB);
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => FolderInfo.Content = folderSize + " MB" + "/" + SharedVariables.SetFolderSize + " MB"));

                //Timer to query http://stream-service.loverad.io/v3/90s90s
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(30);
                timer.Tick += TimerTickAsync;
                timer.Start();
                TimerTickAsync(null, null);

                FileSystemWatcher folderWatcher = new FileSystemWatcher();
                folderWatcher.Path = SharedVariables.path;
                folderWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size;
                folderWatcher.Changed += new FileSystemEventHandler(OnChangedFolderSize);
                folderWatcher.Deleted += FolderWatcher_Deleted;
                folderWatcher.EnableRaisingEvents = true;
                OnChangedFolderSize(null, null);
        }

        private void FolderWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            GetCurrentFolderSize();
        }

        private void OnChangedFolderSize(object sender, FileSystemEventArgs e)
        {
            GetCurrentFolderSize();
        }

        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            SharedVariables.VideoPlayerIsPlaying = false;

            //Position of next video to play
            int posNextToPlay = VideoList.FirstOrDefault(i => i.Vid == SharedVariables.CurrentlyPlayed).SeqPosition + 1;

            //Increment SeqPosition, choosing next item in the observablecollection
            posIndex = posIndex + 1;

            //If next video is not available, force TimerTickAcync
            if (VideoList.IndexOf(VideoList.Where(i => i.SeqPosition == posNextToPlay).FirstOrDefault())==-1)
            {
                filler = true; // switch to a different station 
                TimerTickAsync(sender, e);
            }
            else
            {
                PrePlayProcessing();
            }
        }

        async void TimerTickAsync(object sender, EventArgs e)
        {
            YoutubePlus90s90s ytData = new YoutubePlus90s90s();
            var url = "http://stream-service.loverad.io/v3/90s90s";//Data used by www.90s90s.de
            var root = ytData._download_serialized_json_data<Rootobject>(url);

            var RadioChecked = Station.Children.OfType<WindowsControl.RadioButton>()
            .FirstOrDefault(r => r.IsChecked.HasValue && r.IsChecked.Value);

            //Retrieve data corresponding to the selected station
            SharedVariables.StationChecked = RadioChecked.Name;
            switch (SharedVariables.StationChecked)
            {
                case "Hits":
                        artistName = root.Hits.artist_name.ToString();
                        songTitle = root.Hits.song_title.ToString();
                        if (filler == true)
                        {
                            artistName = root.Dance.artist_name.ToString();
                            songTitle = root.Dance.song_title.ToString();
                        }

                    break;

                case "Beat":
                    artistName = root.Beat.artist_name.ToString();
                    songTitle = root.Beat.song_title.ToString();
                    if (filler == true)
                    {
                        artistName = root.Hits.artist_name.ToString();
                        songTitle = root.Hits.song_title.ToString();
                    }
                    break;

                case "Dance":
                    artistName = root.Dance.artist_name.ToString();
                    songTitle = root.Dance.song_title.ToString();
                    if (filler == true)
                    {
                        artistName = root.Hits.artist_name.ToString();
                        songTitle = root.Hits.song_title.ToString();
                    }
                    break;

                case "Boys":
                    artistName = root.Boygroups.artist_name.ToString();
                    songTitle = root.Boygroups.song_title.ToString();
                    if (filler == true)
                    {
                        artistName = root.Hits.artist_name.ToString();
                        songTitle = root.Hits.song_title.ToString();
                    }
                    break;

            }
            filler = false;
            SharedVariables.VideoTitle = artistName + " - " + songTitle;

            // http://stream-service.loverad.io/v3/90s90s returnes inconsistent data. To avoid duplicate entries in VideoList, check the last 5 entries do not contain newly querried data
            var VLF = VideoList.Take(5).ToList();
            if (!VLF.Any(s => s.Vid == SharedVariables.VideoTitle))
            {
                VideoList.Insert(0, new VideoListData() { Vid = SharedVariables.VideoTitle, SeqPosition=SharedVariables.SeqPosition });
                ListOfVideos.ItemsSource = VideoList;

                //Sequence position of an item within the observablecollection (in effect, substituting Dictionary)
                SharedVariables.SeqPosition = SharedVariables.SeqPosition + 1;
 
                //Check if the video exists in the download folder, change its button label to "Play" and select it (which will trigger play)
                if (GetDirectoryList().Any(f => f.Contains(SharedVariables.VideoTitle)))
                {
                    var item = VideoList.FirstOrDefault(i => i.Vid == SharedVariables.VideoTitle);
                    item.Percent = "Play";
                    if (SharedVariables.VideoPlayerIsPlaying == false)
                    {
                        await Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => PrePlayProcessing()));
                    }
                }
                else
                {
                    await ytData.GetVideoID(); //Download next video

                    //Remove an entrie from the observablecollection if its counterpart file has been removed from the download folder
                    foreach (string s in GetDirectoryList())
                    {
                        foreach (VideoListData listItem in VideoList.ToList())
                        {
                            if (!GetDirectoryList().Any(f => f.Contains(listItem.Vid)))
                            {
                                VideoList.Remove(listItem);
                            }
                        }
                    }
                }
            }
        }

        public void PrePlayProcessing ()
        {
            if (SharedVariables.CurrentlyPlayed == null)
            {
                posIndex = 0;
            }

            SharedVariables.IndexOfPlayed = VideoList.IndexOf(VideoList.Where(i => i.SeqPosition == posIndex).FirstOrDefault());

            //Finding next available item in the observablecollection due to some items may be removed (folder size reduced or hit the limit)
            while (VideoList.IndexOf(VideoList.Where(i => i.SeqPosition == posIndex).FirstOrDefault()) ==-1)
            {
                posIndex = posIndex + 1;
                SharedVariables.IndexOfPlayed = VideoList.IndexOf(VideoList.Where(i => i.SeqPosition == posIndex).FirstOrDefault());
            }

                SharedVariables.CurrentlyPlayed = VideoList[SharedVariables.IndexOfPlayed].Vid.ToString();
                WindowsControl.Button objButton = (WindowsControl.Button)ActiveButton(SharedVariables.IndexOfPlayed);
                if (SharedVariables.VideoPlayerIsPlaying == false)
                {
                    Button_Click(objButton, null);
                }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            WindowsControl.Button button = sender as WindowsControl.Button;

            //Button click beforedownload started
            try
            {
                ButtonState = button.Content.ToString();
            }
            catch { }

            VideoListData selectedItem = ListOfVideos.SelectedItem as VideoListData;

            //"Actual" button click
            if (selectedItem != null && e != null)
            {
                SharedVariables.CurrentlyPlayed = selectedItem.Vid.ToString();
            }

            //Render all other button's content to Play (except in progress) whenever a new button pressed
            if (ButtonState == "Play")
            {
                foreach (WindowsControl.Button controlButton in FindVisualChildren<WindowsControl.Button>(ListOfVideos))
                {
                    try
                    {
                        if (controlButton.Content.ToString() == "Pause" || controlButton.Content.ToString() == "Resume")
                        {
                            controlButton.Content = "Play";
                            controlButton.FontWeight = FontWeights.Normal;
                        }
                    }
                    catch { }
                }
            }
            switch (ButtonState)
            {
                case "Play":
                    SharedVariables.VideoPlayerIsPlaying = true;
                    button.Content = "Pause";
                    button.FontWeight = FontWeights.Bold;
                    
                    var FileList = Directory.GetFiles(SharedVariables.path).ToList();
                    if (GetDirectoryList().Any(f => f.Contains(SharedVariables.CurrentlyPlayed)))
                    {
                        SharedVariables.FullFileName = FileList.FirstOrDefault(s => s.Contains(SharedVariables.CurrentlyPlayed));
                        VideoPlayer.Source = new Uri(SharedVariables.FullFileName);
                        VideoPlayer.Play();
                        posIndex = VideoList.FirstOrDefault(i => i.Vid == SharedVariables.CurrentlyPlayed).SeqPosition;
                        Title = SharedVariables.CurrentlyPlayed;
                    }
                    break;

                case "Pause":
                    button.Content = "Resume";
                    VideoPlayer.Pause();
                    break;

                case "Resume":
                    button.Content = "Pause";
                    VideoPlayer.Play();
                    break;
            }
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj)
         where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //To be implemented
        }

        private void ListOfVideosLayout_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            LeftSide.Opacity = 0.6;
        }

        private void ListOfVideosLayout_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (VidLooseFocus == false || SharedVariables.LeftSide_Pin == "Checked")
            {
                LeftSide.Opacity = 0.6;
            }
            else
            {
                LeftSide.Opacity = 0.0;
            }
            VidLooseFocus = true;
        }

        private void Controls_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
             Controls.Opacity = 0.6;
        }

        private void Controls_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {

            if (ControlLooseFocus == false || (SharedVariables.FolderInfo_Pin == "Checked"))
            {
                Controls.Opacity = 0.6;
            }
            else
            {
                Controls.Opacity = 0.0;
            }
            ControlLooseFocus = true;
        }

        private void SetFolderSize_Click(object sender, RoutedEventArgs e)
        {
            SetFolderSize_InputBox.Visibility = Visibility.Visible;
            Dispatcher.BeginInvoke(DispatcherPriority.Input,
            new Action(delegate () {
            FolderSizeInputBox.Focus();         
            Keyboard.Focus(FolderSizeInputBox);}));
        }

        private void YesBtn_Click(object sender, RoutedEventArgs e)
        {
            SetFolderSize_InputBox.Visibility = Visibility.Collapsed;
            SharedVariables.SetFolderSize = Convert.ToInt64(FolderSizeInputBox.Text);
            SharedVariables.CurrentFolderSize = new DirectoryInfo(SharedVariables.path).GetFiles("*.*", SearchOption.TopDirectoryOnly).Sum(file => file.Length);
            string folderSize = SharedVariables.CurrentFolderSize.ToSize(MyExtension.SizeUnits.MB);
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => FolderInfo.Content = folderSize + " MB" + "/" + SharedVariables.SetFolderSize + " MB"));
            Properties.Settings.Default.FolderSizeSettings = SharedVariables.SetFolderSize;
        }

        private void NoBtn_Click(object sender, RoutedEventArgs e)
        {
            SetFolderSize_InputBox.Visibility = Visibility.Collapsed;
        }

        private void SetFolderPath_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Set Video Folder";
            fbd.ShowDialog();
            if (fbd.SelectedPath != "")
            {
                SharedVariables.path = fbd.SelectedPath;
            }
            Properties.Settings.Default.FolderPathSettings = SharedVariables.path;
        }

        private void Radio_Checked(object sender, RoutedEventArgs e)
        {
            var RadioChecked = Station.Children.OfType<WindowsControl.RadioButton>()
            .FirstOrDefault(r => r.IsChecked.HasValue && r.IsChecked.Value);
            
            SharedVariables.StationChecked = RadioChecked.Name;
            Properties.Settings.Default.StationChecked = SharedVariables.StationChecked;
        }

        private void LeftSide_Pin_Checked(object sender, RoutedEventArgs e)
        {
            SharedVariables.LeftSide_Pin = "Checked";
            Properties.Settings.Default.LeftSide_Pin = SharedVariables.LeftSide_Pin;
        }

        private void LeftSide_Pin_Unchecked(object sender, RoutedEventArgs e)
        {
            SharedVariables.LeftSide_Pin = "";
            Properties.Settings.Default.LeftSide_Pin = SharedVariables.LeftSide_Pin;
        }

        private void FolderInfo_Pin_Checked(object sender, RoutedEventArgs e)
        {
            SharedVariables.FolderInfo_Pin = "Checked";
            Properties.Settings.Default.FolderInfo_Pin = SharedVariables.FolderInfo_Pin;
        }

        private void FolderInfo_Pin_Unchecked(object sender, RoutedEventArgs e)
        {
            SharedVariables.FolderInfo_Pin = "";
            Properties.Settings.Default.FolderInfo_Pin = SharedVariables.FolderInfo_Pin;
        }

        private void MuteBtn_Click(object sender, RoutedEventArgs e)
        {
            VideoPlayer.IsMuted = !VideoPlayer.IsMuted;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (VideoPlayer != null)
            {
                VideoPlayer.Volume = VolumeSlider.Value;
                Properties.Settings.Default.VolumeValue = VolumeSlider.Value;
            }
        }

        private void VideoLayout_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MouseHideTimer.Stop();
            Mouse.OverrideCursor = null; //Show cursor
            MouseHideTimer.Start();
        }

        //Window fullscreen on mouse doubleclick
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount==2)
            {
                switch (WindowState)
                {
                    case (WindowState.Maximized):
                        {
                            ResizeMode = ResizeMode.CanResize;
                            WindowStyle = WindowStyle.SingleBorderWindow;
                            WindowState = WindowState.Normal;
                            break;
                        }
                    case (WindowState.Normal):
                        {
                            ResizeMode = ResizeMode.NoResize;
                            WindowStyle = WindowStyle.None;
                            WindowState = WindowState.Maximized;
                            break;
                        }
                }
            }
        }

        //Window fullscreen when maximize button pressed
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Maximized && !_inStateChange)
            {
                _inStateChange = true;
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                ResizeMode = ResizeMode.NoResize;
                _inStateChange = false;
            }
            base.OnStateChanged(e);
        }

        public void GetCurrentFolderSize()
        {
            SharedVariables.CurrentFolderSize = new DirectoryInfo(SharedVariables.path).GetFiles("*.*", SearchOption.TopDirectoryOnly).Sum(file => file.Length);
            string folderSize = SharedVariables.CurrentFolderSize.ToSize(MyExtension.SizeUnits.MB);
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => FolderInfo.Content = folderSize + " MB" + "/" + SharedVariables.SetFolderSize + " MB"));
        }

        public List<string> GetDirectoryList ()
        {
            var filesWithoutExtensionList = Directory.GetFiles(SharedVariables.path).Select(f => Path.GetFileName(f)).ToList();
            return filesWithoutExtensionList;
        }

        private void VidName_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            VidLooseFocus = false;
        }

        private void FolderInfo_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ControlLooseFocus = false;
        }

        //On button click item select
        private void Select_Current_Item(object sender, KeyboardFocusChangedEventArgs e)
        {
            ListBoxItem item = (ListBoxItem)sender;
            item.IsSelected = true;
        }

        //Get the button control corresponding to a ListBox item
        public object ActiveButton(int itemAt)
        {
            ListBoxItem myListBoxItem =
            (ListBoxItem)(ListOfVideos.ItemContainerGenerator.ContainerFromItem(ListOfVideos.Items.GetItemAt(itemAt)));
            ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(myListBoxItem);
            DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
            WindowsControl.Button activeButton = (WindowsControl.Button)myDataTemplate.FindName("ButtonTag", myContentPresenter);
            return activeButton;
        }

        public class VideoListData : INotifyPropertyChanged
        {
            public string Vid { get; set; }
            public int SeqPosition { get; set; }
            private string percent;
            public string Percent
            {
                get { return percent; }
                set
                {
                    percent = value;
                    OnPropertyChanged("Percent");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            private void OnPropertyChanged([CallerMemberName] String propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

        }


    }

    public static class MouseExt
    {
        public static void SafeOverrideCursor(System.Windows.Input.Cursor cursor)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Mouse.OverrideCursor = cursor;
            }));
        }
    }

    public static class MyExtension
    {
        public enum SizeUnits
        {
            Byte, KB, MB, GB, TB, PB, EB, ZB, YB
        }

        public static string ToSize(this Int64 value, SizeUnits unit)
        {
            return (value / Math.Pow(1024, (Int64)unit)).ToString("0.00");
        }
    }
}