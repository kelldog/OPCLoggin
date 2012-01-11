using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FieldsToStopMonitoring
{
    class Program
    {
        static void Main(string[] args)
        {

            string[] excluded_setpointValues = new string[]
            {
        "bias_current_stpt"
        ,"bias_voltage_stpt"
        ,"coil_current_side_a_stpt"
        ,"coil_current_side_b_stpt"
        ,"hvps_current_stpt"
        ,"hvps_a_voltage_stpt"
        ,"hvps_b_current_stpt"
        ,"hvps_side_b_stpt"
        ,"hvps_b_voltage_stpt"
        ,"phantom_disk_power_stpt"
        ,"mfc_5_stpt"
        ,"mfc_6_stpt"
        ,"mfc_7_stpt"
        ,"mfc_8_stpt"
            };

            StreamReader cfg_reader = new StreamReader(@"C:\OPCDataLogger\OPCDataLogger\aqt_main.cfg");
            StreamWriter cfg_writer = new StreamWriter(@"C:\OPCDataLogger\OPCDataLogger\aqt_main2.cfg",false);
            /*
             * 
Item Name=MODULEPFB1.SYMBOLS.W_BIAS_VOLTAGE_READBACK
Item Format Data As=0
Item Format Data As On Label=
Item Format Data As Off Label=
Item Format Data As Num Dec Places=2
Item=MODULEPFB1.SYMBOLS.W_BIAS_VOLTAGE_READBACK
Item Name=MODULEPFB1.SYMBOLS.X_BIAS_VOLTAGE_READBACK
             * 
             * */

            bool found = false;
            while (true)
            {
                if (cfg_reader.EndOfStream)
                {
                    break;
                }
                string line = cfg_reader.ReadLine();
                
                if (line.Contains("Item Name="))
                {
                    found = false;
                    foreach (string sss in excluded_setpointValues)
                    {
                        if (line.ToLower().Contains(sss))
                        {
                            //read forward 
                            cfg_reader.ReadLine();
                            cfg_reader.ReadLine();
                            cfg_reader.ReadLine();
                            cfg_reader.ReadLine();
                            cfg_reader.ReadLine();
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        cfg_writer.WriteLine(line);
                    }

                }
                else
                {
                    cfg_writer.WriteLine(line);
                }
            }

            cfg_writer.Close();
            cfg_reader.Close();

        }

    }
}
