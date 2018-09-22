using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;
using System.Windows;

namespace WPF90sVideo
{

    public class DownloadVideo
    {
        string dirFileName = SharedVariables.VideoTitle + " (" + SharedVariables._searchResID + ")";
        string LocalVideoTitle = SharedVariables.VideoTitle;

        Window mainWindow = Application.Current.Windows.OfType<Window>().Single();

        private static double _progress;
        public static double Progress
        {
            get { return _progress; }
            set
            {
                if (value != _progress)
                {
                    _progress = value;
                }
            }
        }

        public async Task<string> GetData()
        {
            var extensionsDelete = new[] { ".webm", ".mp4" };
            Progress = 0;
            var client = new YoutubeClient();
            var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(SharedVariables._searchResID);
            var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();
            var ext = streamInfo.Container.GetFileExtension();
            SharedVariables.FileName = Path.Combine(SharedVariables.path, $"{dirFileName}.{ext}");
            long FileSize = streamInfo.Size;
            SharedVariables.FileSizeToDelete = FileSize;
            long CurrentFolderSize = new DirectoryInfo(SharedVariables.path).GetFiles("*.*", SearchOption.TopDirectoryOnly).Sum(file => file.Length);

            //delete oldest file in the directory
            while ((SharedVariables.SetFolderSize * 1024 * 1024) < (CurrentFolderSize + FileSize))
            {
                var fileInfo = new DirectoryInfo(SharedVariables.path).GetFiles().Where(fi => extensionsDelete.Contains(fi.Extension));
                try
                {
                    fileInfo.OrderBy(fi => fi.CreationTime).First().Delete();
                }
                catch
                {
                     //if the file in use (currently playing), delete a next one
                    fileInfo.OrderBy(fi => fi.CreationTime).Skip(1).First().Delete();
                }

                CurrentFolderSize = new DirectoryInfo(SharedVariables.path).GetFiles("*.*", SearchOption.TopDirectoryOnly).Sum(file => file.Length);
            }
            var progressHandler = new Progress<double>(p => Progress = p);
            progressHandler.ProgressChanged += ProgressHandler_ProgressChanged;
            await client.DownloadMediaStreamAsync(streamInfo, SharedVariables.FileName, progressHandler);
            return SharedVariables.FileName;
        }

        private void ProgressHandler_ProgressChanged(object sender, double e)
        {
            var item = mainWindow.VideoList.FirstOrDefault(i => i.Vid == LocalVideoTitle);
            item.Percent = $" [{Progress:P0}]";

            if (SharedVariables.CurrentlyPlayed==null)
            {
                mainWindow.Title= SharedVariables.VideoTitle +" "+ item.Percent;
            }
            
            if (Progress == 1)
            {
                item.Percent = "Play";
            }

            //Play first entrie if no video is playing
            if (Progress == 1 && SharedVariables.VideoPlayerIsPlaying == false)
            {
                mainWindow.PrePlayProcessing();
            }
        }
    }
}

