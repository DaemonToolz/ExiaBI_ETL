using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCF_BIProject.Data.Classes{
    public class ClassMapper {
        public Dictionary<Int32,Products.Composant>    Components{
            get; set;
        }
        public Dictionary<Int32, Products.Produit>      Products{
            get; set;
        }
        public Dictionary<Int32, Generics.Client>       Clients{
            get; set;
        }
        public Dictionary<Int32, Orders.Commande>       Orders{
            get; set;
        }

        public ClassMapper(){
            Components = new Dictionary<Int32, Products.Composant>();
            Products = new Dictionary<Int32, Products.Produit>();
            Clients = new Dictionary<Int32, Generics.Client>();
            Orders = new Dictionary<Int32, Orders.Commande>();
        }

    }
}