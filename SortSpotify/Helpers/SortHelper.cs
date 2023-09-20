using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Formatters;
using SortSpotify.Controllers;
using SortSpotify.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

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

            Dictionary<string, List<string>> playlistsKeywords= new Dictionary<string, List<string>>();

            playlistsKeywords["pop"] = new List<string> { "pop" };
            playlistsKeywords["instrumental rock"] = new List<string> { "instru","rock" };
            playlistsKeywords["classic rock"] = new List<string> { "classic", "rock" };
            playlistsKeywords["modern rock | hard rock"] = new List<string> { "modern", "rock", "hard" };
            playlistsKeywords["alt rock"] = new List<string> { "alt", "rock" };
            playlistsKeywords["indie"] = new List<string> { "indie" };
            playlistsKeywords["blues"] = new List<string> { "blues" };
            playlistsKeywords["prog rock"] = new List<string> { "prog", "rock" };
            playlistsKeywords["pop rock"] = new List<string> { "pop", "rock" };
            playlistsKeywords["power metal"] = new List<string> { "power", "metal" };
            playlistsKeywords["alt metal"] = new List<string> { "alt", "metal" };
            playlistsKeywords["djent | prog metal"] = new List<string> { "djent", "prog", "metal" };
            playlistsKeywords["symphonic metal"] = new List<string> { "symphon", "metal" };
            playlistsKeywords["synthwave | darkwave | cyberpunk | darksynth | spacewave | chillsynth"] = new List<string> { "synthwave", "chillsynth", "darkwave", "cyberpunk", "darksynth", "spacewave" };
            playlistsKeywords["tropical house"] = new List<string> { "tropi", "house" };
            playlistsKeywords["rap"] = new List<string> { "rap", "hip", "hop" };
            playlistsKeywords["reggae"] = new List<string> { "reggae" };


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


                LibraryResponse response = JsonSerializer.Deserialize<LibraryResponse>(responseJsonString);


                savedTracks.AddRange(response.items);

                getLibraryUrl = response.next;

            } while (getLibraryUrl is not null);


            List<MusicInfo> musicInfos = savedTracks.Select<SavedTrack, MusicInfo>(
                savedTrack => new MusicInfo(savedTrack.track.id,
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

                    foreach (KeyValuePair<string, List<string>> genresToPlaylist in playlistsKeywords)
                    {

                        bool keywordAnd= true;
                        bool keywordOr= false;

                        foreach (string keyword in genresToPlaylist.Value)
                        {

                            if (!genre.Contains(keyword))
                            {
                                keywordAnd = false;
                                break;
                            }
                            if (genre.Contains(keyword))
                            {
                                keywordOr = true;
                                break;
                            }


                        }

                        if ()
                        {
                            musicInfo.playlistNames.Add(genresToPlaylist.Key);
                        }

                    }

                }

                if(musicInfo.playlistNames.Count == 0) 
                {
                    musicInfo.playlistNames.Add("default");
                }

            }

            musicInfos.GroupBy(musicInfo => musicInfo.playlistNames);


            List<string> playlistsNames = musicInfos.SelectMany(musicInfo => musicInfo.playlistNames).Distinct().ToList();
            Dictionary<string, List<MusicInfo>> playlists = new Dictionary<string, List<MusicInfo>>();

            foreach (MusicInfo musicInfo in musicInfos)
            {
            
                foreach (string value in musicInfo.playlistNames)
                {
                    if (!playlists.ContainsKey(value))
                    {
                        playlists[value] = new List<MusicInfo>();
                    }
                    playlists[value].Add(musicInfo);
                }
            
            }



            string playlistsStr = JsonSerializer.Serialize(playlists);

            File.WriteAllText(@"C:\Users\Quentin Le Nestour\Documents\playlists.json", playlistsStr);


            string musicInfosStr = JsonSerializer.Serialize(musicInfos);

            File.WriteAllText(@"C:\Users\Quentin Le Nestour\Documents\musicInfos.json", musicInfosStr);


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
