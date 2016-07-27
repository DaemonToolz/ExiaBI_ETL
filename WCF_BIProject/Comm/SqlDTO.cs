using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WCF_BIProject.Data.Classes.Generics;
using WCF_BIProject.Data.Classes.Orders;
using WCF_BIProject.Data.Classes.Products;

namespace WCF_BIProject.Comm{
    public class LinkTable{
        public Int32 Table0Id { get; set; }
        public Int32 Table1Id { get; set; }
        public Int32 Id { get; set; }
    }

    public class LinkTable_Commands:LinkTable{
        public Int32 Quantite { get; set; }
    }

    public class ComposantDto : Composant{
        public Int32 Id { get; set; }
        public ComposantDto(string @ref, string design, double prix, long stamp) : base(@ref, design, prix){}
    }

    public class ProduitDto : Produit{
        public Int32 Id { get; set; }
        public ProduitDto(string @ref, string design, double prix, long stamp, List<Composant> comp) : base(@ref, design, prix, comp){}
    }

    public class ClientDto : Client{
        public ClientDto(string reference, long timestamp, string nom, string adresse, String telephone) : base(reference, nom, adresse, telephone){}

        public Int32 Id { get; set; }
    }

    public class CommandeDto : DataStructure{
        public Int32 Id { get; set; }
        public ClientDto Client { get; set; }
        public Dictionary<ProduitDto,int> Produits { get; set; }
        public DateTime DateCommande { get; set; }
        public DateTime DateProduction { get; set; }
        public DateTime DateLivraison { get; set; }

        public CommandeDto(string @ref, string design, double prix, long stamp) : base(@ref, design, prix){
        }
    }
}