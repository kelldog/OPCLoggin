using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MySql.Data.MySqlClient;


namespace GenerateCFGFile
{
    class Program
    {
        public static string connStr = "server=localhost;user=root;database=AQT;port=3306;password=aqt;";

        static void Main(string[] args)
        {
            StreamReader Queries = new StreamReader(args[0]);
            StreamWriter outFile = new StreamWriter(args[1]);
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
                   // Mysql
                    MySqlDataReader reader = c.ExecuteReader();
                    List<string> Names = new List<string>();
                    while (reader.Read())
                    {
                        Names.Add(reader.GetString(0));
                    }
                    reader.Close();

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
