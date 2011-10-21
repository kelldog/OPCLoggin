using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DatabaseBuilder
{
    public class OPCField_ex
    {
        public string OPCItemID;
        public short DeviceType;
        public string ClientItemID;
        public string ClientTopicID;
        public float ScaleFactor;
        public OPCStationInfo StationInfo;
        public bool RequestFromOPC;
        public string Type;
        public string OPCprefix;
        public string OPCTopicID;
        public string units;
        public bool CurrentlyGraphed;
        public List<string> Profiles;
        public List<byte> WindowNums;
        public float Preferred_Window_Width;

  //      public Type DataType;


        public string OPC_Lookup
        {
            get
            {
                string ret = string.Format("{0}.{1}{2}", OPCTopicID, OPCprefix, OPCItemID);
                ret = ret.TrimEnd('\r', ' ', '\n');
                return ret;
            }
        }



        public UInt32 Mask
        {

            get
            {
                UInt32 mask = 0;
                UInt32 i = 1;
                foreach (char c in OPCItemID)
                {
                    mask += i * (byte)c;
                    i++;
                }
                mask &= 0x00FFFFFF;
                mask |= (((UInt32)StationInfo.StationID) << 24);
                return mask;
            }
        }

        public string FullName
        {
            get
            {
                return string.Format("{0}{1}", StationInfo.StationID, OPCItemID);
            }
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
