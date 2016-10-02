using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace People
{
    /// <summary>
    /// Interaction logic for UserPicControl.xaml
    /// </summary>
    public partial class UserPicControl : UserControl
    {
        private string newPic;
        private Random r;
        private DispatcherTimer updateTimer;

        public UserPicControl()
        {
            InitializeComponent();

            r = new Random(Environment.TickCount);

            updateTimer = new DispatcherTimer();
            updateTimer.Interval = TimeSpan.FromMilliseconds(r.Next(1500, 1800));
            updateTimer.Tick += UpdateTimerTick;
        }

        void UpdateTimerTick(object sender, EventArgs e)
        {
            updateTimer.Stop();
            if (PeopleWidget.Files.Length > 0)
            {
                var file = PeopleWidget.Files[r.Next(0, PeopleWidget.Files.Count())];
                UpdateMode(r.Next(0, 3), file);
            }
            else
                UpdateMode(r.Next(0, 2), null);
        }

        private int mode;
        public int Mode
        {
            get { return mode; }
            set
            {
                mode = value;
                switch (mode)
                {
                    case 0:
                        this.Opacity = 0;
                        UserPic.Opacity = 0;
                        break;
                    case 1:
                        this.Opacity = 1;
                        UserPic.Opacity = 0;
                        break;
                    case 2:
                        this.Opacity = 1;
                        UserPic.Opacity = 1;
                        break;
                    default:
                        this.Opacity = 1;
                        UserPic.Opacity = 1;
                        break;
                }
            }
        }

        public string Source
        {
            get { return ((BitmapImage)UserPic.Source).UriSource.OriginalString; }
            set
            {
                try
                {
                    var bi = new BitmapImage();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.BeginInit();
                    bi.UriSource = new Uri(value);
                    bi.EndInit();
                    UserPic.Source = bi;
                }
                catch { }

            }
        }

        private int order = 0;
        public int Order
        {
            get { return order; }
            set
            {
                order = value;

                ((Storyboard)Resources["Mode0Anim"]).BeginTime = TimeSpan.FromMilliseconds(100 + 50 * value);
                ((Storyboard)Resources["Mode1Anim"]).BeginTime = TimeSpan.FromMilliseconds(100 + 50 * value);
                ((Storyboard)Resources["Mode2Anim"]).BeginTime = TimeSpan.FromMilliseconds(100 + 50 * value);
            }
        }

        public void UpdateMode(int newMode, string pic)
        {
            if (newMode == Mode || (newMode >= 2 && Mode >= 2))
                return;
            mode = newMode;
            newPic = pic;
            Storyboard s = null;
            switch (newMode)
            {
                case 0:
                    s = (Storyboard)Resources["Mode0Anim"];
                    break;
                case 1:
                    s = (Storyboard)Resources["Mode1Anim"];
                    break;
                case 2:
                    if (newPic != null)
                        Source = newPic;
                    s = (Storyboard)Resources["Mode2Anim"];
                    break;
                default: 
                    if (newPic != null)
                        Source = newPic;
                    s = (Storyboard)Resources["Mode2Anim"];
                    break;
            }
            s.Begin();
        }

        private void StoryboardCompleted(object sender, EventArgs e)
        {
            this.Opacity = 0;
            UserPic.Opacity = 0;
            if (newPic != null)
                Source = newPic;
            if (RequiresUpdate())
                updateTimer.Start();

        }

        private void StoryboardCompleted1(object sender, EventArgs e)
        {
            this.Opacity = 1;
            UserPic.Opacity = 0;
            if (newPic != null)
                Source = newPic;
            if (RequiresUpdate())
                updateTimer.Start();
        }

        private void StoryboardCompleted2(object sender, EventArgs e)
        {
            this.Opacity = 1;
            UserPic.Opacity = 1;
            if (RequiresUpdate())
                updateTimer.Start();
        }

        private bool RequiresUpdate()
        {
            return r.Next(0, 10) < 3; //20% chance to update
        }
    }
}
