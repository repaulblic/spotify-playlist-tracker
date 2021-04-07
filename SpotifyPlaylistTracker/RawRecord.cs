using System;
using System.Collections.Generic;
using System.Text;

namespace SpotifyPlaylistTracker
{
    public class RawRecord
    {
        public string playlistName { get; set; }
        public string playlistID { get; set; }
        public string versionID { get; set; }
        public string description { get; set; }
        public List<string> tracks { get; set; }

    }
}
