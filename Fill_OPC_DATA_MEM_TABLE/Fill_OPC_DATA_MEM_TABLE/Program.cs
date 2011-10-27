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
            int HowManyDaysBack = 1;
            try
            {
                HowManyDaysBack = int.Parse(args[0]);
            }
            catch
            {

            }

            ToCopyFrom = ToCopyFrom.Subtract( new TimeSpan(HowManyDaysBack,0,0,0,0) );
            try
            {
                Conn.Open();
                int RecordsCopied = OPCLib.AQT_Database.FILL_MEMORY_TABLE_FROM(Conn, ToCopyFrom, DateTime.Now);
                Console.WriteLine(string.Format("Copied {0} records from {1} To opc_data_mem TABLE", RecordsCopied, ToCopyFrom));
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
