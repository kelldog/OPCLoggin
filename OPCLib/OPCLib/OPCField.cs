using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPCLib
{
    public class OPCField
    {
        public OPCField()
        {
            LastUpdate = int.MaxValue;
        }
        public string Name;
        public short ID;
        public float Scale;
        public string Type;
        public int LastUpdate;
        public float LastValue;
        public DateTime LastUpdateTime;
    }
}
