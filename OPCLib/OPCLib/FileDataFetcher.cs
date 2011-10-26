using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Threading;

namespace OPCLib
{
    public class FileDataFetcher
    {
        static MySqlConnection Conn = AQT_Database.GetMYSQLConnection();
        static int MaxNoUpdateCycles = 7;
        static int CyclesPerHDTableWrite = 5;
        static int CyclesCounter = 0;
        //this depends on mysql installation
        static string inFilePath = @"C:\ProgramData\MySQL\MySQL Server 5.5\Data\aqt\temp.csv";
        static string HDTablePath = @"C:\ProgramData\MySQL\MySQL Server 5.5\Data\aqt\HDTable_temp.csv";
        static Mutex block = new Mutex();
        
        public static void ParseFile(string file)
        {
              //WaitCallback cb = new WaitCallback(parseFile);
              //ThreadPool.QueueUserWorkItem(cb, file);
              //File
                block.WaitOne();
                parseFile(file);
                block.ReleaseMutex();
        }
        private static void parseFile(string file)
        {

            string[] FileLines = File.ReadAllLines(file);
            File.Delete(file);
            Console.WriteLine("deleted file: " + file);
            string[] HeaderNames = FileLines[0].Split(',');
        
            int MySQLInsertSuccesses = 0;
            int MySQLInsertFailures = 0;
            int ParseSuccesses = 0;
            int ParseFailures = 0;
            int TotalRecordsToInsert = 0;
            int UnchangedFields = 0;
            int RelalignedFields = 0;
            StreamWriter tempFile = new StreamWriter(inFilePath);
            StreamWriter HDFile = new StreamWriter(HDTablePath , true);
            try
            {
                
                string[] line = HeaderNames.ToArray();
                TotalRecordsToInsert = line.Length - 2;//2 fields for date and time
                List<string> OPCFieldNames = new List<string>();

                for (int i = 2; i < line.Length; i++)
                {
                    OPCFieldNames.Add( line[i]);
                }
                List<OPCField> FieldInfos = new List<OPCField>();

                for (int i = 0; i < OPCFieldNames.Count; i++)
                {
                    OPCField F = AQT_Database.GetFieldInfo(OPCFieldNames[i], Conn);
                   // F.Name = OPCFieldNames[i];
                    if (F == null)
                    {
                       Console.WriteLine("Field not found in database: " + OPCFieldNames[i]);
                    }
                    FieldInfos.Add(F);
                }

                DateTime time;

                for (int zz = 1; zz < FileLines.Length; zz++)
                {
                    line = FileLines[zz].Split(',');
                    time = DateTime.Parse(line[0] + " " + line[1]);
                    int i = 2;

                    Console.WriteLine(string.Format("{0}", zz));
                    while (i < line.Length)
                    {
                        OPCField CurrentField = FieldInfos[i - 2];
                        if (CurrentField == null)
                        {
                            Console.WriteLine("found null field: ");
                            continue;
                        }
                        try
                        {
                            float val;

                            if (CurrentField.Type.Contains("BOOLEAN"))//BOOLEAN FIELD --> Text file has false in it
                            {
                                if(line[i].Contains("Fa"))
                                {
                                    val = 0;
                                }
                                else if (line[i].Contains("Tr"))
                                {
                                    val = 1;
                                }
                                else
                                {
                                    val = 0;
                                    throw new Exception("Unparsible value");
                                }
                            }
                            else
                            {
                                val = float.Parse(line[i]);
                            }

                            ParseSuccesses++;

                            val /= CurrentField.Scale;
                            if (val == CurrentField.LastValue && CurrentField.LastUpdate < MaxNoUpdateCycles)
                            {
                                CurrentField.LastUpdate++;
                                UnchangedFields++;
                            }
                            else 
                            {
                                if (CurrentField.LastUpdate >= MaxNoUpdateCycles)
                                {
                                    RelalignedFields++;
                                }
                                AQT_Database.WriteToFile(tempFile, CurrentField, val, time);
                                AQT_Database.WriteToFile(HDFile, CurrentField, val, time);
                                CurrentField.LastValue = val;
                                CurrentField.LastUpdate = 0;
                                CurrentField.LastUpdateTime = DateTime.Now;
                            }
                        }
                        catch (Exception ex)
                        {
                            ParseFailures++;
                        }
                        i++;
                    }
                }
                tempFile.Close();
                HDFile.Close();

                if (Conn.State != System.Data.ConnectionState.Open)
                {
                    Conn.Open();
                }
                try
                {
                    if (CyclesCounter > CyclesPerHDTableWrite)
                    {
                        CyclesCounter = 0;
                        
                        MySQLInsertSuccesses = AQT_Database.WriteInFileToDatabase(Conn, Path.GetFileName( HDTablePath ) );
                        Console.WriteLine("Wrote To HD Table");
                        File.Delete(HDTablePath);
                        Console.WriteLine("deleted file: " + HDTablePath);
                    }
                    else
                    {
                        CyclesCounter++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                try
                {
                    MySQLInsertSuccesses =  AQT_Database.WriteInFileToDatabaseMemTable(Conn,  Path.GetFileName(inFilePath) );
                }              
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                MySQLInsertFailures = TotalRecordsToInsert - MySQLInsertSuccesses;
                Console.WriteLine(string.Format("Inserted {0} records out of {1} @ {2} on {3}. {4} unchanged fields. {5} Realigned fields", MySQLInsertSuccesses, TotalRecordsToInsert, DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString(), UnchangedFields, RelalignedFields));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Conn.Close();
                tempFile.Close();
                HDFile.Close();
                File.Delete(inFilePath);
            }
            
        }
    }
}
