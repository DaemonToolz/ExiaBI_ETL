using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using WCF_BIProject.Util.Connect;

namespace WCF_BIProject.Util.Events{
    public class DatabaseEvent : INotifyPropertyChanged{
     
        private GenericConnector connector;

        public DatabaseEvent(ref GenericConnector connector){
            this.connector = connector;
        }

            // This is a straightforward implementation for 
            // declaring a public field
        public GenericConnector Connector{
            get{
                return connector;
            }
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs e){
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        protected void OnPropertyChanged(string propertyName){
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}