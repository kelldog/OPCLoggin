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

        public static void ParseFile(string file)
        {
            WaitCallback cb = new WaitCallback(parseFile);
            ThreadPool.QueueUserWorkItem(cb, file);
            
        }
        private static void parseFile(object f)
        {
            string file = (string)f;
            StreamReader reader = new StreamReader(file);
            List<string> lines = new List<string>();

            List<string> HeaderNames = new List<string>();
            HeaderNames.Add("");
            int u = 0;
            bool FirstLine = true;
            char nextread = (char)reader.Read();
            while (FirstLine)
            {
                if (nextread == '\n')
                {
                    FirstLine = false;
                    break;
                    
                }
                else if (nextread == ',')
                {
                    HeaderNames.Add("");
                    u++;
                }
                else
                {
                    HeaderNames[u] += nextread;
                }
                nextread = (char)reader.Read();
            }
            
            while (!reader.EndOfStream)
            {
                lines.Add(reader.ReadLine());
            }
            reader.Close();
            File.Delete(file);
            Console.WriteLine("deleted file: "+file);

            MySqlConnection Conn = AQT_Database.GetMYSQLConnection();
            int MySQLInsertSuccesses = 0;
            int MySQLInsertFailures = 0;
            int ParseSuccesses = 0;
            int ParseFailures = 0;

            //this depends on mysql installation
            string inFilePath = @"C:\ProgramData\MySQL\MySQL Server 5.5\Data\aqt\temp.csv";
            StreamWriter tempFile = new StreamWriter(inFilePath);

            try
            {
                
                string[] line = HeaderNames.ToArray();

                List<string> OPCFieldNames = new List<string>();

                for (int i = 2; i < line.Length; i++)
                {
                    OPCFieldNames.Add(line[i]);
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

                int zz = 0;

                Console.WriteLine(string.Format("lines to parse: {0} Fields per lines {1}", lines.Count(),OPCFieldNames.Count()));
               
                while (zz < lines.Count)
                {
                    line = lines[zz].Split(',');
                    
                    time = DateTime.Parse(line[0] + " " + line[1]);
                    int i = 2;
                    Console.WriteLine(string.Format("{0}",zz));
                    while (i < line.Length)
                    {
                        try
                        {
                            float val;
                            if (line[i].Contains("Fa") )
                            {
                                val = 0;
                            }
                            else if (line[i].Contains("Tr"))
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
                                try
                                {
                                    val *= FieldInfos[i - 2].Scale;
                                    AQT_Database.WriteToFile(tempFile, FieldInfos[i - 2], val, time);

                                    //AQT_Database.Load_To_AQT_Database(FieldInfos[i - 2], val, time, Conn);
                                    MySQLInsertSuccesses++;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    MySQLInsertFailures++;
                                }
                            }
                            else
                            {
                                MySQLInsertFailures++;
                            }
                        }
                        catch(Exception ex)
                        {
                            ParseFailures++;
                        }
                        i++;
                    }
                    zz++;
                }

                tempFile.Close();

                if (Conn.State == System.Data.ConnectionState.Closed)
                {
                    Conn.Open();
                }

                try
                {
                    AQT_Database.WriteInFileToDatabase(Conn, inFilePath);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                try
                {
                    AQT_Database.WriteInFileToDatabaseMemTable(Conn, inFilePath);
                }              
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                Conn.Close();
                Console.WriteLine(string.Format("Entries Parsed: {0}  Failed Parsed Entries: {1}   MySQL Successes: {2}  MySQL Failures: {3}", ParseSuccesses, ParseFailures, MySQLInsertSuccesses, MySQLInsertFailures));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                reader.Close();
                Conn.Close();
                tempFile.Close();
                File.Delete(inFilePath);

            }
        }
    }
}
