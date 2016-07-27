using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace WCF_BIProject
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEtlService" in both code and config file together.
    [ServiceContract/*(CallbackContract = typeof(IEtlCallback))*/]

    public interface IEtlService
    {


        [OperationContract]
        int GetConnectors();

        [OperationContract]
        String GetEvents(int index);
        // TODO: Add your service operations here

        [OperationContract]
        int GetEventSize();

        [OperationContract]
        int GetWarnings();

        [OperationContract]
        int GetErrors();

        [OperationContract]
        int GetTransactions();

        //[OperationContract(IsOneWay = true)]
        //void Update();
    }

    /*
    [DataContract]
    public class ClientEventUpdater
    {
        [DataMember]
        public String Event { get; set; }

        [DataMember]
        public int Connections { get; set; }

        [DataMember]
        public int FailedTransactions { get; set; }

        [DataMember]
        public int SuccessfulTransactions { get; set; }

        [DataMember]
        public int TransactionsWarnings { get; set; }

        [DataMember]
        public List<String> Events { get; set; }
    }


    public interface IEtlCallback
    {

        [OperationContract(IsOneWay = true)]
        void NotifyServer(ClientEventUpdater eventData);
    }
    */
}
