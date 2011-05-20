using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.ServiceProcess;
using System.Configuration.Assemblies;
using System.Reflection;
using System.Collections;
using System.ComponentModel;

namespace OPCDatabaseLogger
{
    public class OPCDataParser : ServiceBase
    {
        public static FileSystemWatcher watcher;
        public static Assembly OPCcode;
        public static Type T;

        public static object AO;
        public static MethodInfo ParseMethod;

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Created)
            {
                Console.WriteLine("Log file Change event detected..." + e.FullPath);
                watcher.EnableRaisingEvents = false;
                ParseMethod.Invoke(AO, new object[] { e.FullPath });
                watcher.EnableRaisingEvents = true;
            }
        }
        public OPCDataParser()
        {
            this.ServiceName = "OPC_FILE_WATCHER";
            this.EventLog.Log = "Application";
            this.CanHandlePowerEvent = true;
            this.CanHandleSessionChangeEvent = true;
            this.CanPauseAndContinue = true;
            this.CanShutdown = true;
            this.CanStop = true;

            OPCcode = Assembly.LoadFrom(@"C:\OPCLib\OPCLib.dll");
            T = OPCcode.GetType("OPCLib.FileDataFetcher");
            AO = Activator.CreateInstance(T);
            ParseMethod = T.GetMethod("ParseFile");

            watcher = new FileSystemWatcher(@"C:\OPCDataLogger\OPCDataLogger\Log Data", "*.csv");
            watcher.EnableRaisingEvents = false;
            watcher.Changed += new FileSystemEventHandler(watcher_Changed);



            watcher.EnableRaisingEvents = true;

        }

        /// <summary>
        /// The Main Thread: This is where your Service is Run.
        /// </summary>
        static void Main()
        {
            ServiceBase.Run(new OPCDataParser());
        }

        /// <summary>
        /// Dispose of objects that need it here.
        /// </summary>
        /// <param name="disposing">Whether
        ///    or not disposing is going on.</param>
        protected override void Dispose(bool disposing)
        {
            watcher.EnableRaisingEvents = false ;
            base.Dispose(disposing);
        }

        /// <summary>
        /// OnStart(): Put startup code here
        ///  - Start threads, get inital data, etc.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// OnStop(): Put your stop code here
        /// - Stop threads, set final data, etc.
        /// </summary>
        protected override void OnStop()
        {
            watcher.EnableRaisingEvents = false;
            base.OnStop();
        }

        /// <summary>
        /// OnPause: Put your pause code here
        /// - Pause working threads, etc.
        /// </summary>
        protected override void OnPause()
        {
            watcher.EnableRaisingEvents = false;
            base.OnPause();
        }

        /// <summary>
        /// OnContinue(): Put your continue code here
        /// - Un-pause working threads, etc.
        /// </summary>
        protected override void OnContinue()
        {
            watcher.EnableRaisingEvents = true ;
            base.OnContinue();
        }

        /// <summary>
        /// OnShutdown(): Called when the System is shutting down
        /// - Put code here when you need special handling
        ///   of code that deals with a system shutdown, such
        ///   as saving special data before shutdown.
        /// </summary>
        protected override void OnShutdown()
        {
            watcher.EnableRaisingEvents = false;
            base.OnShutdown();
        }

        /// <summary>
        /// OnCustomCommand(): If you need to send a command to your
        ///   service without the need for Remoting or Sockets, use
        ///   this method to do custom methods.
        /// </summary>
        /// <param name="command">Arbitrary Integer between 128 & 256</param>
        protected override void OnCustomCommand(int command)
        {
            //  A custom command can be sent to a service by using this method:
            //#  int command = 128; //Some Arbitrary number between 128 & 256
            //#  ServiceController sc = new ServiceController("NameOfService");
            //#  sc.ExecuteCommand(command);

            base.OnCustomCommand(command);
        }

        /// <summary>
        /// OnPowerEvent(): Useful for detecting power status changes,
        ///   such as going into Suspend mode or Low Battery for laptops.
        /// </summary>
        /// <param name="powerStatus">The Power Broadcast Status
        /// (BatteryLow, Suspend, etc.)</param>
        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            return base.OnPowerEvent(powerStatus);
        }

        /// <summary>
        /// OnSessionChange(): To handle a change event
        ///   from a Terminal Server session.
        ///   Useful if you need to determine
        ///   when a user logs in remotely or logs off,
        ///   or when someone logs into the console.
        /// </summary>
        /// <param name="changeDescription">The Session Change
        /// Event that occured.</param>
        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            base.OnSessionChange(changeDescription);
        }

        private void InitializeComponent()
        {

        }
    }
}
    