using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using MySql.Data;
using MySql.Data.MySqlClient;

namespace OPCDatabaseLogger
{
    public class AQT_Database
    {

        public static string connStr = "server=localhost;user=root;database=AQT;port=3306;password=aqt;";

        public static void Load_To_AQT_Database(OPCField F, float Value, DateTime time, MySqlConnection conn)
        {
            MySqlCommand c = new MySqlCommand("INSERT INTO opc_data (Value,Time,ID) VALUES (@val,@t,@id)", conn);
            c.Parameters.Add(FillP("@val", Value, MySqlDbType.Float));
            c.Parameters.Add(FillP("@id", F.ID, MySqlDbType.Int32));
            c.Parameters.Add(FillP("@t", time, MySqlDbType.DateTime));
            //c.Parameters.Add(FillParam("@st", d.fieldinfo.StationInfo.StationID, conn, System.Data.SqlDbType.TinyInt));
            c.ExecuteNonQuery();
        }

        public static MySqlConnection GetMYSQLConnection()
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            return conn;
        }
        public void InsertnewOPCField(OPCField F, MySqlConnection conn)
        {
            MySqlCommand c = new MySqlCommand("INSERT into fields ( Name ) VALUES (  ) " , conn);

        }

        public static OPCField GetFieldInfo(string Name, MySqlConnection conn)
        {
            MySqlCommand c = new MySqlCommand(string.Format("SELECT * FROM fields WHERE Name ='{0}'", Name), conn);
            OPCField Field = null;

            MySqlDataReader reader = c.ExecuteReader();

            try
            {
                if (reader.Read())
                {
                    Field = new OPCField();
                    Field.ID = (int)reader["ID"];
                    Field.Name = (string)reader["Name"];
                    Field.Scale = reader.GetFloat("Scale");
                }
            }
            finally
            {
                reader.Close();
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
}
