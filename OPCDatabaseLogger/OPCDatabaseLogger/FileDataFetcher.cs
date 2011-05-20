using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MySql.Data;
using MySql.Data.MySqlClient;



namespace OPCDatabaseLogger
{
    public class FileDataFetcher
    {

        public static void ParseFile(string file)
        {
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
            Console.WriteLine("deleted");

            MySqlConnection Conn = AQT_Database.GetMYSQLConnection();
            int MySQLInsertSuccesses = 0;
            int MySQLInsertFailures = 0;
            int ParseSuccesses = 0;
            int ParseFailures = 0;

            try
            {
                Conn.Open();
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
                while (zz < lines.Count)
                {
                    line = lines[zz].Split(',');
                    time = DateTime.Parse(line[0] + " " + line[1]);
                    int i = 2;

                    while (i < line.Length)
                    {
                        try
                        {
                            float val = float.Parse(line[i]);
                            ParseSuccesses++;
                            if (FieldInfos[i - 2] != null)
                            {
                                try
                                {
                                    val *= FieldInfos[i - 2].Scale;
                                    AQT_Database.Load_To_AQT_Database(FieldInfos[i - 2], val, time, Conn);
                                    MySQLInsertSuccesses++;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    MySQLInsertFailures++;
                                }
                                
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
            }
        }
    }
}
