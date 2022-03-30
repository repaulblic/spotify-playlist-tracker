using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace SpotifyPlaylistTracker
{
    public class RawRecord : IEquatable<RawRecord>
    {
        public DateTime timeStamp { get; set; }
        public string playlistName { get; set; }
        public string playlistID { get; set; }
        public string description { get; set; }
        public List<string> tracks { get; set; }

        public bool Equals([AllowNull] RawRecord other)
        {
            if(other == null)
            {
                return false;
            }
            else
            {
                this.tracks.Sort();
                other.tracks.Sort();

                bool isEqual = true;
                isEqual &= this.playlistID == other.playlistID;
                isEqual &= this.tracks.SequenceEqual(other.tracks);

                return isEqual;
            }
        }

        public (int additions, int deletions) Diff([AllowNull]RawRecord previous)
        {
            if (this.Equals(previous))
            {
                return (0, 0);
            }
            else if (previous == null)
            {
                return (this.tracks.Count, 0);
            }
            else
            {
                int additions = 0;
                int deletions = 0;

                foreach (var item in this.tracks)
                {
                    if (!previous.tracks.Contains(item))
                    {
                        additions++;
                    }
                }

                foreach (var item in previous.tracks)
                {
                    if (!this.tracks.Contains(item))
                    {
                        deletions++;
                    }
                }

                return (additions, deletions);
            }
        }

        public static RawRecord RawRecordFromPlaylist(Playlists playlist)
        {
            RawRecord record = new RawRecord
            {
                timeStamp = playlist.timeStamp,
                playlistName = playlist.name,
                playlistID = playlist.id,
                description = playlist.description,
                tracks = playlist.tracks.items.Select(i => i.track.ToString()).ToList(),
            };

            record.tracks.Sort();

            return record;
        }

        
    }
}
