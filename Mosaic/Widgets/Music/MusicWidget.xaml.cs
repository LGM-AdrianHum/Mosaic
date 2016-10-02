using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Shell;
using Mosaic.Base;
using WMPLib;
using WmpRemoteLib;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Path = System.IO.Path;
using UserControl = System.Windows.Controls.UserControl;

namespace Music
{
    /// <summary>
    /// Interaction logic for MusicWidget.xaml
    /// </summary>
    public partial class MusicWidget : UserControl
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        private Form fakeForm; //used for wmpcontrol, doesn't work without attaching to any form
        private WindowsMediaPlayer mediaPlayer;
        private List<string> albumArts;
        private Random r;
        private DispatcherTimer updateTimer; //updates album art
        private DispatcherTimer refreshTimer; //checks state of mediaPlayer
        private int lastIndex;
        private WMPPlayState lastState;
        private string lastMediaSource;

        public bool IsMediaLoaded
        {
            get
            {
                if (mediaPlayer == null)
                    return false;
                return (mediaPlayer.currentMedia != null);
            }
        }

        public MusicWidget()
        {
            InitializeComponent();
        }

        public void Load()
        {
            Share.SharedStrings.Add("Music_CurrentMediaArt", null);
            Share.SharedStrings.Add("Music_CurrentMediaTitle", null);
            Share.SharedStrings.Add("Music_CurrentMediaArtist", null);
            Share.SharedStrings.Add("Music_PlaybackState", null);

            albumArts = new List<string>();
            r = new Random(Environment.TickCount);

            if (ShellLibrary.IsPlatformSupported)
            {
                var lib = ShellLibrary.Load(KnownFolders.MusicLibrary, true);
                foreach (var l in lib)
                {
                    foreach (var file in Directory.GetFiles(l.Path, "*.jpg", SearchOption.AllDirectories))
                    {
                        albumArts.Add(file);
                    }
                }
                lib.Dispose();
            }
            else
            {
                if (Directory.Exists(path))
                {
                    foreach (var f in Directory.GetFiles(path, "*.jpg", SearchOption.AllDirectories))
                    {
                        albumArts.Add(f);
                    }
                }
            }


            try
            {
                var rm = new RemotedWindowsMediaPlayer();
                fakeForm = new Form();
                fakeForm.Controls.Add(rm);
                fakeForm.FormBorderStyle = FormBorderStyle.None;
                fakeForm.Opacity = 0;
                fakeForm.ShowInTaskbar = false;
                fakeForm.Show();
                mediaPlayer = rm.GetOcx() as WindowsMediaPlayer;
            }
            catch (Exception ex)
            {
                logger.Error("Can't connect to wmp. " + ex);
            }

            if (mediaPlayer == null)
                return;

            if (mediaPlayer.currentMedia != null)
            {
                Artist.Text = mediaPlayer.currentMedia.getItemInfo("Artist");
                SongTitle.Text = mediaPlayer.currentMedia.getItemInfo("Title");

                Share.SharedStrings["Music_CurrentMediaTitle"] = mediaPlayer.currentMedia.getItemInfo("Title");
                Share.SharedStrings["Music_CurrentMediaArtist"] = mediaPlayer.currentMedia.getItemInfo("Artist");

                var s = (Storyboard)Resources["PopupOpenAnim"];
                s.Begin();

                if (GetPicture() != null)
                {
                    AlbumArt.Source = GetPicture();
                    Share.SharedStrings["Music_CurrentMediaArt"] = ((BitmapImage)AlbumArt.Source).UriSource.OriginalString;
                }
                else
                {
                    AlbumArt.Source = new BitmapImage(new Uri("Resources/zune_icon.png", UriKind.Relative));
                }

                if (mediaPlayer.playState == WMPPlayState.wmppsPlaying)
                {
                    PlayPause.IsChecked = true;
                    Share.SharedStrings["Music_PlaybackState"] = "playing";
                }

                lastMediaSource = mediaPlayer.currentMedia.sourceURL;
            }
            else
            {
                if (albumArts.Count > 0)
                    AlbumArt.Source = new BitmapImage(new Uri(albumArts[r.Next(albumArts.Count)]));
            }

            if (albumArts.Count > 0)
            {
                AlbumArt1.Source = new BitmapImage(new Uri(albumArts[r.Next(albumArts.Count)]));
                AlbumArt2.Source = new BitmapImage(new Uri(albumArts[r.Next(albumArts.Count)]));
                AlbumArt3.Source = new BitmapImage(new Uri(albumArts[r.Next(albumArts.Count)]));
                AlbumArt4.Source = new BitmapImage(new Uri(albumArts[r.Next(albumArts.Count)]));
            }

            lastState = mediaPlayer.playState;

            updateTimer = new DispatcherTimer();
            updateTimer.Interval = TimeSpan.FromSeconds(15);
            updateTimer.Tick += UpdateTimerTick;
            updateTimer.Start();

            refreshTimer = new DispatcherTimer();
            refreshTimer.Interval = TimeSpan.FromMilliseconds(500);
            refreshTimer.Tick += RefreshTimerTick;
            refreshTimer.Start();
        }

        void RefreshTimerTick(object sender, EventArgs e)
        {
            //check play state
            if (mediaPlayer.playState != lastState)
            {
                MediaPlayerPlayStateChanged();
                lastState = mediaPlayer.playState;
            }

            if (mediaPlayer.currentMedia != null && mediaPlayer.currentMedia.sourceURL != lastMediaSource)
            {
                MediaPlayerMediaChange();
                lastMediaSource = mediaPlayer.currentMedia.sourceURL;
            }
        }

        private ImageSource GetPicture()
        {
            string imagePath = Path.GetDirectoryName(mediaPlayer.currentMedia.sourceURL) + "\\AlbumArt_{" + GetCollectionID() + "}_Large.jpg";
            if (!File.Exists(imagePath))
            {
                imagePath = Path.GetDirectoryName(mediaPlayer.currentMedia.sourceURL) + "\\AlbumArt_{" + GetCollectionID() + "}_Small.jpg";
                if (!File.Exists(imagePath))
                {
                    imagePath = Path.GetDirectoryName(mediaPlayer.currentMedia.sourceURL) + "\\Folder.jpg";
                    if (!File.Exists(imagePath))
                    {
                        imagePath = Path.GetDirectoryName(mediaPlayer.currentMedia.sourceURL) + "\\Cover.jpg";
                        if (!File.Exists(imagePath))
                        {
                            imagePath = Path.GetDirectoryName(mediaPlayer.currentMedia.sourceURL) + "\\AlbumArtSmall.jpg";
                            if (!File.Exists(imagePath))
                                imagePath = string.Empty;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(imagePath))
                return new BitmapImage(new Uri(imagePath));
            else
                return null;
        }

        private Guid GetCollectionID()
        {
            if (string.IsNullOrEmpty(mediaPlayer.currentMedia.getItemInfo("WM/WMCollectionID")))
                return new Guid();
            return new Guid(mediaPlayer.currentMedia.getItemInfo("WM/WMCollectionID"));
        }

        void UpdateTimerTick(object sender, EventArgs e)
        {
            if (albumArts.Count > 0)
            {
                var s = (Storyboard)Resources["AlbumArtFadeOut"];
                lastIndex = r.Next(4);
                switch (lastIndex)
                {
                    case 0:
                        s.Begin(AlbumArt1);
                        break;
                    case 1:
                        s.Begin(AlbumArt2);
                        break;
                    case 2:
                        s.Begin(AlbumArt3);
                        break;
                    case 3:
                        s.Begin(AlbumArt4);
                        break;
                }
            }
        }

        void MediaPlayerPlayStateChanged()
        {
            if (mediaPlayer.playState == WMPPlayState.wmppsPlaying)
            {
                PlayPause.IsChecked = true;
                Share.SharedStrings["Music_PlaybackState"] = "playing";
                Share.SendMessage("Mosaic.Widgets", "Clock:Play");
            }
            else
            {
                PlayPause.IsChecked = false;
                Share.SharedStrings["Music_PlaybackState"] = "paused";
                Share.SendMessage("Mosaic.Widgets", "Clock:Pause");
            }
        }


        void MediaPlayerMediaChange()
        {
            if (mediaPlayer.currentMedia != null)
            {
                var s = (Storyboard)Resources["PopupOpenAnim"];
                s.Begin();
                if (GetPicture() != null)
                {
                    AlbumArt.Source = GetPicture();
                    Share.SharedStrings["Music_CurrentMediaArt"] = ((BitmapImage)AlbumArt.Source).UriSource.OriginalString;
                }
                else
                {
                    AlbumArt.Source = new BitmapImage(new Uri("Resources/zune_icon.png", UriKind.Relative));
                    Share.SharedStrings["Music_CurrentMediaArt"] = string.Empty;
                }
                Artist.Text = mediaPlayer.currentMedia.getItemInfo("Title");
                SongTitle.Text = mediaPlayer.currentMedia.getItemInfo("Artist");

                Share.SharedStrings["Music_CurrentMediaTitle"] = Artist.Text;
                Share.SharedStrings["Music_CurrentMediaArtist"] = SongTitle.Text;

                Share.SendMessage("Mosaic.Widgets", "Clock:MediaChanged");
            }
        }

        /*void MediaPlayerCurrentMediaChanged(object sender, RoutedPropertyChangedEventArgs<WmpMediaItem> e)
        {
            if (mediaPlayer.CurrentMedia != null)
            {
                var s = (Storyboard)Resources["PopupOpenAnim"];
                s.Begin();
                if (mediaPlayer.CurrentMedia.Picture != null)
                {
                    AlbumArt.Source = mediaPlayer.CurrentMedia.Picture;
                    Share.SharedObjects["Music_CurrentMediaArt"] = ((BitmapImage)AlbumArt.Source).UriSource.OriginalString;
                }
                else
                {
                    AlbumArt.Source = new BitmapImage(new Uri("Resources/zune_icon.png", UriKind.Relative));
                    Share.SharedObjects["Music_CurrentMediaArt"] = string.Empty;
                }
                Artist.Text = mediaPlayer.CurrentMedia.Artist;
                SongTitle.Text = mediaPlayer.CurrentMedia.Title;

                Share.SharedObjects["Music_CurrentMediaTitle"] = mediaPlayer.CurrentMedia.Title;
                Share.SharedObjects["Music_CurrentMediaArtist"] = mediaPlayer.CurrentMedia.Artist;
            }
        }*/

        public void Unload()
        {
            updateTimer.Stop();
            refreshTimer.Stop();

            Share.SharedStrings.Remove("Music_CurrentMediaTitle");
            Share.SharedStrings.Remove("Music_CurrentMediaArtist");
            Share.SharedStrings.Remove("Music_CurrentMediaArt");
        }

        private void PopupOpenAnimCompleted(object sender, EventArgs e)
        {
            var s = (Storyboard)Resources["PopupCloseAnim"];
            s.Begin();
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Windows Media Player\\wmplayer.exe";
            if (File.Exists(path))
            {
                var info = new ProcessStartInfo();
                info.WindowStyle = ProcessWindowStyle.Maximized;
                info.FileName = path;
                Process.Start(info);
            }
        }

        private void UserControlMouseEnter(object sender, MouseEventArgs e)
        {
            if (mediaPlayer.currentMedia != null)
            {
                var s = (Storyboard)Resources["ShowControlsPanelAnim"];
                s.Begin();
            }
        }

        private void UserControlMouseLeave(object sender, MouseEventArgs e)
        {
            if (mediaPlayer.currentMedia != null)
            {
                var s = (Storyboard)Resources["HideControlsPanelAnim"];
                s.Begin();
            }
        }

        private void PrevClick(object sender, RoutedEventArgs e)
        {
            //if (mediaPlayer.currentMedia != null)
            //{
            //    mediaPlayer.controls.previous();
            //}

            PreviousTrack();
        }

        private void NextClick(object sender, RoutedEventArgs e)
        {
            //if (mediaPlayer.currentMedia != null)
            //{
            //    mediaPlayer.controls.next();
            //}

            NextTrack();
        }

        private void PlayPauseClick(object sender, RoutedEventArgs e)
        {
            /*if (mediaPlayer.currentMedia != null)
            {
                if (mediaPlayer.playState == WMPPlayState.wmppsPlaying)
                    mediaPlayer.controls.pause();
                else
                    mediaPlayer.controls.play();
            }*/
            PlayPauseTrack();
        }

        public void NextTrack()
        {
            WinAPI.SendKeyPress(WinAPI.MEDIA_NEXT_TRACK);
        }

        public void PlayPauseTrack()
        {
            WinAPI.SendKeyPress(WinAPI.MEDIA_PLAY_PAUSE);
        }

        public void PreviousTrack()
        {
            WinAPI.SendKeyPress(WinAPI.MEDIA_NEXT_TRACK);
        }

        private void StoryboardCompleted(object sender, EventArgs e)
        {
            var s = (Storyboard)Resources["AlbumArtFadeIn"];
            switch (lastIndex)
            {
                case 0:
                    AlbumArt1.Source = new BitmapImage(new Uri(albumArts[r.Next(albumArts.Count)]));
                    s.Begin(AlbumArt1);
                    break;
                case 1:
                    AlbumArt2.Source = new BitmapImage(new Uri(albumArts[r.Next(albumArts.Count)]));
                    s.Begin(AlbumArt2);
                    break;
                case 2:
                    AlbumArt3.Source = new BitmapImage(new Uri(albumArts[r.Next(albumArts.Count)]));
                    s.Begin(AlbumArt3);
                    break;
                case 3:
                    AlbumArt4.Source = new BitmapImage(new Uri(albumArts[r.Next(albumArts.Count)]));
                    s.Begin(AlbumArt4);
                    break;
            }
        }
    }
}
