using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.OleDb;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Threading;

using System.Diagnostics;
using MySql.Data.MySqlClient;



namespace DatabaseBuilder
{

    public class OPCStationInfo
    {
        public short StationID;
        public short ChamberTypeID;
        public short StationTypeID;
    }

    public class AQT_Database
    {
        public static void Load_To_AQT_Database(OPCField F, float Value, DateTime time, MySqlConnection conn)
        {
            MySqlCommand c = new MySqlCommand("INSERT INTO opc_data (Value,Time,ID) VALUES (@val,@t,@id)", conn);
            c.Parameters.Add(FillP("@val", Value, MySqlDbType.Float));
            c.Parameters.Add(FillP("@id", F.ID, MySqlDbType.Int32));
            c.Parameters.Add(FillP("@t", time, MySqlDbType.DateTime));
            //c.Parameters.Add(FillParam("@st", d.fieldinfo.StationInfo.StationID, conn, System.Data.SqlDbType.TinyInt));
            c.ExecuteNonQuery();
        }

        public static void FieldInsert(OPCField F, MySqlConnection Conn)
        {
            MySqlCommand c = new MySqlCommand("INSERT INTO fields (Scale,Name,StationID,StationTypeID,ChamberTypeID, Type,Units) VALUES (@sc, @nm, @SID, @STID,@CHTID,@TI,@Un)", Conn);
            c.Parameters.Add(FillP( "@sc", F.Scale, MySqlDbType.Float ));
            c.Parameters.Add(FillP( "@nm", F.Name, MySqlDbType.VarChar ));
            c.Parameters.Add(FillP( "@SID", F.StationID, MySqlDbType.Int32 ));
            c.Parameters.Add(FillP( "@STID", F.StationTypeID, MySqlDbType.Int32 ));
            c.Parameters.Add(FillP( "@CHTID", F.ChamberTypeID, MySqlDbType.Int32 ));
            c.Parameters.Add(FillP("@TI", F.Type, MySqlDbType.VarChar));
            c.Parameters.Add(FillP("@Un", F.Units, MySqlDbType.VarChar));
            //UNITS
            //Comments
            //AQT_Name

            c.ExecuteNonQuery();
        }

        public static MySqlConnection GetMYSQLConnection()
        {
            MySqlConnection conn = new MySqlConnection(Program.connStr);
            return conn;
        }
        public void InsertnewOPCField(OPCField_ex F, MySqlConnection conn)
        {
            MySqlCommand c = new MySqlCommand("INSERT into fields ( Name ) VALUES (  ) ", conn);

        }

        public static OPCField GetFieldInfo(string Name, MySqlConnection conn)
        {
            MySqlCommand c = new MySqlCommand("SELECT * from fields WHERE Name =" + Name, conn);
            OPCField Field = null;

            MySqlDataReader reader = c.ExecuteReader();

            if (reader.Read())
            {
                Field = new OPCField();
                Field.ID = (short)reader["ID"];
                Field.Name = (string)reader["Name"];
                Field.Scale = (float)reader["Scale"];
            }
            return Field;
        }


        public static MySqlParameter FillP(string paramName, object value, MySqlDbType dbtype)
        {
            MySqlParameter p = new MySqlParameter(paramName, dbtype);
            p.Value = value;
            return p;
        }
    }

    public class Program
    {

        public static string connStr = "server=localhost;user=root;database=aqt;port=3306;password=aqt;";

        public static string ToolServerF = "Provider=Microsoft.JET.OLEDB.4.0;" + "data source={0}";
        public static string CellWorkF = "Provider=Microsoft.JET.OLEDB.4.0;" + "data source={0}";
        public static string ToolServer, CellWork;
        public static List<OPCStationInfo> StationInfoList;

        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                ToolServer = string.Format(ToolServerF, @"C:\Users\administrator.AQTSOLAR\Desktop\Intevac_Access_Databases\ToolServer.mdb");
                CellWork = string.Format(CellWorkF, @"C:\Users\administrator.AQTSOLAR\Desktop\Intevac_Access_Databases\CellWork.mdb");
            }
            else
            {
                ToolServer = string.Format(ToolServerF, args[0]);
                CellWork = string.Format(CellWorkF, args[1]);
            }

            Console.WriteLine(string.Format("using Tool Server File: {0}",ToolServer ));
            Console.WriteLine(string.Format("using Cell Work File: {0}", CellWork)) ;

            Thread.Sleep(1000);

            List<OPCField_ex> OPCFields = new List<OPCField_ex>();

            //Get the list of stations and the cahmber type for each station
             StationInfoList = GetDistinctStations(CellWork);

            //add all available OPC fields for each station
           // foreach (OPCStationInfo station in StationInfoList)
           // {
           //     OPCFields.AddRange(ProcessDataQuery(station, ToolServer));
           // }

            GetFromItemTable( OPCFields , ToolServer );

            MySqlConnection Conn = AQT_Database.GetMYSQLConnection();
            Conn.Open();
            int StationID = -1;

            foreach (OPCField_ex o in OPCFields)
            {
                OPCField F = new OPCField();
                F.StationID = StationID;
                F.StationTypeID = StationID;
                F.ChamberTypeID = StationID;
                if (o.StationInfo != null)
                {
                    F.StationID = o.StationInfo.StationID;
                    F.StationTypeID = o.StationInfo.StationTypeID;
                    F.ChamberTypeID = o.StationInfo.ChamberTypeID;
                }
                F.Type = o.Type;
                F.Name = o.OPC_Lookup;
                F.Scale = o.ScaleFactor;
                F.Units = o.units;
                Console.WriteLine("added: " + F.Name);
                AQT_Database.FieldInsert(F, Conn);
            }
            Conn.Close();
        }

        public static List<OPCStationInfo> GetDistinctStations(string source)
        {
            List<OPCStationInfo> AvailableStations = new List<OPCStationInfo>();
            //string source = CellWork;
            OleDbConnection mConnection = new OleDbConnection(source);
            try
            {
                OleDbCommand mCommand = new OleDbCommand("SELECT StationID,ChamberTypeID,StationTypeID from StationParameters");
                mCommand.CommandType = CommandType.Text;
                mConnection.Open();
                mCommand.Connection = mConnection;
                OleDbDataReader mReader;
                mReader = mCommand.ExecuteReader();

                while (mReader.Read())
                {
                    OPCStationInfo new_station = new OPCStationInfo();
                    new_station.StationID = mReader.GetInt16(0);
                    new_station.ChamberTypeID = mReader.GetInt16(1);
                    new_station.StationTypeID = mReader.GetInt16(2);
                    AvailableStations.Add(new_station);
                }
                mReader.Close();
            }
            catch (Exception ex)
            {
                throw ex;
                //Program.ErrorLog.WriteEntry(string.Format("Parse Error From pulling Station Info.  Station ID: {0} ", ex.Message));
            }
            finally
            {
                mConnection.Close();
            }

            return AvailableStations;
        }

        public static List<OPCField_ex> ProcessDataQuery(OPCStationInfo Station, string ToolServer)
        {

            OleDbCommand mCommand = new OleDbCommand("ProcessDataQuery");
            mCommand.CommandType = CommandType.StoredProcedure;
            OleDbConnection mConnection = new OleDbConnection(ToolServer);
            OleDbParameter IdIn = new OleDbParameter("@station", OleDbType.Integer);
            IdIn.Value = Station.StationID;
            mCommand.Parameters.Add(IdIn);
            mConnection.Open();
            mCommand.Connection = mConnection;
            OleDbDataReader mReader;
            mReader = mCommand.ExecuteReader();
            List<OPCField_ex> fields = new List<OPCField_ex>();

            while (mReader.Read())
            {
                OPCField_ex f = new OPCField_ex();
                f.StationInfo = Station;
                f.ScaleFactor = mReader.GetFloat(11);
                f.ClientTopicID = mReader.GetString(2);
                f.ClientItemID = mReader.GetString(1);
                f.DeviceType = (short)mReader.GetInt32(0);
                f.OPCItemID = mReader.GetString(3);
                f.RequestFromOPC = false;
                f.OPCprefix = mReader.GetString(4);
                f.OPCTopicID = mReader.GetString(5);
                //f.Type = (Int32)mReader["DataType"];********************************
                try
                {
                  //  f.DataType = GetTypeFromNum((Int32)mReader["DataType"]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message+"  "+mReader.GetString(9));
                }
               // if (f.DataType != typeof(float))
               // {

               // }
                fields.Add(f);
            }

            mReader.Close();
            mConnection.Close();
            return fields;
        }

        public static void GetFromItemTable(List<OPCField_ex> OPCFields, string ToolServer)
        {

            Dictionary<int,string> UnitLookup = new Dictionary<int,string>();
            Dictionary<int, string> TypeLookup = new Dictionary<int, string>();


            OleDbCommand mCommand = new OleDbCommand("SELECT * FROM Items WHERE 1");
            mCommand.CommandType = CommandType.Text;
            OleDbConnection mConnection = new OleDbConnection(ToolServer);
            mConnection.Open();
            mCommand.Connection = mConnection;

            OleDbCommand FillDictionary = new OleDbCommand("SELECT UnitType,UnitTypeName FROM UnitType WHERE 1");
            FillDictionary.CommandType = CommandType.Text;
            FillDictionary.Connection = mConnection;
            OleDbDataReader FillReader = FillDictionary.ExecuteReader();

            while(FillReader.Read())
            {
                UnitLookup.Add((int)FillReader[0],(string)FillReader[1]);
            }

            FillReader.Close();


            OleDbCommand FillDictionaryType = new OleDbCommand("SELECT DataType,DataTypeName FROM DataType WHERE 1");
            FillDictionaryType.CommandType = CommandType.Text;
            FillDictionaryType.Connection = mConnection;
            OleDbDataReader FillReaderType = FillDictionaryType.ExecuteReader();

            while (FillReaderType.Read())
            {
                TypeLookup.Add((int)FillReaderType[0], (string)FillReaderType[1]);
            }

            FillReaderType.Close();

            OleDbDataReader mReader;
            mReader = mCommand.ExecuteReader();
            List<OPCField_ex> fields = new List<OPCField_ex>();
            int allreadyhave = 0;

            Console.Write("enumerating all opc items to pull OPC Scale Factor");

            try
            {
                while (mReader.Read())
                {
                    OPCField_ex f = new OPCField_ex();

                    f.ClientTopicID = (string)mReader["ClientTopicID"];

                    string test = f.ClientTopicID;
                    Debug.WriteLine(test);
                    if (!test.Contains("STATION"))
                    {
                        continue;
                    }

                   // f.DataType =
                    f.Type = TypeLookup[(int)mReader["DataType"]];
                    // Console.Write(".");
                    f.ScaleFactor = (float)mReader["OPCScaleFactor"];

                    if(  mReader["UnitType"].GetType() == typeof(System.DBNull) )
                    {
 
                    }
                    else
                    {
                        int lookup_unit = (int)mReader["UnitType"];

                        f.units = UnitLookup[lookup_unit];
                    }

                    string number = f.ClientTopicID.Substring(f.ClientTopicID.IndexOf("STATION") + 7, 2);
                    int n = int.Parse(number);

                    if (n < 99 && n < 0)
                    {
                        throw new Exception();
                    }

                    f.StationInfo = GetStationFromNumber(n);

                    f.ClientItemID = (string)mReader["ClientItemID"];
                    try
                    {
                        f.OPCItemID = (string)mReader["OPCItemID"];
                    }
                    catch
                    {
                        continue;
                    }
                    try
                    {
                        f.OPCprefix = (string)mReader["OPCPrefix"];
                    }
                    catch
                    {

                    }
                    try
                    {
                        f.OPCTopicID = (string)mReader["OPCTopicID"];
                    }
                    catch
                    {

                    }
                   // OPCField_ex AlreadExist = OPCFields.Where(b => b.OPC_Lookup == f.OPC_Lookup).First();
                   // if (AlreadExist == null)
                  //  {
                        OPCFields.Add(f);
                        string stationT = "NULL";
                        if(f.StationInfo != null)
                        {
                            stationT = f.StationInfo.StationID.ToString();
                        }
                        Console.WriteLine(string.Format("Added field: {0} scale {1} station {2}",f.OPC_Lookup,f.ScaleFactor,stationT));
                  //  }
                   // else
                  //  {
                   //     allreadyhave++;
                   //     if (AlreadExist.ScaleFactor != f.ScaleFactor)
                   //     {

                    //    }
                   // }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                mReader.Close();
                mConnection.Close();
            }
        }

        public static OPCStationInfo GetStationFromNumber(int number)
        {
            OPCStationInfo looked_up = null;
            looked_up = StationInfoList.Where(n => n.StationID == number).First();
            return looked_up;
        }

        public static string GetTypeStringFromNum(int num)
        {
            switch(num)
            {
                case 2:	return "SHORT";
                case 3:	return "LONG";
                case 4:	return "FLOAT";
                case 5:	return "DOUBLE";
                case 8:	return "STRING";
                case 11:return "BOOLEAN";
                case 16:return "CHAR";
                case 17:return "BYTE";
                case 18:return "WORD";
                case 19:return "DWORD";
            }
            return "NULL";
        }

        public static Type GetTypeFromNum(int num)
        {
            switch (num)
            {
                case 2: return typeof(short);
                case 3: return typeof(long);
                case 4: return typeof(float);
                case 5: return typeof(double);
                case 8: return typeof(string);
                case 11: return typeof(bool);
                case 16: return typeof(char);
                case 17: return typeof(byte);
                case 18: return typeof(int);
                case 19: return typeof(int);
            }
            return null;
        }


    }
}
