using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Mosaic.Base;
using Mosaic.Core.Controls;
using Social.Base;

namespace Mosaic.Core
{
    /// <summary>
    /// Interaction logic for MosaicFriendWidgetHub.xaml
    /// </summary>
    public partial class MosaicFriendWidgetHub : Window
    {
        private string id;
        private SocialProvider socialProvider;

        public MosaicFriendWidgetHub(string id)
        {
            this.id = id;


            InitializeComponent();
        }

        private void WindowSourceInitialized(object sender, EventArgs e)
        {
            this.Left = 0;
            this.Top = 0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;

            socialProvider = new SocialProvider();
            ThreadStart threadStarter = delegate
                                            {
                                                var info = socialProvider.GetFriendInfoById(id);
                                                this.Dispatcher.Invoke((Action)delegate
                                                                                    {
                                                                                        if (info != null)
                                                                                        {
                                                                                            Username.Text = info.Name;
                                                                                            Gender.Text = info.Gender;
                                                                                            Birthday.Text = info.Birthday;
                                                                                            Hometown.Text = info.Hometown;
                                                                                            Relationship.Text = info.Relationship;
                                                                                        }
                                                                                    });

                                                this.Dispatcher.Invoke((Action)delegate
                                                {
                                                    var entries = socialProvider.GetFriendStream(id);
                                                    foreach (var entry in entries)
                                                    {
                                                        var item = new WallItem();
                                                        item.WallEntry = entry;
                                                        item.Order = FeedsPanel.Children.Count;
                                                        FeedsPanel.Children.Add(item);
                                                    }
                                                });
                                            };
            var thread = new Thread(threadStarter);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            if (File.Exists(E.Root + "\\Cache\\" + id + ".png"))
            {
                UserPic.Source = new BitmapImage(new Uri(E.Root + "\\Cache\\" + id + ".png"));
            }
        }

        private void WindowKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void BackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
