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

        //this depends on mysql installation
        static string inFilePath = @"C:\ProgramData\MySQL\MySQL Server 5.5\Data\aqt\temp.csv";

        public static void ParseFile(string file)
        {
              //WaitCallback cb = new WaitCallback(parseFile);
              //ThreadPool.QueueUserWorkItem(cb, file);
              //File
              parseFile(file);
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
            StreamWriter tempFile = new StreamWriter(inFilePath);

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
                        try
                        {
                            float val;
                            if (line[i].Contains("Fa"))//BOOLEAN FIELD --> Text file has false in it
                            {
                                val = 0;
                            }
                            else if (line[i].Contains("Tr"))//BOOLEAN FIELD --> Text File has true in it
                            {
                                val = 1;
                            }
                            else
                            {
                                val = float.Parse(line[i]);
                            }

                            ParseSuccesses++;

                            if (FieldInfos[i - 2] != null)
                            {
                                val /= FieldInfos[i - 2].Scale;
                                AQT_Database.WriteToFile(tempFile, FieldInfos[i - 2], val, time);
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

                if (Conn.State != System.Data.ConnectionState.Open)
                {
                    Conn.Open();
                }
                try
                {
                    MySQLInsertSuccesses = AQT_Database.WriteInFileToDatabase(Conn, inFilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                try
                {
                    MySQLInsertSuccesses =  AQT_Database.WriteInFileToDatabaseMemTable(Conn, inFilePath);
                }              
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                MySQLInsertFailures = TotalRecordsToInsert - MySQLInsertSuccesses;
                Console.WriteLine(string.Format("Inserted {0} records out of {1} @ {2} on {3}", MySQLInsertSuccesses, TotalRecordsToInsert,DateTime.Now.ToLongTimeString() , DateTime.Now.ToLongDateString()));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Conn.Close();
                tempFile.Close();
                File.Delete(inFilePath);
            }
            
        }
    }
}
