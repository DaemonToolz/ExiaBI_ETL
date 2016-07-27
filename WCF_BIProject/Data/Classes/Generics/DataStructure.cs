using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WCF_BIProject.Data.Classes.Orders;
using WCF_BIProject.Data.Classes.Products;

namespace WCF_BIProject.Data.Classes.Generics{
    //[BsonDiscriminator(RootClass = true)]
    //[BsonKnownTypes(typeof(Commande), typeof(Produit), typeof(Composant))]
    public abstract class DataStructure{

        //[BsonElementAttribute("reference")]
        //[BsonElement]
        public String Reference {
            get; set;
        }

        //[BsonElementAttribute("designation")]
        //[BsonElement]
        public String Designation {
            get; set;
        }
        //[BsonElementAttribute("prix")]
        //[BsonElement]
        public double Prix{
            get; set;
        }

        public DataStructure(String @ref, String design, double prix){
            Reference = @ref;
            Designation = design;
            Prix = prix;
        }
        
    }
}