using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MySql.Data.MySqlClient;
using System.Threading;

namespace GenerateCFGFile
{
    class Program
    {
        public static string connStr = "server=localhost;user=root;database=aqt;port=3306;password=aqt;";

        static void Main(string[] args)
        {

            string outFilePath = "";
            string AQT_Fields_To_Graph_QueryPath = "";

            if (args.Length == 2)
            {
                outFilePath = args[1];
                AQT_Fields_To_Graph_QueryPath = args[0];
            }
            else
            {
                outFilePath = @"C:\Users\administrator.AQTSOLAR\Desktop\Intevac_Logging_Files\output.txt";
                AQT_Fields_To_Graph_QueryPath = @"C:\Users\administrator.AQTSOLAR\Desktop\Intevac_Logging_Files\AQTQuerryV2.txt";
            }

            Console.Write(string.Format("Using outfile: {0}", outFilePath));
            Console.Write(string.Format("Using Query File: {0}", AQT_Fields_To_Graph_QueryPath));
            
            Thread.Sleep(1000);

            StreamReader Queries = new StreamReader(AQT_Fields_To_Graph_QueryPath);
            StreamWriter outFile = new StreamWriter(outFilePath);
            MySqlConnection conn = new MySqlConnection(Program.connStr);

            try
            {
                conn.Open();
                while (!Queries.EndOfStream)
                {
                    string next = Queries.ReadLine();
                    if (!next.Contains("SELECT"))
                    {
                        continue;
                    }

                    MySqlCommand c = new MySqlCommand(next, conn);
                    
                    MySqlDataReader reader = c.ExecuteReader();
                    List<string> Names = new List<string>();
                    List<int> IDs = new List<int>();
                    while (reader.Read())
                    {
                        Names.Add(reader.GetString(0));
                        int ID = reader.GetInt32(1);
                        IDs.Add(ID);
                       

                    }
                    reader.Close();

                    foreach (int id in IDs)
                    {
                        MySqlCommand c2 = new MySqlCommand(string.Format("INSERT INTO aqt_fields (ID,Name,Scale,StationID,StationTypeID,ChamberTypeID,Units,AQT_Name,TypeID) SELECT ID,Name,Scale,StationID,StationTypeID,ChamberTypeID,Units,AQT_Name,TypeID FROM fields WHERE ID = {0}", id), conn);
                        try
                        {
                            c2.ExecuteNonQuery();
                            Console.WriteLine(string.Format("ID cloned {0}", id));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                    foreach (string s in Names)
                    {
                        Console.WriteLine("Processing"+s);
                        outFile.WriteLine(string.Format("Item Name={0}\r\nItem Format Data As=0\r\nItem Format Data As On Label=\r\nItem Format Data As Off Label=\r\nItem Format Data As Num Dec Places=2\r\nItem={0}", s));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                outFile.Close();
                Queries.Close();
                conn.Close();
            }
        }
    }
}
