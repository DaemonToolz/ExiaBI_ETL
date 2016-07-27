using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WCF_BIProject.Data.Classes.Generics;

namespace WCF_BIProject.Data.Classes.Products{

    public class Composant : DataStructure{
        //[BsonConstructor]
        public Composant(string @ref, string design, double prix) : base(@ref, design, prix){}
    }
}