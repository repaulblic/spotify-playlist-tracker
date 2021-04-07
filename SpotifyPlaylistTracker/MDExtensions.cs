using System;
using System.Collections.Generic;
using System.Text;

namespace SpotifyPlaylistHistory
{
    static class MDExtensions
    {
        public static string GenerateMD(Playlists playlist)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"### [{playlist.name}]({playlist.external_urls.spotify})");
            sb.AppendLine();
            sb.AppendLine($"> {playlist.description}");
            sb.AppendLine($"###### Version ID: {playlist.snapshot_id}");
            sb.AppendLine();
            sb.AppendLine("| No. | Title | Artist(s) | Album | Length |");
            sb.AppendLine("|---|---|---|---|---|");
            int i = 1;

            foreach (var item in playlist.tracks.items)
            {
                StringBuilder trackSB = new StringBuilder();
                List<string> artists = new List<string>();
                TimeSpan trackLength = TimeSpan.FromMilliseconds(item.track.duration_ms);

                trackSB.Append($"| {i} | ");
                trackSB.Append($"[{item.track.name}]({item.track.external_urls.spotify}) | ");

                foreach (var artist in item.track.artists)
                {
                    artists.Add($"[{artist.name}]({artist.external_urls.spotify})");
                }

                trackSB.Append($"{string.Join(", ", artists)} | ");
                trackSB.Append($"[{item.track.album.name}]({item.track.album.external_urls.spotify}) | ");
                trackSB.Append($"{trackLength.ToString("mm\\:ss")} |");
                sb.AppendLine(trackSB.ToString());
                i++;
            }

            return sb.ToString();

        }
    }
}
