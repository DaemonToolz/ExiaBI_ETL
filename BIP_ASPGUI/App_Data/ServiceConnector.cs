using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BIP_ASPGUI{
    static public class ServiceConnector{
        public static int getData(){
            return Global.Service.GetConnectors();
        }

        public static int getFailures(){
            return Global.Service.GetErrors();
        }
        public static int getWarnings(){
            return Global.Service.GetWarnings();
        }
        public static int getTransactions(){
            return Global.Service.GetTransactions();
        }


        public static List<String> getEvents(){
            try {
                List<String> tempo = new List<String>();
                for (int i = 0; i < Global.Service.GetEventSize(); ++i)
                    tempo.Add(Global.Service.GetEvents(i));
                return tempo;
            }
            catch{
                return null;
            }
        }
    }
}