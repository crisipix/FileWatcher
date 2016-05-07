using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Timers;

namespace FileChangeWatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Watch");
            RunWatch();

        }

        // Define other methods and classes here

        private static Dictionary<string, ChangeFileInfo> _fileDictionary = new Dictionary<string, ChangeFileInfo>();

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private static void RunWatch()
        {
            var watcher = new FileSystemWatcher();
            watcher.Path = @"C:\Users\Tiff\Documents\GitHub\FileWatcher\FolderToWatch";
            watcher.Filter = "*.txt";

            // Create a new FileSystemWatcher and set its properties.
            /* Watch for changes in LastAccess and LastWrite times, and
                the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.
            watcher.Filter = "*.txt";

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnFileChanged);
            watcher.Created += new FileSystemEventHandler(OnFileChanged);
            watcher.Deleted += new FileSystemEventHandler(OnFileChanged);

            watcher.EnableRaisingEvents = true;

            // Setup the Timer
            System.Timers.Timer timer = new System.Timers.Timer(10000);
            timer.Elapsed += HandleChangedFiles;
            timer.Start();

            var input = Console.ReadLine();
            while (input != "q")
            {
                if (input == "dir")
                {
                    Print();
                }

                input = Console.ReadLine();
            }
        }

        /// <summary>
        /// Adding changed files to a dictionary, if the event triggers multiple times for the same file
        /// it'll just collect it into this dictionary and keep overwriting existing entries or adding new ones. 
        /// the changed file handler will handle all of the changes after a set amount of time all in one batch.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void OnFileChanged(object sender, FileSystemEventArgs args)
        {
            //Console.WriteLine("Type of change : {0} File To Change {1}", args.ChangeType, args.FullPath);
            var key = Path.GetFileNameWithoutExtension(args.FullPath);// + args.ChangeType;
            if (_fileDictionary.ContainsKey(key))
            {
                _fileDictionary[key] = new ChangeFileInfo { Action = args.ChangeType, FilePath = args.FullPath, Time = DateTime.Now };
            }
            else
            {
                _fileDictionary.Add(key, new ChangeFileInfo { Action = args.ChangeType, FilePath = args.FullPath, Time = DateTime.Now });
            }
        }

        private static void HandleChangedFiles(object sender, ElapsedEventArgs arg)
        {
            Console.WriteLine("HANDLING CHANGED FILES");
            Print();
            _fileDictionary.Clear();
        }

        private static void Print()
        {
            if (!_fileDictionary.Any())
            {
                Console.WriteLine("No Changes");
            }
            foreach (var fileChange in _fileDictionary)
            {
                Console.WriteLine("File : {0} Action : {1} Time : {2:MM/dd/yyyy H:mm:ss zzz}", fileChange.Key, fileChange.Value.Action.ToString(), fileChange.Value.Time);
            }
        }

        public class ChangeFileInfo
        {
            public string FilePath { get; set; }
            public WatcherChangeTypes Action { get; set; }
            public DateTime Time { get; set; }
        }
    }
}
