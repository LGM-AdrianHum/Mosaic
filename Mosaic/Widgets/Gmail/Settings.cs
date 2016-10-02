using Mosaic.Base;

namespace Gmail
{
    public class Settings : XmlSerializable
    {
        public Settings()
        {
            RefreshInterval = 60; //seconds
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public int RefreshInterval { get; set; }

    }
}
