using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace OPCLib
{
    public class AQT_Database
    {

        public static string connStr = "server=localhost;user=root;database=AQT;port=3306;password=aqt;";
        public static Dictionary<string, OPCField> CachedFields = new Dictionary<string, OPCField>();
        public static void Load_To_AQT_Database(OPCField F, float Value, DateTime time, MySqlConnection conn)
        {
            MySqlCommand c = new MySqlCommand("INSERT INTO opc_data (Value,Time,ID) VALUES (@val,@t,@id)", conn);
            c.Parameters.Add(FillP("@val", Value, MySqlDbType.Float));
            c.Parameters.Add(FillP("@id", F.ID, MySqlDbType.Int32));
            c.Parameters.Add(FillP("@t", time, MySqlDbType.DateTime));
            //c.Parameters.Add(FillParam("@st", d.fieldinfo.StationInfo.StationID, conn, System.Data.SqlDbType.TinyInt));
            c.ExecuteNonQuery();
        }

        public static void WriteInFileToDatabase(MySqlConnection conn,string inFile)
        {
            string command = string.Format("LOAD DATA INFILE \'temp.csv\' INTO TABLE opc_data \n FIELDS TERMINATED BY \',\' \n LINES TERMINATED BY \'\\r\\n\';" , inFile );
            MySqlCommand c = new MySqlCommand( command , conn);
            c.ExecuteNonQuery();
        }
        public static void WriteToFile(StreamWriter fileout,OPCField F, float Value, DateTime Time)
        {
	        string time = string.Format("{0}-{1}-{2} {3}:{4}:{5}.{6}", Time.Year, Time.Month, Time.Day, Time.Hour, Time.Minute, Time.Second, Time.Millisecond);
            fileout.Write(string.Format("{0},{1},{2}\r\n", F.ID, time, Value));
        }
        public static MySqlConnection GetMYSQLConnection()
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            return conn;
        }
        public void InsertnewOPCField(OPCField F, MySqlConnection conn)
        {
            throw new Exception("field not in table lookup");
        }

        public static OPCField GetFieldInfo(string Name, MySqlConnection conn)
        {
           
            Name = Name.TrimEnd('\r', ' ', '\n');
            Name = Name.TrimStart('\r', ' ', '\n');
            string querry = "SELECT * FROM aqt_fields WHERE Name='" + Name+"'";
           // Console.WriteLine(querry);
            MySqlCommand c = new MySqlCommand(querry, conn);
            if (CachedFields.ContainsKey(Name))
            {
                return CachedFields[Name];
            }
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
            OPCField Field = null;
            MySqlDataReader reader = c.ExecuteReader();

            try
            {
                if (reader.Read())
                {
                    
                    Field = new OPCField();
                    Field.ID = reader.GetInt16("ID");
                    Field.Name = (string)reader["Name"];
                    Field.Scale = reader.GetFloat("Scale");
                }
            }
            finally
            {
                reader.Close();
            }

            CachedFields.Add(Field.Name, Field);
            return Field;
        }


        public static MySqlParameter FillP(string paramName, object value, MySqlDbType dbtype)
        {
            MySqlParameter p = new MySqlParameter(paramName, dbtype);
            p.Value = value;
            return p;
        }
    }
}
