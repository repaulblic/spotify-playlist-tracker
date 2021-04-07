using RestSharp;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace SpotifyPlaylistHistory
{
    class Program
    {
        private static readonly string tokenURL = "https://accounts.spotify.com/api/token";
        private static readonly string playlistURL = "https://api.spotify.com/v1/playlists";

        static void Main(string[] args)
        {
            string playlistID = "7AodoCcN7r6zCDut0GnG8g";
            RestSharp.RestClient restClient = new RestSharp.RestClient();

            var request = new RestRequest(tokenURL, Method.POST);
            request.AddHeader("Authorization", "Basic ");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("grant_type", "client_credentials");

            IRestResponse<TokenResponse> response = null;
            do
            {
                response = restClient.Post<TokenResponse>(request);

                if (!response.IsSuccessful)
                {
                    Thread.Sleep(10000);
                }
            } while (!response.IsSuccessful);


            request = new RestRequest($"{playlistURL}/{playlistID}",Method.GET);
            request.AddHeader("Authorization", $"Bearer {response.Data.access_token}");
            request.AddParameter("fields", "name,owner.display_name,snapshot_id,external_urls,tracks.items(added_at,track.name, track.track_number,track.id,track.album.name,track.duration_ms,track.album.external_urls.spotify,track.artists(name,external_urls),track.external_urls.spotify),tracks.total,tracks.next,tracks.previous");

            IRestResponse<Playlists> playlistResponse = restClient.Get<Playlists>(request);

            if (!playlistResponse.IsSuccessful)
            {
                Console.WriteLine("Get Playlist error");
                Console.WriteLine(playlistResponse.Content);
                return;
            }

            Playlists playlist = playlistResponse.Data;
            string next = playlistResponse.Data.tracks.next;

            while ( next != null)
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

            StringBuilder sb = new StringBuilder();
            Console.WriteLine();
            string s = $"{ playlist.name } - By { playlist.owner.display_name}\tversion: {playlist.snapshot_id}";
            Console.WriteLine(s);
            Console.WriteLine(string.Concat(Enumerable.Repeat("-",s.Length)));

            sb.AppendLine(s);
            sb.AppendLine(string.Concat(Enumerable.Repeat("-", s.Length)));

            int maxTitle = playlist.tracks.items.Max(i => i.track.name.Length);
            int maxArtist = playlist.tracks.items.Max(i => string.Join(", ", i.track.artists.Select(j => j.name)).Length);
            int maxAdded = 24;


            foreach (var item in playlist.tracks.items)
            {
                Console.WriteLine(String.Format($"{{0,-{maxTitle}}} | {{1,-{maxArtist}}} | {{2,{maxAdded}}}", item.track.name, string.Join(", ", item.track.artists.Select(i => i.name)), item.added_at));
                sb.AppendLine(String.Format($"{{0,-{maxTitle}}} | {{1,-{maxArtist}}} | {{2,{maxAdded}}}", item.track.name, string.Join(", ", item.track.artists.Select(i => i.name)), item.added_at));
            }

            File.WriteAllText($"{playlistID}.md", MDExtensions.GenerateMD(playlist));
        }
    }
}
