﻿using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace SpotifyPlaylistTracker
{
    class Program
    {
        private static readonly string tokenURL = "https://accounts.spotify.com/api/token";
        private static readonly string playlistURL = "https://api.spotify.com/v1/playlists";

        static void Main(string[] args)
        {
            var playlistIDs = File.ReadLines("PlaylistIDs.txt");

            string playlistPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Playlists");
            string prettyPath = Path.Combine(playlistPath, "Pretty");
            string rawPath = Path.Combine(playlistPath, "Raw");

            RestSharp.RestClient restClient = new RestSharp.RestClient();

            var request = new RestRequest(tokenURL, Method.POST);
            request.AddHeader("Authorization", $"Basic {args[0]}");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("grant_type", "client_credentials");

            IRestResponse<TokenResponse> response;
            do
            {
                response = restClient.Post<TokenResponse>(request);

                if (!response.IsSuccessful)
                {
                    Thread.Sleep(10000);
                }
            } while (!response.IsSuccessful);

            List<ReadMeTableEntry> readMeEntries = new List<ReadMeTableEntry>();

            foreach (var playlistId in playlistIDs)
            {
                var id = playlistId.Split(':').Last();

                request = new RestRequest($"{playlistURL}/{id}", Method.GET);
                request.AddHeader("Authorization", $"Bearer {response.Data.access_token}");
                
                IRestResponse<Playlists> playlistResponse = restClient.Get<Playlists>(request);

                if (!playlistResponse.IsSuccessful)
                {
                    Console.WriteLine($"Get Playlist error. ID: {playlistId}");
                    Console.WriteLine(playlistResponse.Content);
                    continue;
                }

                Playlists playlist = playlistResponse.Data;
                playlist.timeStamp = DateTime.UtcNow;
                string next = playlistResponse.Data.tracks.next;

                while (next != null)
                {
                    var tracksRequest = new RestRequest(next, Method.GET);
                    tracksRequest.AddHeader("Authorization", $"Bearer {response.Data.access_token}");
                    IRestResponse<Tracks> tracksResponse = restClient.Get<Tracks>(tracksRequest);

                    if (tracksResponse.IsSuccessful)
                    {
                        next = tracksResponse.Data.next;
                        playlist.tracks.items.AddRange(tracksResponse.Data.items);
                    }
                    //DONT DO THIS
                    //COULD CAUSE LOOP
                    else
                    {
                        Console.WriteLine("Error getting tracks");
                        Console.WriteLine(tracksResponse.Content);
                    }
                }

                var currentRawRecord = RawRecord.RawRecordFromPlaylist(playlist);
                var entry = StringExtensions.CreateReadMeTableEntry(playlist);

                if (File.Exists(Path.Combine(rawPath, $"{playlist.id}")))
                {
                    string previousRawRecordString = File.ReadAllText(Path.Combine(rawPath, $"{playlist.id}"));
                    RawRecord previousRawRecord = StringExtensions.RawRecordFromString(previousRawRecordString);
                    if (!currentRawRecord.Equals(previousRawRecord))
                    {
                        entry.LastChanged = DateTime.UtcNow;
                    }
                    else
                    {
                        entry.LastChanged = previousRawRecord.timeStamp;
                        currentRawRecord.timeStamp = previousRawRecord.timeStamp;
                    }

                }

                readMeEntries.Add(entry);
                File.WriteAllText(Path.Combine(prettyPath, $"{playlist.id}.md"), StringExtensions.GeneratePrettyMD(playlist));
                File.WriteAllText(Path.Combine(rawPath, $"{playlist.id}"), StringExtensions.GenerateJSON(currentRawRecord));

            }

            File.WriteAllText("README.md", StringExtensions.GenerateReadMe(readMeEntries));
        }
    }
}
