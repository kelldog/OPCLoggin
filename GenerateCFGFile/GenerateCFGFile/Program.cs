using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MySql.Data.MySqlClient;
using System.Threading;
using System.Diagnostics;

namespace GenerateCFGFile
{
    class Program
    {
        public static string header = @"[Servers]
Server Name=APPLICOM.OPCServer.1 (Intevac-28404)
Server ProgID=APPLICOM.OPCServer.1
Server ReconnectAuto=0
Server ReconnectSecs=30
Server Type=0
Server Machine=Intevac-28404
Group Name=1_sec
Group Id=2
Group UpdateRate=1000
Group TimeBias=0
Group DeadBand=0
Group IsActive=True
Group Log File Name Mode=0
Group Log File Name=C:\OPCDataLogger\OPCDataLogger\Log Data\aqt.csv
Group Log File Name Tag=
Group Log File Creation=0
Group Log File Time Enabled1=True
Group Log File Time1=12:00:00 AM
Group Log File Time Enabled2=False
Group Log File Time2=12:00:00 AM
Group Log File Time Enabled3=False
Group Log File Time3=12:00:00 AM
Group Log File Time Enabled4=False
Group Log File Time4=12:00:00 AM
Group Log File Time Enabled5=False
Group Log File Time5=12:00:00 AM
Group Log File Time Enabled6=False
Group Log File Time6=12:00:00 AM
Group Log File Options=1
Group Log File Counter=1
Group Log DB Table Name=
Group Log DB Data=0
Group Log Remote Control=1
Group Log Remote Start Tag=MODULE0.SYMBOLS.AUTOMATIC_MOTION_START
Group Log Remote Pause Tag=
Group Log Remote Stop Tag=MODULE0.SYMBOLS.AUTOMATIC_MOTION_START
Group Log Remote Start Tag Invert=False
Group Log Remote Pause Tag Invert=False
Group Log Remote Stop Tag Invert=True
Group Log Remote Cycle Tag=
Group Log Based On=Time
Group Log Data Rate=1000
Group Log Trigger=
Group Log Mark=1
Group Log Header=True
Group Log MilliSecs=False
Group Log Item Only=False
Group Log Data Format=0
Group Log State=0
Group Heart Beat Mode=0
Group Heart Beat Tag=
Group Heart Beat Interval=1000
Group Chart Data Rate=1000
Group Web Live Data Mode=0";

        public static string connStr = "server=localhost;user=root;database=aqt;port=3306;password=aqt;";

        static void Main(string[] args)
        {

            bool add_fields_aqt_fields_table = true;

            string outFilePath = "";
            string AQT_Fields_To_Graph_QueryPath = "";
            string UnitQueryFile = "";

            if (args.Length == 2)
            {
                outFilePath = args[1];
                AQT_Fields_To_Graph_QueryPath = args[0];
            }
            else
            {
                outFilePath = @"C:\OPC_BACKEND_CODE\Intevac_Logging_Files\output.txt";
                AQT_Fields_To_Graph_QueryPath = @"C:\OPC_BACKEND_CODE\Intevac_Logging_Files\AQTQuerryV3.txt";
                UnitQueryFile = @"C:\OPC_BACKEND_CODE\Intevac_Logging_Files\UnitsUpdateQuery.txt";
            }

            Console.Write(string.Format("Using outfile: {0}", outFilePath));
            Console.Write(string.Format("Using Query File: {0}", AQT_Fields_To_Graph_QueryPath));
            Console.Write(string.Format("Using Unit Query File: {0}", UnitQueryFile));
           
    
            string[] excluded_setpointValues = new string[]
            {
                "bias_current_stpt"
                ,"bias_voltage_stpt"
                ,"coil_current_side_a_stpt"
                ,"coil_current_side_b_stpt"
                ,"hvps_current_stpt"
                ,"hvps_a_voltage_stpt"
                ,"hvps_b_current_stpt"
                ,"hvps_side_b_stpt"
                ,"hvps_b_voltage_stpt"
                ,"phantom_disk_power_stpt"
                ,"mfc_5_stpt"
                ,"mfc_6_stpt"
                ,"mfc_7_stpt"
                ,"mfc_8_stpt"
            };
                
            Thread.Sleep(1000);
            string[] Queries = File.ReadAllLines(AQT_Fields_To_Graph_QueryPath);
            string[] UnitLines = File.ReadAllLines(UnitQueryFile);           

            StreamWriter outFile = new StreamWriter(outFilePath);
            MySqlConnection conn = new MySqlConnection(Program.connStr);

            List<string> loadedfields = new List<string>();

            try
            {
                conn.Open();
                foreach(string next in Queries)
                {
                    if (!next.Contains("SELECT"))
                    {
                        continue;
                    }

                    MySqlCommand c = new MySqlCommand(next, conn);
                    
                    MySqlDataReader reader = c.ExecuteReader();
                    List<string> Names = new List<string>();
                    List<int> IDs = new List<int>();

                    Trace.WriteLine(next);
                    while (reader.Read())
                    {
                        Names.Add(reader.GetString(0));
                        int ID = reader.GetInt32(1);
                        IDs.Add(ID);
                      
                    }
                    reader.Close();
            //        reader.Dispose();
                    if (IDs.Count == 0)
                    {
                        continue;
                    }

                    for (int k = 0; k < IDs.Count; k++ )
                    {

                        if( Names[k].ToUpper() != Names[k])//watch out for lower case duplicates on the POWER B signal
                        {
                            //int uu= 0;
                            Console.WriteLine("found lower case duplicate: " + Names[k]);
                            continue;
                        }
                        foreach (string sss in excluded_setpointValues)
                        {
                            if (Names[k].ToLower().Contains(sss))
                            {
                                Console.WriteLine("excluded setpoint: " + Names[k]);
                                continue;
                            }
                        }
                        MySqlCommand c2 = new MySqlCommand(string.Format("INSERT INTO aqt_fields (ID,Name,Scale,StationID,StationTypeID,ChamberTypeID,Units,AQT_Name,Type) SELECT ID,Name,Scale,StationID,StationTypeID,ChamberTypeID,Units,AQT_Name,Type FROM fields WHERE ID = {0}", IDs[k]), conn);
                        if (add_fields_aqt_fields_table)
                        {
                            try
                            {       
                                c2.ExecuteNonQuery();
                                Console.WriteLine(string.Format("ID cloned {0}", IDs[k]));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            finally
                            {
                                if (c2 != null)
                                {
                                    c2.Dispose();
                                }
                            }
                        }
                        loadedfields.Add(Names[k]);
                    }

                }

            
                outFile.WriteLine(header);
                foreach (string s in loadedfields)
                {
                    Console.WriteLine("Processing" + s);   
                    outFile.WriteLine(string.Format("Item Name={0}\r\nItem Format Data As=0\r\nItem Format Data As On Label=\r\nItem Format Data As Off Label=\r\nItem Format Data As Num Dec Places=2\r\nItem={0}", s));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                outFile.Close();
                conn.Close();
            }
            if (!add_fields_aqt_fields_table)
            {
                return;
            }
           // /*
            try
            {
                conn.Open();

                foreach (string l in UnitLines)
                {
                    Console.WriteLine("Applying units query: " + l);
                    MySqlCommand c = new MySqlCommand(l, conn);
                    c.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            Console.ReadLine();
            //*/
        }
    }
}
