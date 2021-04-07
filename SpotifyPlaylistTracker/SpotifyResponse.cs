using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpotifyPlaylistTracker
{
    public class ExternalUrls
    {
        public string spotify { get; set; }
    }

    public class Owner
    {
        public string display_name { get; set; }
        public ExternalUrls external_urls { get; set; }
    }

    public class Album
    {
        public string name { get; set; }
        public ExternalUrls external_urls { get; set; }
    }

    public class Artist
    {
        public string name { get; set; }
        public ExternalUrls external_urls { get; set; }
    }

    public class Track
    {
        public Album album { get; set; }
        public List<Artist> artists { get; set; }
        public ExternalUrls external_urls { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public int track_number { get; set; }
        public long duration_ms { get; set; }

        public override string ToString()
        {
            return $"{name} -- {string.Join(", ", artists.Select(i => i.name))} -- {album.name}"; 
        }
    }

    public class Item
    {
        public DateTime added_at { get; set; }
        public Track track { get; set; }
    }

    public class Tracks
    {
        public List<Item> items { get; set; }
        public string next { get; set; }
        public object previous { get; set; }
        public int total { get; set; }
    }

    public class Playlists
    {
        public ExternalUrls external_urls { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public Owner owner { get; set; }
        public string snapshot_id { get; set; }
        public string id { get; set; }
        public Tracks tracks { get; set; }
    }


}
