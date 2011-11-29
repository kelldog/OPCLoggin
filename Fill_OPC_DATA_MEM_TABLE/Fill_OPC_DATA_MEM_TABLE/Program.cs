using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.Common;
using MySql.Data.MySqlClient;
using OPCLib;

namespace Fill_OPC_DATA_MEM_TABLE
{
    class Program
    {
        static void Main(string[] args)
        {

            MySqlConnection Conn = OPCLib.AQT_Database.GetMYSQLConnection();
            DateTime ToCopyFrom = DateTime.Now;
            int HowManyDaysBack = 30;
            try
            {
                HowManyDaysBack = int.Parse(args[0]);
            }
            catch
            {

            }

            //TimeSpan TimeStep = new TimeSpan(1,0,0,0,0);
            DateTime LastTime = DateTime.Now;
            try
            {
                Conn.Open();
                for (int i = 0; i <= HowManyDaysBack; i++)
                {

                   // if(Conn.State == MysqlCon
                    ToCopyFrom = ToCopyFrom.Subtract(new TimeSpan(1, 0, 0, 0, 0));

                    int RecordsCopied = OPCLib.AQT_Database.FILL_MEMORY_TABLE_FROM(Conn, ToCopyFrom, LastTime);
                    Console.WriteLine(string.Format("Copied {0} records from {1} To opc_data_mem TABLE", RecordsCopied, ToCopyFrom));
                    LastTime = ToCopyFrom;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Conn.Close();
            }
            Console.ReadLine();
        }
    }
}
