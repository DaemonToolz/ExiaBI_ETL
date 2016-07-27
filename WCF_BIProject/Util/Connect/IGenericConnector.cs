using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCF_BIProject.Util.Connect{
    interface IGenericConnector{
        void Write(String table);
        void Read(String table);

        void checkAvailability();
    }
}
