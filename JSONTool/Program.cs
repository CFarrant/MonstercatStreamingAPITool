﻿using JSONTool.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace JSONTool
{
    public class Program
    {
        public static string username = "JSONParseTool";
        public static string password = "APIDev";
        public static string authentication = Base64Encode(username+":"+password);

        public static Dictionary<string, Artist> APIArtists;
        public static Dictionary<string, Album> APIAlbums;
        public static Dictionary<string, Track> APITracks;

        public static Dictionary<string, Artist> allArtists;
        public static List<Album> allAlbums;
        public static List<Track> allTracks;

        public static void Main(string[] args)
        {
            allArtists = new Dictionary<string, Artist>();
            Console.WriteLine("[INFO] Calling API & Grabbing All Existing Data...");
            GrabAllAPIAlbumsAsync().Wait();
            Console.WriteLine("[INFO] Calling Monstercat.com & Grabbing All New Data...");
            GrabAllSourceAlbumsAsync().Wait();
            Console.WriteLine("[INFO] Calling API & Saving All New Data...");
            PersistData();
            Console.WriteLine("[INFO] All Albums & Songs Imported, Press Any Key to Continue...");
            Console.ReadKey(true);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return "Basic " + System.Convert.ToBase64String(plainTextBytes);
        }

        private static async Task GrabAllAPIAlbumsAsync()
        {
            string jsonString = "";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://monstercatstreaming.tk:8080/api/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync($"album/");
                response.EnsureSuccessStatusCode();
                jsonString = await response.Content.ReadAsStringAsync();
            }

            var jsSerializer = new JavaScriptSerializer();
            APIAlbums = new Dictionary<string, Album>();
            APIArtists = new Dictionary<string, Artist>();
            APITracks = new Dictionary<string, Track>();

            object[] deserialize = (object[])jsSerializer.DeserializeObject(jsonString);
            foreach(Dictionary<string, object> o in deserialize)
            {
                o.TryGetValue("id", out object id);
                o.TryGetValue("name", out object name);
                o.TryGetValue("type", out object type);
                o.TryGetValue("releaseCode", out object releaseCode);
                o.TryGetValue("genreprimary", out object genreprimary);
                o.TryGetValue("genresecondary", out object genresecondary);
                o.TryGetValue("coverURL", out object coverURL);
                o.TryGetValue("artist", out object artist);

                var artistDictionary = (Dictionary<string, object>)artist;

                artistDictionary.TryGetValue("name", out object artistName);

                Artist art = new Artist((string)artistName);
                Album alb = new Album((string)id, (string)name, art, (string)type, (string)releaseCode, (string)coverURL, (string)genreprimary, (string)genresecondary);
                try
                {
                    Console.WriteLine("[INFO]: Caching Album " + id);
                    APIAlbums.Add(alb.id, alb);
                }
                catch (Exception ex){}
                try
                {
                    Console.WriteLine("[INFO]: Caching Artist " + artistName);
                    APIArtists.Add(art.name, art);
                }
                catch (Exception ex){}
            }

            Console.WriteLine("[INFO]: All Albums in API Cached");

            foreach(string albumId in APIAlbums.Keys)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://monstercatstreaming.tk:8080/api/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync($"album/{albumId}");
                    response.EnsureSuccessStatusCode();
                    jsonString = await response.Content.ReadAsStringAsync();
                }

                deserialize = (object[])jsSerializer.DeserializeObject(jsonString);
                foreach (Dictionary<string, object> o in deserialize)
                {
                    o.TryGetValue("id", out object id);
                    o.TryGetValue("tracknumber", out object tracknumber);
                    o.TryGetValue("title", out object title);
                    o.TryGetValue("genreprimary", out object genreprimary);
                    o.TryGetValue("genresecondary", out object genresecondary);
                    o.TryGetValue("songURL", out object songURL);
                    o.TryGetValue("artist", out object artist);

                    var artistDictionary = (Dictionary<string, object>)artist;

                    artistDictionary.TryGetValue("name", out object artistName);

                    Artist art = new Artist((string)artistName);
                    Track tra = new Track((string)id, (string)title, art, (string)genreprimary, (string)genresecondary, (string)songURL, new Album(), (int)tracknumber);

                    try
                    {
                        Console.WriteLine("[INFO]: Caching Track " + tra.getId());
                        APITracks.Add(tra.getId(), tra);
                    }
                    catch (Exception ex) { }
                    try
                    {
                        Console.WriteLine("[INFO]: Caching Artist " + artistName);
                        APIArtists.Add(art.name, art);
                    }
                    catch (Exception ex) { }
                }
            }
        }

        private static async Task GrabAllSourceAlbumsAsync()
        {
            string jsonString = "";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://connect.monstercat.com");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync($"api/catalog/release/");
                response.EnsureSuccessStatusCode();
                jsonString = await response.Content.ReadAsStringAsync();
            }

            allAlbums = new List<Album>();
            var jsSerializer = new JavaScriptSerializer();
            Dictionary<string, object> dict = (Dictionary<string, object>)jsSerializer.DeserializeObject(jsonString);

            dict.TryGetValue("total", out object totalReleases);
            int total = (int)totalReleases;
            int skip = 0;
            do
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://connect.monstercat.com");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync($"api/catalog/release?skip={skip}");
                    response.EnsureSuccessStatusCode();
                    jsonString = await response.Content.ReadAsStringAsync();
                }

                dict = (Dictionary<string, object>)jsSerializer.DeserializeObject(jsonString);

                dict.TryGetValue("results", out object result);
                object[] temp = (object[])result;
                int i = 0;
                while (temp.Count() > i)
                {
                    dict = (Dictionary<string, object>)temp[i];
                    dict.TryGetValue("_id", out object id);
                    object name = null;
                    dict.TryGetValue("title", out name);
                    if (name == null)
                    {
                        dict.TryGetValue("label", out name);
                    }
                    object releaseCode = null;
                    dict.TryGetValue("catalogId", out releaseCode);
                    if (releaseCode == null)
                    {
                        string tempJson = "";
                        using (var client = new HttpClient())
                        {
                            client.BaseAddress = new Uri("https://connect.monstercat.com");
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            HttpResponseMessage response = await client.GetAsync($"api/catalog/release/{id}/tracks");
                            response.EnsureSuccessStatusCode();
                            tempJson = await response.Content.ReadAsStringAsync();
                        }

                        Dictionary<string, object> dict2 = (Dictionary<string, object>)jsSerializer.DeserializeObject(tempJson);

                        dict2.TryGetValue("results", out object result2);
                        object[] temp2 = (object[])result2;

                        dict2 = (Dictionary<string, object>)temp2[0];
                        dict2.TryGetValue("albumCatalogIds", out object dict3);
                        object[] temp3 = (object[])dict3;
                        releaseCode = temp3[0];
                    }
                    dict.TryGetValue("type", out object type);
                    dict.TryGetValue("coverUrl", out object coverURL);
                    dict.TryGetValue("renderedArtists", out object albumArtist);
                    dict.TryGetValue("genrePrimary", out object primaryGenre);
                    dict.TryGetValue("genreSecondary", out object secondaryGenre);
                    Artist a = new Artist((string)albumArtist);
                    try
                    {
                        if (!APIArtists.ContainsKey(a.getName()))
                        {
                            allArtists.Add(a.getName(), a);
                        }
                    }
                    catch (Exception ex) { }
                    allAlbums.Add(new Album((string)id, (string)name, a, (string)type,
                        (string)releaseCode, (string)coverURL, (string)primaryGenre, (string)secondaryGenre));
                    Console.WriteLine($"[INFO]: Processed Album {skip+i+1} of {total}");
                    i++;
                }
                skip += i;
            } while (skip < total);

            allTracks = new List<Track>();
            foreach (Album a in allAlbums)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://connect.monstercat.com");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync($"api/catalog/release/{a.getId()}/tracks");
                    response.EnsureSuccessStatusCode();
                    jsonString = await response.Content.ReadAsStringAsync();
                }

                dict = (Dictionary<string, object>)jsSerializer.DeserializeObject(jsonString);

                dict.TryGetValue("results", out object result);
                object[] temp = (object[])result;
                int i = 0;

                while (temp.Count() > i){
                    List<Track> tempTrackList = new List<Track>();
                    dict = (Dictionary<string, object>)temp[i];
                    dict.TryGetValue("_id", out object id);
                    dict.TryGetValue("title", out object name);
                    dict.TryGetValue("artistsTitle", out object trackArtist);
                    dict.TryGetValue("genrePrimary", out object primaryGenre);
                    dict.TryGetValue("genreSecondary", out object secondaryGenre);
                    dict.TryGetValue("albums", out object albumsarray);
                    object[] albums = (object[])albumsarray;
                    for (int j = 0; j < albums.Count(); j++)
                    {
                        dict = (Dictionary<string, object>)albums[j];
                        dict.TryGetValue("trackNumber", out object track);
                        dict.TryGetValue("streamHash", out object hash);
                        dict.TryGetValue("albumId", out object albumid);

                        string url = "";
                        if (hash != null)
                        {
                            url = "https://blobcache.monstercat.com/blobs/" + hash;
                            Artist a2 = new Artist((string)trackArtist);
                            try
                            {
                                if (!APIArtists.ContainsKey(a2.getName()))
                                {
                                    allArtists.Add(a2.getName(), a2);
                                }
                            }
                            catch (Exception ex) { }
                            tempTrackList.Add(new Track((string)id, (string)name, a2, (string)primaryGenre, (string)secondaryGenre, url, a, (int)track));
                            Console.WriteLine($"[INFO]: Processed Track {i + 1} of {temp.Count()}");
                        }
                    }

                    foreach (Track t in tempTrackList)
                    {
                        if (!APITracks.ContainsKey(t.getId()))
                        {
                            allTracks.Add(t);
                        }
                    }
                    i++;
                }
            }
        }

        public static void PersistData()
        {
            string host = "http://monstercatstreaming.tk:8080/api/";

            foreach (KeyValuePair<string, Artist> a in allArtists)
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(host+"/artist");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("Authorization", authentication);

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(a.Value);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var results = streamReader.ReadToEnd();
                    Console.WriteLine($"[INFO] Added Artist: \"{a.Value.getName()}\" to the Database");
                }
            }

            List<Album> newAlbums = new List<Album>();
            foreach(Album a in allAlbums)
            {
                if (!APIAlbums.ContainsKey(a.id))
                {
                    newAlbums.Add(a);
                }
            }

            foreach (Album a in newAlbums)
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(host+"/album");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("Authorization", authentication);

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(a);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var results = streamReader.ReadToEnd();
                    Console.WriteLine($"[INFO] Added Album: \"{a.getName()}\" to the Database");
                }
            }

            foreach (Track t in allTracks)
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(host+"/song");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("Authorization", authentication);

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(t);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var results = streamReader.ReadToEnd();
                    Console.WriteLine($"[INFO] Added Song: \"{t.getAlbum().getReleaseCode()} ~ ({t.getTrackNumber()}){t.getName()}\" to the Database");
                }
            }
        }
    }
}
