using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheCodeKing.Net.Messaging;
using TheCodeKing.Net.Messaging.Concrete.MailSlot;

namespace Mosaic.Base
{
    public static class Share
    {
        public static Dictionary<string, string> SharedStrings;
        private static IXDBroadcast broadcast;

        static Share()
        {
            SharedStrings = new Dictionary<string, string>();
            broadcast = XDBroadcast.CreateBroadcast(XDTransportMode.WindowsMessaging, false);
        }

        public static void SendMessage(string channel, string message)
        {
            broadcast.SendToChannel(channel, message);
        }
    }
}
