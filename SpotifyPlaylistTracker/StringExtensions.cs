using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpotifyPlaylistTracker
{
    static class StringExtensions
    {
        public static string GeneratePrettyMD(Playlists playlist)
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
            sb.AppendLine($"> Last Checked: {playlist.timeStamp:u}");
            sb.AppendLine("");
            sb.AppendLine("| No. | Title | Artist(s) | Album | Length |");
            sb.AppendLine("|---|---|---|---|---|");
            sb.Append(tracksSb);

            return sb.ToString();
        }

        public static ReadMeTableEntry CreateReadMeTableEntry(Playlists playlist)
        {
            TimeSpan totalTime = TimeSpan.FromMilliseconds(playlist.tracks.items.Sum(i => i.track.duration_ms));

            ReadMeTableEntry entry = new ReadMeTableEntry()
            {
                PlaylistId = playlist.id,
                PlaylistName = playlist.name,
                Songs = playlist.tracks.items.Count,
                PlaylistLength = GetTimeString(totalTime)

            };

            return entry;
        }

        public static RawRecord RawRecordFromString(string recordString)
        {
            RawRecord rawRecord =  JsonConvert.DeserializeObject<RawRecord>(recordString);
            rawRecord.tracks.Sort();

            return rawRecord;
        }

        public static string GenerateJSON(RawRecord record)
        {
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

        internal static string GenerateReadMe(List<ReadMeTableEntry> readMeEntries)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder entriesSb = new StringBuilder();

            foreach (var entry in readMeEntries)
            {
                StringBuilder entrySB = new StringBuilder();
                entrySB.Append($"[{entry.PlaylistName}](/Playlists/Pretty/{entry.PlaylistId}.md) | ");
                entrySB.Append($"| {entry.Songs} | ");
                entrySB.Append($"| {entry.PlaylistLength} | ");
                string lastChange = entry.LastChanged != default ? entry.LastChanged.ToShortDateString() : "Unknown";
                entrySB.Append($"| {lastChange} | ");
                entriesSb.AppendLine(entrySB.ToString());

            }

            sb.AppendLine("## Playlists");
            sb.AppendLine("|Playlist | Songs | Playlist Length| Last Change Date|");
            sb.AppendLine("|---|---|---|---|");
            sb.Append(entriesSb);

            return sb.ToString();
        }
    }
}
