using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WCF_BIProject.Data.Classes.Generics;
using WCF_BIProject.Data.Classes.Products;
using WCF_BIProject.Data.State;

namespace WCF_BIProject.Data.Classes.Orders{
    public class Commande : DataStructure{
        // [BsonElementAttribute("etat")]
        public String Etat{ get; set; }
        //[BsonElementAttribute("produits")]
        //[BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<Produit, Int32> Produits{ get; set; }
        //[BsonElementAttribute("client")]
        //[BsonElement]
        public Client Client { get; set; }

        //[BsonElementAttribute("dateCommande")]
        //[BsonElement]
        public DateTime DateCommande { get; set; }

        //[BsonElementAttribute("dateProduction")]
        //[BsonElement]
        public DateTime DateProduction { get; set; }

        //[BsonElementAttribute("dateLivraison")]
        //[BsonElement]
        public DateTime DateLivraison { get; set; }

        //[BsonConstructor]
        public Commande(string @ref, string design, double prix, Client c, Dictionary<Produit, Int32> p, DateTime cmd, DateTime prod, DateTime liv, String etat) : base(@ref, design, prix){
            Client = c;
            Produits = p;
            DateCommande = cmd;
            DateProduction = prod;
            DateLivraison = liv;
            Etat = etat;
        }
    }
}