using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Mosaic.Base;

namespace Mosaic.Core.Controls
{
    /// <summary>
    /// Interaction logic for MosaicFriendWidgetControl.xaml
    /// </summary>
    public partial class MosaicFriendWidgetControl : UserControl
    {
        private DispatcherTimer tileAnimTimer;
        private Random random;
        private readonly int seed;

        public MosaicFriendWidgetControl(int seed)
        {
            this.seed = seed;
            InitializeComponent();
        }

        private void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            random = new Random(Environment.TickCount + seed);
            tileAnimTimer = new DispatcherTimer();
            tileAnimTimer.Interval = TimeSpan.FromSeconds(random.Next(14, 26));
            tileAnimTimer.Tick += TileAnimTimerTick;
            if (E.AnimationEnabled)
                tileAnimTimer.Start();
        }

        void TileAnimTimerTick(object sender, EventArgs e)
        {
            var s = (Storyboard)Resources["TileAnim"];
            s.Begin();
        }

        private void UserControlUnloaded(object sender, RoutedEventArgs e)
        {
            tileAnimTimer.Stop();
        }
    }
}
