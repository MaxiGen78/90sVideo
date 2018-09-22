using System.Text.RegularExpressions;

namespace WPF90sVideo
{
    public static class SharedVariables
    {
        private static string _videoTitle;
        public static string VideoTitle
        {
            get
            {
                //Remove all extra symbols that can prevent file to be saved in the download folder
                string x = Regex.Replace(_videoTitle, @"[^A-Za-z0-9|\(|\)|\-|\s|_|.]", " ");
                return Regex.Replace(x, @"\s+", " ");
            }
            set
            {
                _videoTitle = value;
            }
        }

        public static string _searchResID { get; set; }
        public static string FileName { get; set; }
        public static string FileNameToDelete { get; set; }
        public static long FileSizeToDelete { get; set; }
        public static long CurrentFolderSize { get; set; }
        public static long SetFolderSize { get; set; }
        public static string path { get; set; }
        public static string TempPath { get; set; }
        public static string fileNameTemp { get; set; }
        public static string FullFileName { get; set; }
        public static string StationChecked { get; set; }
        public static string LeftSide_Pin { get; set; }
        public static string FolderInfo_Pin { get; set; }
        public static string CurrentSelected { get; set; }
        public static string CurrentlyPlayed { get; set; }
        public static string NextToPlay{ get; set; }
        public static int IndexOfPlayed;
        public static int IndexOfNextToPlay;
        public static object CurrentPercent { get; set; }
        public static bool VideoPlayerIsPlaying=false;
        public static bool VideoPlayerOnPause = false;
        public static int SeqPosition { get; set; }
    }
}
