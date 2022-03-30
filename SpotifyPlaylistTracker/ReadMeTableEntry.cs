using System;
using System.Collections.Generic;
using System.Text;

namespace SpotifyPlaylistTracker
{
    public class ReadMeTableEntry
    {
        public string PlaylistName { get; set; }
        public string PlaylistId { get; set; }
        public int Songs { get; set; }
        public string PlaylistLength { get; set; }
        public DateTime LastChanged { get; set; }
    }
}
