using RestSharp;
using System;
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

            foreach (var playlistId in playlistIDs)
            {
                var id = playlistId.Split(':').Last();

                request = new RestRequest($"{playlistURL}/{id}", Method.GET);
                request.AddHeader("Authorization", $"Bearer {response.Data.access_token}");
                request.AddParameter("fields", "id,name,description,owner,snapshot_id,external_urls,tracks.items(added_at,track.name, track.track_number,track.id,track.album.name,track.duration_ms,track.album.external_urls.spotify,track.artists(name,external_urls),track.external_urls.spotify),tracks.total,tracks.next,tracks.previous");

                IRestResponse<Playlists> playlistResponse = restClient.Get<Playlists>(request);

                if (!playlistResponse.IsSuccessful)
                {
                    Console.WriteLine("Get Playlist error");
                    Console.WriteLine(playlistResponse.Content);
                    return;
                }

                Playlists playlist = playlistResponse.Data;
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

                File.WriteAllText(Path.Combine(prettyPath, $"{playlist.id}.md"), StringExtensions.GenerateMD(playlist));
                File.WriteAllText(Path.Combine(rawPath, $"{playlist.id}"), StringExtensions.GenerateJSON(playlist));

            }
        }
    }
}
