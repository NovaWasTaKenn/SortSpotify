using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Formatters;
using SortSpotify.Controllers;
using SortSpotify.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using static System.Net.WebRequestMethods;

namespace SortSpotify.Helpers
{
    public static class SortHelper
    {

        private static string _currentUserId;


        public static List<string> ExtractKeywords(string str)
        {

            List<string> keywords = str.Split(" ").ToList();

            return keywords;
            
        }

        public static async void Sort()
        {
            Console.WriteLine("Sort");

            List<SavedTrack> savedTracks = new List<SavedTrack>();

            Dictionary<string, string> playlistsKeywords= new Dictionary<string, string>();

            playlistsKeywords["pop"] = "(pop)";
            playlistsKeywords["instrumental rock"] = "(?=.*instru.*)(?=.*rock)";
            playlistsKeywords["classic rock"] = "(?=.*classi.*)(?=.*rock)";
            playlistsKeywords["modern rock | hard rock"] = "(?=.*(modern.*)|(hard))(?=.*rock)";
            playlistsKeywords["alt rock"] = "(?=.*alt.*)(?=.*rock)";
            playlistsKeywords["indie"] = "(indie)";
            playlistsKeywords["blues"] = "(blues)";
            playlistsKeywords["prog rock"] = "(?=.*prog.*)(?=.*rock)";
            playlistsKeywords["pop rock"] = "(?=.*pop)(?=.*rock)";
            playlistsKeywords["power metal"] = "(?=.*power)(?=.*metal)";
            playlistsKeywords["alt metal"] = "(?=.*alt.*)(?=.*metal)";
            playlistsKeywords["djent | prog metal"] = "(?=.*(djent)|(prog))(?=.*metal)";
            playlistsKeywords["symphonic metal"] = "(symphon.*)&(metal)";
            playlistsKeywords["synthwave | darkwave | cyberpunk | darksynth | spacewave | chillsynth"] = @"(synthwave)|(chillsynth)|(darkwave)|(cyberpunk)|(darksynth)|(spacewave)|(filter\s*house)";
            playlistsKeywords["tropical house"] = "(?=.*tropi.*)(?=.*house)";
            playlistsKeywords["rap"] = "(rap)|((?=.*hip)(?=.*hop))";
            playlistsKeywords["reggae"] = "(reggae)";
            playlistsKeywords["downtempo"] = @"(downtempo)|(trip\shop)|(livetronica)|(electronica)|(american\spost\srock)";
            playlistsKeywords["grunge"] = @"(grunge)|(permanent\swave)";

            //Dictionary<string, List<string>> genresToPlaylists= new Dictionary<string, List<string>>();
            //
            //
            //foreach (KeyValuePair<string, List<string>> kvp in playlistsKeywords)
            //{
            //
            //    foreach (string value in kvp.Value)
            //    {
            //        if (!genresToPlaylists.ContainsKey(value))
            //        {
            //            genresToPlaylists[value] = new List<string>();
            //        }
            //        genresToPlaylists[value].Add(kvp.Key);
            //    }
            //
            //} modern  |  rock&alt



            //Dictionary<string, List<string>> genresToPlaylists = new Dictionary<string, List<string>>();
            //
            //genresToPlaylists["pop"] = new List<string> { @"pop" };
            //genresToPlaylists["instrumental rock"] = new List<string> { @"(?:instru.*rock|rock.*instru)" };
            //genresToPlaylists["classic rock"] = new List<string> { @"(?:classic.*rock|rock.*classic)" };
            //genresToPlaylists["modern rock | hard rock"] = new List<string> { @"(?:modern.*rock|rock.*modern)", @"(?:hard.*rock|rock.*hard)" };
            //genresToPlaylists["alt rock"] = new List<string> { @"(?:alt.*rock|rock.*alt)" };
            //
            //genresToPlaylists["indie"] = new List<string> { @"indie" };
            //genresToPlaylists["blues"] = new List<string> { @"blues" };
            //genresToPlaylists["prog rock"] = new List<string> { @"(?:prog.*rock|rock.*prog)" };
            //genresToPlaylists["pop rock"] = new List<string> { @"(?:pop.*rock|rock?:pop)" };
            //genresToPlaylists["power metal"] = new List<string> { @"(?:power.*metal|metal.*power)" };
            //genresToPlaylists["alt metal"] = new List<string> { @"(?:alt.*metal|metal.*alt)" };
            //genresToPlaylists["djent | prog metal"] = new List<string> { @"djent", @"(?:prog.*metal|metal.*prog)" };
            //genresToPlaylists["symphonic metal"] = new List<string> { @"(?:symphon.*metal|symphon.*metal)" };
            //genresToPlaylists["synthwave | darkwave | cyberpunk | darksynth | spacewave"] = new List<string> { @"synthwave", @"darkwave", @"cyberpunk", @"darksynth", @"spacewave" };
            //genresToPlaylists["tropical house"] = new List<string> { @"(?:tropical.*house)" };
            //genresToPlaylists["rap"] = new List<string> { @"rap", @"hip-hop" };
            //genresToPlaylists["reggae"] = new List<string> { @"reggae"};



            string getLibraryUrl = "https://api.spotify.com/v1/me/tracks?limit=50";
            do
            {

                string responseJsonString = await ApiHelper.DoWithRetryAsync(getLibraryUrl, 5, HttpMethod.Get, null, null, true).ConfigureAwait(false);
                
                //File.WriteAllText(@"C:\Users\Quentin Le Nestour\Documents\savedTrackResponse.json", responseJsonString);


                LibraryResponse responseLibrary = JsonSerializer.Deserialize<LibraryResponse>(responseJsonString);


                savedTracks.AddRange(responseLibrary.items);

                getLibraryUrl = responseLibrary.next;

            } while (getLibraryUrl is not null);


            List<MusicInfo> musicInfos = savedTracks.Select<SavedTrack, MusicInfo>(
                savedTrack => new MusicInfo(savedTrack.track.id,
                savedTrack.track.uri,
                savedTrack.track.name,
                savedTrack.track.artists.Select(artist => artist.id).ToList(),
                savedTrack.track.artists.Select(artist => artist.name).ToList()
                )).ToList();


            string getArtistBaseUrl = "https://api.spotify.com/v1/artists"; //?ids=382ObEPsp2rxGrnsizN5TX%2C1A2GTWGtFfWp7KSQTwWOyo%2C2noRn2Aes5aoNVsU6iWThc


            List<string> artistsIds = musicInfos.SelectMany(musicInfo => musicInfo.artistIds).Distinct().ToList();

            int count = 0;
            int len = artistsIds.Count;

            List<Artist> artistList = new List<Artist>();

            while (count < len)
            {

                string artistStr = "";

                if(len-count < 20)
                {
                    artistStr = String.Join(",", artistsIds.GetRange(count, len-count));
                    count += len - count;
                }
                else
                {
                    artistStr = String.Join(",", artistsIds.GetRange(count, 20));
                    count += 20;
                }


                string getArtistUrl = getArtistBaseUrl + $"?ids={artistStr}";
                string responseJsonString = await ApiHelper.DoWithRetryAsync(getArtistUrl, 5, HttpMethod.Get, null, null, true).ConfigureAwait(false);
                
                //File.WriteAllText(@"C:\Users\Quentin Le Nestour\Documents\savedAlbumResponse.json", responseJsonString);
                
                
                ArtistResponse artistResponse = JsonSerializer.Deserialize<ArtistResponse>(responseJsonString);

                artistList.AddRange(artistResponse.artists);

                //musicInfo.genres = artistResponse.artists.SelectMany(artist => artist.genres).Distinct().ToList();

            }

            List<MusicInfo> musicInfosWithGenre = new List<MusicInfo>();

            foreach (var artist in artistList)
            {
                foreach (var musicInfo in musicInfos)
                {
                    if (musicInfo.artistIds.Contains(artist.id) & musicInfosWithGenre.Where(m => m.id == musicInfo.id).Count() == 0)
                    {
                        musicInfo.genres.AddRange(artist.genres);
                        musicInfosWithGenre.Add(musicInfo);
                    }
                    if (musicInfo.artistIds.Contains(artist.id) & musicInfosWithGenre.Where(m => m.id == musicInfo.id).Count() != 0)
                    {
                        List<string> genresToAdd = artist.genres.Except(musicInfo.genres).ToList();
                        musicInfo.genres.AddRange(genresToAdd);
                    }


                }
            }

            //foreach(MusicInfo musicInfo in musicInfos)
            //{
            //
            //    string artistsIdsStr = String.Join(",", musicInfo.artistIds);
            //
            //    string getArtistUrl = getArtistBaseUrl + $"?ids={artistsIdsStr}";
            //    string responseJsonString = await ApiHelper.DoWithRetryAsync(getArtistUrl, 5, HttpMethod.Get, null, null, true).ConfigureAwait(false);
            //
            //    //File.WriteAllText(@"C:\Users\Quentin Le Nestour\Documents\savedAlbumResponse.json", responseJsonString);
            //
            //
            //    ArtistResponse artistResponse = JsonSerializer.Deserialize<ArtistResponse>(responseJsonString);
            //
            //    musicInfo.genres = artistResponse.artists.SelectMany(artist => artist.genres).Distinct().ToList();
            //
            //}

            

            foreach (MusicInfo musicInfo in musicInfos)
            {

                foreach (string genre in musicInfo.genres)
                {

                    foreach (KeyValuePair<string, string> keyword in playlistsKeywords)
                    {

                        if (Regex.Match(genre, keyword.Value).Success)
                        {
                            musicInfo.playlistNames.Add(keyword.Key);
                        }

                    }

                }


                if(musicInfo.playlistNames.Count() == 0)
                {
                    musicInfo.playlistNames.Add("default");
                }

            }

            musicInfos.GroupBy(musicInfo => musicInfo.playlistNames);


            List<string> playlistsNames = musicInfos.SelectMany(musicInfo => musicInfo.playlistNames).Distinct().ToList();
            Dictionary<string, List<MusicInfo>> playlists = new Dictionary<string, List<MusicInfo>>();

            foreach (MusicInfo musicInfo in musicInfos)
            {
            
                foreach (string playlistName in musicInfo.playlistNames)
                {
                    if (!playlists.ContainsKey(playlistName))
                    {
                        playlists[playlistName] = new List<MusicInfo>();
                    }
                    playlists[playlistName].Add(musicInfo);
                }
            
            }

            //Genres non pris en cpt : ((livetronica, downtempo, trip hop), american post-rock), none, vocaloid, (grunge, permanent wave, rock), 


            string getPlaylistsUrl = "https://api.spotify.com/v1/me/playlists?limit=50";

            List<Playlist> userPlaylists = new List<Playlist>();

            do
            {

                string responseJsonString = await ApiHelper.DoWithRetryAsync(getPlaylistsUrl, 5, HttpMethod.Get, null, null, true).ConfigureAwait(false);

                //File.WriteAllText(@"C:\Users\Quentin Le Nestour\Documents\savedTrackResponse.json", responseJsonString);


                PlaylistResponse responsePLaylist = JsonSerializer.Deserialize<PlaylistResponse>(responseJsonString);


                userPlaylists.AddRange(responsePLaylist.items);

                getPlaylistsUrl = responsePLaylist.next;

            } while (getPlaylistsUrl is not null);

            List<string> userPlaylistsNames = userPlaylists.Select(userPlaylist => userPlaylist.name).Distinct().ToList();


            string getCurrentUser = "https://api.spotify.com/v1/me";
            string userResponseJson = await ApiHelper.DoWithRetryAsync(getCurrentUser, 5, HttpMethod.Get, null, null, true).ConfigureAwait(false);

            UserResponse response = JsonSerializer.Deserialize<UserResponse>(userResponseJson);

            string userId = response.id;

            string createPlaylistUrl = $"https://api.spotify.com/v1/users/{userId}/playlists";

            foreach (var playlist in playlists)
            {

                string playlistId = "";

                if (!userPlaylistsNames.Contains(playlist.Key))
                {
                    string responseJsonString = await ApiHelper.DoWithRetryAsync(createPlaylistUrl, 5, HttpMethod.Post, new StringContent(
                        $"{{\"name\":\"{playlist.Key}\",\"description\":\"Playlist created automatically\",\"public\":false,\"collaborative\":false}}",
                        System.Text.Encoding.UTF8, "application/json"), null, true).ConfigureAwait(false);

                    Playlist playlistResponse = JsonSerializer.Deserialize<Playlist>(responseJsonString);

                    playlistId = playlistResponse.id;

                }
                else
                {
                    playlistId = userPlaylists.Where(userPlaylist => userPlaylist.name == playlist.Key).ToList()[0].id;

                }





                string addTracksUrl = $"https://api.spotify.com/v1/playlists/{playlist.Key}/tracks";

                int countTracks = 0;
                int lenTracks = playlist.Value.Count;

                do
                {

                    string uris = String.Join(",", playlist.Value.Select(musicInfo => musicInfo.uri).ToList().GetRange(countTracks, lenTracks - countTracks < 100 ? lenTracks - countTracks : 100));

                    countTracks += lenTracks - countTracks < 100 ? lenTracks - countTracks : 100;

                    
                    string responseJsonString = await ApiHelper.DoWithRetryAsync(addTracksUrl, 5, HttpMethod.Post, new StringContent(
                        $"{{\"playlist_id\":\"{playlistId}\",\"uris\":\"{uris}\"}}",
                        System.Text.Encoding.UTF8, "application/json"), null, true).ConfigureAwait(false);

                } while (countTracks < len);

            }

            


            string playlistsStr = JsonSerializer.Serialize(playlists);

            System.IO.File.WriteAllText(@"C:\Users\Quentin Le Nestour\Documents\playlists.json", playlistsStr);


            string musicInfosStr = JsonSerializer.Serialize(musicInfos);

            System.IO.File.WriteAllText(@"C:\Users\Quentin Le Nestour\Documents\musicInfos.json", musicInfosStr);


            Console.WriteLine(savedTracks);

            //genres = genres.Select<string, string>(x => {
            //
            //    var genreSplit = x.Split(" ").ToList();
            //    if (genreSplit.Count >= 3)
            //    {
            //        return String.Join(" ", genreSplit.GetRange(1, genreSplit.Count - 1));
            //    }
            //    return x;
            //
            //} ).ToList();
            //
            //genres = genres.Distinct().ToList();

            //
            //
            //var genres2 = new List<string>();
            //genres.CopyTo(genres2.ToArray());
            //
            //foreach (string genre in genres)
            //{
            //
            //    foreach (var item in genresAGarder)
            //    {
            //
            //        if (genre.ToLower().Contains(item))
            //        {
            //            genres2.Remove(genre);
            //            break;
            //        }
            //
            //    }
            //
            //}

           

            Console.WriteLine(musicInfos);
            Console.WriteLine(playlists);

        }
        
        
    }
}
