using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Timers;
using WCF_BIProject.Util.Connect;
using WCF_BIProject.Util.Local;

namespace WCF_BIProject{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    //[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class EtlService : IEtlService{
        protected static Local      EtlTreatment;
        public static int           Transactions = 0;
        public static int           Failures = 0;
        public static int           Warnings = 0;

#if DEBUG_1
        protected readonly static Random temporaryRandom = new Random();
        protected readonly static Timer timerDispatcher = new Timer();
#endif
        static EtlService(){
            if (EtlTreatment == null)
                EtlTreatment = new Local();

#if DEBUG_1
            timerDispatcher.Interval = 20000;
            timerDispatcher.Elapsed += new System.Timers.ElapsedEventHandler(OnTimer);
            timerDispatcher.Start();
#endif

        }

        public int GetConnectors(){
            return GenericConnector.Connectors;
        }

#if DEBUG_1
        protected static void OnTimer(object sender, System.Timers.ElapsedEventArgs args){
            for(int i = 0; i < temporaryRandom.Next(0,500); ++i)
                EtlService.Failures++;
            for (int i = 0; i < temporaryRandom.Next(0, 500); ++i)
                EtlService.Warnings++;
            for (int i = 0; i < temporaryRandom.Next(0, 500); ++i)
                EtlService.Transactions++;

        }

#endif 
        public String GetEvents(int index){
            String @event = EtlTreatment.events.ElementAt(index);
            return @event;
        }

        public int GetEventSize(){
            int size = EtlTreatment.events.Count;
            return size;
        }

        public int GetWarnings(){
            return Warnings;
        }

        public int GetErrors(){
            return Failures;
        }

        public int GetTransactions(){
            return Transactions;
        }
        /*
        public IEtlCallback Callback
        {
            get
            {
                return OperationContext.Current.GetCallbackChannel<IEtlCallback>();
            }
        }
        public void Update(){
            ClientEventUpdater ceu = new ClientEventUpdater();
            ceu.Events = EtlTreatment.events;
            ceu.FailedTransactions = Failures;
            ceu.SuccessfulTransactions = Transactions;
            ceu.TransactionsWarnings = Warnings;
            ceu.Connections = GetConnectors();

            Callback.NotifyServer(ceu);
        }
       */
    }

  
}
