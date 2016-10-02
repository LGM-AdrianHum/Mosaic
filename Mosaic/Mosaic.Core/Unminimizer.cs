using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using Mosaic.Base;

namespace Mosaic.Core
{
    //Prevents windows from minimizing when user clicks show desktop button
    public class Unminimizer
    {
        private DispatcherTimer timer;
        private IntPtr handle;

        public void Initialize(IntPtr hwnd)
        {
            handle = hwnd;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += TimerTick;
            timer.Start();
        }

        void TimerTick(object sender, EventArgs e)
        {
            var className = new StringBuilder(100);
            var fwHandle = WinAPI.GetForegroundWindow();
            WinAPI.GetClassName(fwHandle, className, className.Capacity);
            if (className.ToString() == "WorkerW")
            {
                WinAPI.SetWindowPos(fwHandle, new IntPtr(1), 0, 0, 0, 0,
                                    WinAPI.SWP_NOMOVE | WinAPI.SWP_NOSIZE | WinAPI.SWP_NOACTIVATE);
            }
        }
    }
}
