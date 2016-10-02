using Mosaic.Base;

namespace Hotmail
{
    public class Settings : XmlSerializable
    {
        public Settings()
        {
            RefreshInterval = 60; //seconds
            PopServer = "pop3.live.com";
            MailHttp = "www.hotmail.com";
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public int RefreshInterval { get; set; }
        public int LastMsgCount { get; set; }
        public string PopServer { get; set; }
        public string MailHttp { get; set; }
    }
}
