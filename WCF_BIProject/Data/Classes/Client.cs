using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCF_BIProject.Data.Classes.Generics{
    public class Client{

        //[BsonElementAttribute("reference")]
        /*
        [BsonElement]
        public String Reference{
            get; set;
        }
        */
        //[BsonElementAttribute("nom")]
        //[BsonElement]
        public String Nom{
            get;set;
        }

        //[BsonElementAttribute("adresse")]
        //[BsonElement]
        public String Adresse {
            get;set;
        }

        //[BsonElementAttribute("telephone")]
        //[BsonElement]
        public String Telephone{
            get;set;
        } 

        public Client(String reference, String nom, String adresse, String telephone){
            //Reference = reference;
            Nom = nom;
            Adresse = adresse;
            Telephone = telephone;
        }
    }
}