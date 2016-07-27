using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Data.SqlClient;
using WCF_BIProject.Util.Connect;
using System.ServiceModel;
using WCF_BIProject.Util.Connect.NoSQL;

namespace WCF_BIProject.Util.Local{
    public class Local : IDisposable{
        // Multithreading for RT treatment
        private List<GenericConnector> mtListeners;

        //private readonly Random idGenerator = new Random();
        // Maw connections:
        // 3 SQL + 1 NoSQL

        public readonly int _MAX_CONNECTION = 4;

        //public static event Devart.Data.MySql.OnChangeEventHandler OnChange;

        public readonly List<String> events = new List<string>();
        private static MongoDBConnector mongoDB = null;

        public Local(){
            String key;
            mtListeners = new List<GenericConnector>();

            try {
                mtListeners.Add(new GenericConnector("preparation", key = ConnectionStrings.ISqlData_0CString, null, this, ref mongoDB));
            }
            catch(Exception e){
                events.Add(DateTime.Now+":Database number 1 not detected");
                events.Add("--- Cause:" + e);
                EtlService.Failures++;
            }
            
            try{
                mtListeners.Add(new GenericConnector("fabrication", (key = ConnectionStrings.ISqlData_1CString), null, this, ref mongoDB));
            }
            catch(Exception e){
                events.Add(DateTime.Now+":Database number 2 not detected");
                events.Add("--- Cause:" + e);
                EtlService.Failures++;
               
            }

            try {
                mtListeners.Add(new GenericConnector("expedition", key = ConnectionStrings.ISqlData_2CString, null, this, ref mongoDB));
            }
            catch(Exception e){
                events.Add(DateTime.Now+":Database number 3 not detected");
                events.Add("--- Cause:" + e);
                EtlService.Failures++;
                
            }
            
            try{
                mongoDB = new MongoDBConnector();
                foreach(var obj in mtListeners)
                    obj.redirectMongo(ref mongoDB);
                mongoDB.askUpdate(mtListeners.ToArray());
            }
            catch (Exception e)
            {
                events.Add(DateTime.Now + ":Cannot connect to Remote MongoDB Database : " + e);
                events.Add("--- Cause:" + e);
                EtlService.Failures++;
            }
        }


        internal static void NotificationCallback(object o, Devart.Data.MySql.MySqlTableChangeEventArgs args, ref GenericConnector connector){
            try {
                //if (args.Command != MySql && args.Command != SqlNotificationInfo.Delete) {
                //OnChange.Invoke(o, args);
                //connector.Update();
                //}
            }
            catch(Exception e){
                connector.signalEvent(DateTime.Now+":Impossible to extract data from the targeted connector #"+connector.LocalId);
                connector.signalEvent("---- Cause:" + e);
            }

        }

        public void updateEvents(String @event){
            if (events.Count > 128)
                events.Clear();
            events.Add(@event);
        }

        public void Dispose(){
            foreach(var obj in mtListeners)  obj.Dispose();
            mongoDB.Dispose();
        }

    }
}