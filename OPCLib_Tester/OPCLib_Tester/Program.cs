using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Configuration.Assemblies;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using OPCLib;


namespace OPCLib_Tester
{
    class Program
    {
        public static bool Running = true;
        public static FileSystemWatcher watcher;

        static bool firstPass = true;

        static void Main(string[] args)
        {
          //  OPCcode = Assembly.LoadFrom(@"C:\Users\administrator.AQTSOLAR\Desktop\Intevac_Logging_Files\OPCLib.dll");
         //   T = OPCcode.GetType("OPCLib.FileDataFetcher");
          //  AO = Activator.CreateInstance(T);
          //  ParseMethod = T.GetMethod("ParseFile");
         //   FileDataFetcher fetcher = FileDataFetcher.ParseFile(
            watcher = new FileSystemWatcher(@"C:\OPCDataLogger\OPCDataLogger\Log Data", "*.csv");
            watcher.Changed += new FileSystemEventHandler(watcher_Changed);
            watcher.EnableRaisingEvents = true;

            while (Running)
            {
                Thread.Sleep(300);
            }
        }

        static void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (firstPass)
            {//blow the first pass file away since it could be too long to process in time
                File.Delete(e.FullPath);
                firstPass = false;
                return;
            }
            if (e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Created)
            {
                Console.WriteLine("Log file Change event detected..." + e.FullPath);
                watcher.EnableRaisingEvents = false;
                FileDataFetcher.ParseFile( e.FullPath );
                watcher.EnableRaisingEvents = true;
            }
        }
    }
}
