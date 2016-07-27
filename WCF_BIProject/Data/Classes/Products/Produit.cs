using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WCF_BIProject.Data.Classes.Generics;

namespace WCF_BIProject.Data.Classes.Products{
    public class Produit : DataStructure{


        public Int32 TotalPresence{ get; set; }
        //[BsonElementAttribute("composants")]
        //[BsonElement]
        public List<Composant> Composants { get; set; }
        //[BsonConstructor]
        public Produit(string @ref, string design, double prix, List<Composant> comp) : base(@ref, design, prix){
            Composants = comp;
            TotalPresence = 0;
        }
    }
}