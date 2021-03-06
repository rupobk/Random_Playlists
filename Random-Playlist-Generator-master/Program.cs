﻿using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Random_Playlist
{
    enum MediaType
    {
        Audio,
        Video,
        Custom,
        Unknown
    }

    class Program
    {
        static readonly List<string> AudioFileTypes = new List<string> { ".aa", ".aac", ".aax", ".act", ".aiff", ".amr", ".ape", ".au", ".awb", ".dct", ".dss", ".dvf", ".flac", ".gsm", ".ikla", ".ivs", ".m4a", ".m4b", ".m4p", ".mmf", ".mp3", ".mpc", ".msv", ".oga", ".ogg", ".opus", ".ra,", ".raw", ".rm", ".sln", ".tta", ".vox", ".wav", ".webm", ".wma", ".wv" };
        static readonly List<string> VideoFileTypes = new List<string> { ".3g2", ".3gp", ".amv", ".asf", ".avi", ".drc", ".flv", ".f4v", ".f4p", ".f4a", ".f4b", ".m2v", ".m4p", ".m4v", ".m4v", ".mkv", ".mng", ".mov", ".mp2", ".mp4", ".mpe", ".mpeg", ".mpeg", ".mpg", ".mpg", ".mpv", ".mxf", ".nsv", ".ogg", ".ogv", ".qt", ".rm", ".rmvb", ".roq", ".svi", ".vob", ".webm", ".wmv", ".yuv" };
        static List<string> CustomFileTypes = null;

        static void Main(string[] args)
        {
            var folders = new List<string>();
            var foldersexcl = new List<string>();
            string playlistName = null;
            var mediaType = MediaType.Unknown;
            var recurseFolders = false;
            var randomOrderPlaylist = false;
            var maxLength = 0;

            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-"))
                {
                    switch (args[i])
                    {
                        case "-d":
                            //include folder list
                            for (var x = i + 1; x < args.Length; x++)
                            {
                                if (args[x].StartsWith("-")) break;
                                i++;
                                folders.Add(args[x]);
                            }
                            break;
                        case "-e":
                            //exclude folder list
                            for (var y = i + 1; y < args.Length; y++)
                            {
                                if (args[y].StartsWith("-")) break;
                                i++;
                                foldersexcl.Add(args[y].ToUpper());
                            }
                            break;
                        case "-r":
                            recurseFolders = true;
                            break;
                        case "-t":
                            //media type
                            i++;
                            if (mediaType != MediaType.Unknown) break;
                            if (args[i].Equals("audio", StringComparison.OrdinalIgnoreCase)) mediaType = MediaType.Audio;
                            else if (args[i].Equals("video", StringComparison.OrdinalIgnoreCase)) mediaType = MediaType.Video;
                            else
                            {
                                Console.WriteLine($"Unknown media type specified: {args[i]}");
                                return;
                            }
                            break;
                        case "-u":
                            randomOrderPlaylist = true;
                            break;
                        case "-m":
                            //max length
                            i++;
                            if (!int.TryParse(args[i], out maxLength))
                            {
                                Console.WriteLine($"Unknown max length format: {args[i]}");
                                return;
                            }
                            break;
                        case "-x":
                            //exclude file types
                            i++;
                            var exts = args[i].Split(';');
                            foreach (var ext in exts)
                            {
                                if (ext.StartsWith("."))
                                {
                                    AudioFileTypes.Remove(ext);
                                    VideoFileTypes.Remove(ext);
                                }
                                else
                                {
                                    var dotext = $".{ext}";
                                    AudioFileTypes.Remove(dotext);
                                    VideoFileTypes.Remove(dotext);
                                }
                            }
                            break;
                        case "-i":
                            //include only these file types
                            i++;
                            mediaType = MediaType.Custom;
                            CustomFileTypes = args[i].Split(';').ToList();
                            for (var x = 0; x < CustomFileTypes.Count; x++)
                            {
                                if (!CustomFileTypes[x].StartsWith("."))
                                {
                                    CustomFileTypes[x] = $".{CustomFileTypes[x]}";
                                }
                            }
                            break;
                        case "-p":
                            i++;
                            playlistName = args[i];
                            break;
                        case "-h":
                            ShowHelp();
                            return;
                    }
                }
                else
                {
                    if (folders.Count == 0) folders.Add(args[i].TrimEnd('\\'));
                    else playlistName = args[i];
                }
            }

            if (folders.Count == 0) folders.Add(Directory.GetCurrentDirectory());
            if (string.IsNullOrEmpty(playlistName)) playlistName = Path.Combine(folders[0], $"Random {Path.GetFileName(folders[0])}");

            if (!playlistName.EndsWith(".m3u", StringComparison.OrdinalIgnoreCase)) playlistName = $"{playlistName}.m3u";
            var playlistFolder = Path.GetDirectoryName(playlistName);

            foreach (var folder in folders)
            {
                if (!Directory.Exists(folder))
                {
                    Console.WriteLine($"Cannot find folder {folder}");
                    return;
                }
            }

            try
            {
                if (randomOrderPlaylist)
                // Wiedergabelisten neu mischen
                {
                    // Wiedergabelisten finden
                    var files = new List<string>();
                    foreach (var folder in folders)
                    {
                        var thisFolderFiles = Directory.EnumerateFiles(folder, "*.m3u", recurseFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                        files.AddRange(thisFolderFiles.ToList<string>());
                    }
                    foreach (var f in files)
                    {
                        // neu sortieren u. Kopie machen
                        var zeilen = new List<string>();
                        string line = "";
                        System.IO.StreamReader file = new System.IO.StreamReader(f.ToString());
                        while ((line = file.ReadLine()) != null)
                        {
                            if (line.Substring(1, 1) != "#")
                                zeilen.Add(line);
                        }
                        file.Close();
                        var rnd = new Random();
                        zeilen = zeilen.OrderBy(x => rnd.Next()).ToList<string>();
                        string neueDatei = f.ToString().Substring(0, f.Length - 4) + "_rnd" + f.ToString().Substring(f.Length - 4, 4);
                        File.WriteAllLines(neueDatei, zeilen);
                    }
                }
                else
                {
                    var files = new List<string>();
                    foreach (var folder in folders)
                    {
                        var thisFolderFiles = Directory.EnumerateFiles(folder, "*", recurseFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                        files.AddRange(thisFolderFiles.ToList<string>());
                    }
                    if (mediaType == MediaType.Unknown)
                    {
                        foreach (var file in files)
                        {
                            //figure out what kind of files these are
                            var ext = Path.GetExtension(file);
                            if (AudioFileTypes.Contains(ext))
                            {
                                mediaType = MediaType.Audio;
                                break;
                            }
                            else if (VideoFileTypes.Contains(ext))
                            {
                                mediaType = MediaType.Video;
                                break;
                            }
                        }
                        if (mediaType == MediaType.Unknown)
                        {
                            //still can't figure it out
                            Console.WriteLine("The folders specified don't contain any media files.");
                            ShowHelp();
                            return;
                        }
                    }

                    switch (mediaType)
                    {
                        case MediaType.Audio:
                            //zuerst nur Audio-Dateien filtern u. Rest weglöschen
                            files = files.Where(f => AudioFileTypes.Contains(Path.GetExtension(f).ToLower())).ToList<string>();

                            //jetzt -e Verzeichnisse weglöschen
                            if (foldersexcl.Count > 0)
                            {
                                var files2 = new List<string>(files);
                                foreach (var file in files2)
                                {
                                    foreach (var folder in foldersexcl)
                                    {
                                        if (file.ToLower().Contains(folder.ToLower()))
                                        {
                                            files.Remove(file);
                                            break;
                                        }
                                    }
                                }
                            }
                            break;
                        case MediaType.Video:
                            files = files.Where(f => VideoFileTypes.Contains(Path.GetExtension(f).ToLower())).ToList<string>();
                            break;
                        case MediaType.Custom:
                            files = files.Where(f => CustomFileTypes.Contains(Path.GetExtension(f).ToLower())).ToList<string>();
                            break;
                    }

                    var rnd = new Random();
                    files = files.OrderBy(x => rnd.Next()).ToList<string>();

                    if (maxLength > 0)
                    {
                        files = files.Take(maxLength).ToList<string>();
                    }

                    if (File.Exists(playlistName))
                    {
                        File.Delete(playlistName);
                    }
                    File.WriteAllLines(playlistName, files);
                    Console.WriteLine($"Successfully created {playlistName}");
                    Console.ReadKey();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
                return;
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine(@"
Generate a random M3U playlist.

Usage:
Random.Playlist.exe [-d folder1 folder2] [-p playlistName] [-r] [-t audio|video] [-m maxLength] [-x types] [-i types] [-h]

  -d    A list of folders to include, separated by spaces.
        If this is not included, the current folder is used.
  -e    A list of directories to exclude, separated by spaces.
  -p    The name of the playlist file. If not specified,
        the playlist will be named after the first folder being searched,
        preceded by the word Random. e.g. Random Music.m3u
  -r    Search subfolders too.
  -t    Specify whether to look for audio or video files. If not specified,
        the first file found will determine if it looks for audio or video
        files. e.g. If it comes across an audio file first, then only audio
        files will be included.
  -m    Limit the playlist to a maximum of this many files. e.g. -m 100
  -x    A list of file extensions to exclude, separated by semicolons.
        e.g. -x m4a;wav
  -i    A list of file extensions to include, separated by semicolons.
        All other files will be ignored. e.g. -i mp3;m4a
  -u    Search playlists and change order of titles in random way
  -h    Show this help screen
");
            //todo: Laurin fragen wegen Adaptierung: Random in vorhandenen Playlisten machen
        }

    }
}
