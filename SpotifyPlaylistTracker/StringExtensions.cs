using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpotifyPlaylistTracker
{
    static class StringExtensions
    {
        public static string GenerateMD(Playlists playlist)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder tracksSb = new StringBuilder();
            
            int songCount = 1;
            long totalMs = 0;

            foreach (var item in playlist.tracks.items)
            {
                StringBuilder trackSB = new StringBuilder();
                List<string> artists = new List<string>();
                TimeSpan trackLength = TimeSpan.FromMilliseconds(item.track.duration_ms);

                trackSB.Append($"| {songCount} | ");
                trackSB.Append($"[{item.track.name}]({item.track.external_urls.spotify}) | ");

                foreach (var artist in item.track.artists)
                {
                    artists.Add($"[{artist.name}]({artist.external_urls.spotify})");
                }

                trackSB.Append($"{string.Join(", ", artists)} | ");
                trackSB.Append($"[{item.track.album.name}]({item.track.album.external_urls.spotify}) | ");
                trackSB.Append($"{trackLength.ToString("mm\\:ss")} |");
                tracksSb.AppendLine(trackSB.ToString());
                songCount++;
                totalMs += item.track.duration_ms;
            }

            TimeSpan totalTime = TimeSpan.FromMilliseconds(totalMs);

            sb.AppendLine($"### [{playlist.name}]({playlist.external_urls.spotify})");
            sb.AppendLine();
            if (!string.IsNullOrWhiteSpace(playlist.description))
            {
                sb.AppendLine($"> {playlist.description}<br>");
            }
            else
            {
                sb.AppendLine($"> ");

            }
            sb.AppendLine($"> Created by [{playlist.owner.display_name}]({playlist.owner.external_urls.spotify}) • {songCount} songs, {GetTimeString(totalTime)}");
            sb.AppendLine("");
            sb.AppendLine("| No. | Title | Artist(s) | Album | Length |");
            sb.AppendLine("|---|---|---|---|---|");
            sb.Append(tracksSb);

            return sb.ToString();
        }

        public static string GenerateJSON(Playlists playlist)
        {
            RawRecord record = new RawRecord
            {
                playlistName = playlist.name,
                playlistID = playlist.id,
                description = playlist.description,
                tracks = playlist.tracks.items.Select(i => i.track.ToString()).ToList()
            };

            record.tracks.Sort();

            return JsonConvert.SerializeObject(record, Formatting.Indented);
        }

        private static string GetTimeString(TimeSpan ts)
        {
            switch (ts.TotalMinutes)
            {
                case double m when m <= 10:
                    return $"{ts.Minutes} min {ts.Seconds} sec";
                case double m when 10 < m && m <= 60:
                    return $"{ts.Minutes} min";
                case double m when 60 < m && m <= 60*36:
                    return $"{ts.Hours} hr {ts.Minutes} min";
                case double m when 60 * 36 < m && m <= 60*48:
                    return $"{ts.Days} day {ts.Hours} hr";
                case double m when 60 * 48 < m:
                    return $"{ts.Days} days {ts.Hours} hr";
                default:
                    return "";
            }
        }
    }
}
