using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Win32;
using Mosaic.Base;
using Mosaic.Core;
using Mosaic.Windows;
using TheCodeKing.Net.Messaging;

namespace Mosaic
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static ScreenManager WindowManager;
        public static WidgetManager WidgetManager;
        public static Settings Settings;
        public static Options OptionsWindow;

        private IXDListener listener; //listens messages from extern apps

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            if (!Directory.Exists(E.Root + "\\Thumbnails"))
                Directory.CreateDirectory(E.Root + "\\Thumbnails");
            if (!Directory.Exists(E.Root + "\\Cache"))
                Directory.CreateDirectory(E.Root + "\\Cache");

            StartupUri = new Uri("Windows/MainWindow.xaml", UriKind.Relative);

            Settings = (Settings)XmlSerializable.Load(typeof(Settings), E.Root + "\\Mosaic.config") ?? new Settings();
            //System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(Settings.Language);
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(Settings.Language);
            E.Language = Settings.Language;
            E.AnimationEnabled = Settings.AnimationEnabled;

            if (Settings.UseSoftwareRendering)
                RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            WindowManager = new ScreenManager();
            WidgetManager = new WidgetManager();
            WidgetManager.FindWidgets();

            if (Settings.LoadedWidgets.Count == 0)
            {
                Settings.LoadedWidgets.Add(new LoadedWidget() {Name = "Clock", Column = 1, Row = 1});
                Settings.LoadedWidgets.Add(new LoadedWidget() { Name = "Store", Column = 1, Row = 2 });
            }

            listener = XDListener.CreateListener(XDTransportMode.WindowsMessaging);
            listener.RegisterChannel("Mosaic.Main");
            listener.RegisterChannel("Mosaic.Widgets");
            listener.MessageReceived += ListenerMessageReceived;
        }

        void ListenerMessageReceived(object sender, XDMessageEventArgs e)
        {
            List<string> parameters = null;
            string command;
            if (e.DataGram.Message.Contains(":"))
            {
                string[] temp = e.DataGram.Message.Split(new[] { ':' }, 2);
                command = temp[0];
                parameters = temp[1].Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            else
            {
                command = e.DataGram.Message;
            }

            if (e.DataGram.Channel == "Mosaic.Widgets")
            {
                var widget = WidgetManager.GetWidgetByName(command);
                if (widget != null && parameters != null && parameters.Count > 0)
                    widget.WidgetComponent.Notify(parameters[0]);
            }
            else
            {
                switch (command)
                {
                    case "Hide":
                        MainWindow.Hide();
                        break;
                    case "Install":
                        if (parameters == null || parameters.Count < 2)
                            return;
                        WidgetManager.InstallWidget(parameters[0], parameters[1]);
                        break;
                    case "InstallZip":
                        if (parameters == null || parameters.Count < 2)
                            return;
                        WidgetManager.InstallWidgetFromZip(parameters[0], parameters[1]);
                        Share.SendMessage("Mosaic.Widgets", "Store:Installed");
                        break;
                    case "Load":
                        if (parameters == null || parameters.Count == 0)
                            return;
                        if (WidgetManager.HasWidget(parameters[0]) && !WidgetManager.IsWidgetLoaded(parameters[0]))
                            WidgetManager.LoadWidget(parameters[0]);
                        break;
                    case "LoadExt":
                        if (parameters == null || parameters.Count == 0)
                            return;
                        WidgetManager.LoadExternalWidget(parameters[0]);
                        break;
                }
            }
        }

        public static void ShowOptions()
        {
            if (OptionsWindow != null && OptionsWindow.IsVisible)
            {
                OptionsWindow.Activate();
                return;
            }

            OptionsWindow = new Options();
            OptionsWindow.Topmost = true;
            OptionsWindow.UpdateSettings += OptionsWindowUpdateSettings;

            if (App.Settings.Language == "he-IL" || App.Settings.Language == "ar-SA")
            {
                OptionsWindow.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            }
            else
            {
                OptionsWindow.FlowDirection = System.Windows.FlowDirection.LeftToRight;
            }

            OptionsWindow.ShowDialog();
        }

        static void OptionsWindowUpdateSettings(object sender, EventArgs e)
        {
            OptionsWindow.UpdateSettings -= OptionsWindowUpdateSettings;
            ((MainWindow)App.Current.MainWindow).UpdateSettings();
        }

        private void ApplicationDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("An error occured. See log for detailed information");
            Logger.Error("An error occured.\n" + e.Exception);
        }
    }
}
