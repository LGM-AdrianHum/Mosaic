using Mosaic.Base;

namespace Today
{
    public class Settings : XmlSerializable
    {
        public Settings()
        {
            RefreshInterval = 300; //seconds (= 5minutes)
            FeedURL = "https://www.google.com/calendar/feeds/default/private/full";
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public int RefreshInterval { get; set; }
        public int LastMsgCount { get; set; }
        public string FeedURL { get; set; }
    }
}
