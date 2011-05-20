using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.ServiceProcess;
using System.Configuration.Install;
using System.Configuration.Assemblies;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using OPCLib;

namespace OPCDatabaseLogger
{
    class Program
    {
        static void Main(string[] args) 
        { 

            string opt = null;
            if (args.Length >= 1) 
            { 
                opt = args[0].ToLower(); 
            } 
            if (opt == "/install" || opt == "/uninstall") 
            { 
                TransactedInstaller ti = new TransactedInstaller();
                MonitorInstaller mi = new MonitorInstaller("OPCFILEWATCHER"); 
                ti.Installers.Add(mi); 
                string path = String.Format("/assemblypath={0}", Assembly.GetExecutingAssembly().Location); 
                string[] cmdline = { path }; 
                InstallContext ctx = new InstallContext("", cmdline); 
                ti.Context = ctx; 
                if (opt == "/install") 
                {
                    Console.WriteLine("Installing"); 
                    ti.Install(new Hashtable()); 
                } 
                else if (opt == "/uninstall") 
                { 
                    Console.WriteLine("Uninstalling"); 
                        try { ti.Uninstall(null); } catch (InstallException ie) { Console.WriteLine(ie.ToString()); } 
                } 
            }
            else 
            { 
                ServiceBase[] services;
                services = new ServiceBase[] { new OPCDataParser() }; ServiceBase.Run(services); 
            }
        } 

    }

    [RunInstaller(true)]
    public class MonitorInstaller : Installer 
    {
        
        public MonitorInstaller(string service_name) { 
            ServiceProcessInstaller spi = new ServiceProcessInstaller(); 
            spi.Account = ServiceAccount.LocalSystem;
            //spi.Password = "";
          //  spi.Username = "Line1";
            ServiceInstaller si = new ServiceInstaller(); 
            si.ServiceName = service_name; 
            si.StartType = ServiceStartMode.Automatic;
            si.Description = "OPCFILEWATCHER";
            si.DisplayName = "OPC File Parser and Database Loader"; 
            this.Installers.Add(spi); 
            this.Installers.Add(si); 
        } 
    } 
}
     