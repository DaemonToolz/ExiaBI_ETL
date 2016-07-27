
using BIP_ASPGUI.Service;
using System;
using System.ServiceModel;

namespace BIP_ASPGUI
{
    public class Global : System.Web.HttpApplication{
        private static readonly EtlServiceClient service = new EtlServiceClient();
        //private static readonly WcfCallback callback = new WcfCallback();
        protected void Application_Start(object sender, EventArgs e){
            //service = new EtlServiceClient(new InstanceContext(callback));
            service.Open();
        }

        protected void Application_Stop(object sender, EventArgs e){
            service.Close();
        }

        public static EtlServiceClient Service{
            get { return service; }
        }

        //public static WcfCallback Callback{
        //    get { return callback; }
        //}

        //public static void update(){
        //    service.Update();
        //}
    }
}