
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MySql.Data.MySqlClient.Memcached;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WCF_BIProject.Data.Classes.Generics;
using WCF_BIProject.Data.Classes.Orders;
using WCF_BIProject.Data.Classes.Products;

namespace WCF_BIProject.Util.Connect.NoSQL{
    public class MongoDBConnector : IDisposable{
        private static MongoClient                                          Client;
        private static IMongoDatabase                                       Database;

        private static IMongoCollection<Data.Classes.Generics.Client>       Clients;
        private static IMongoCollection<Commande>                           Commandes;
        private static IMongoCollection<Produit>                            Produits;
        private static IMongoCollection<Composant>                          Composants;
        private Object locker;


        private static bool exists = false;

        public void askUpdate(params GenericConnector[] databases){
            if (/*!exists && */databases != null){
                databases.ToList().ForEach(db => db._init());
                //exists = true;
            }
        }


        public MongoDBConnector(){
            locker = new Object();
            Client = new MongoClient(ConnectionStrings.OSqlData_1);

            Database = Client.GetDatabase("BIP_MDB");
            
            exists = Database.ListCollections().ToList().Count != 0;

            Clients = Database.GetCollection<Data.Classes.Generics.Client>(Tables.SqlTableClient);
            Composants = Database.GetCollection<Composant>(Tables.SqlTableComp);
            Produits = Database.GetCollection<Produit>(Tables.SqlTableProduct);
            Commandes = Database.GetCollection<Commande>(Tables.SqlTableCommand);  
        }

        public void Dispose(){
  
        }

        public void AddProduct(DataStructure data, String type) {
            lock (locker)
            {
                if (type == null)
                    type = data.GetType().Name;

                switch (type)
                {
                    case "Produit":
                        if (Produits.Find(Builders<Produit>.Filter.Eq("Reference", ((Produit)data).Reference)).Count() != 0)
                            UpdateProduct(data);
                        else 
                            Produits.InsertOne((Produit)data);
                        break;

                    case "Composant":
                        if (Composants.Find(Builders<Composant>.Filter.Eq("Reference", ((Composant)data).Reference)).Count() != 0)
                            UpdateProduct(data);
                       else
                            Composants.InsertOne((Composant)data);
                        break;

                    case "Commande":
                        if (Commandes.Find(Builders<Commande>.Filter.Eq("Reference", ((Commande)data).Reference)).Count() != 0)
                            UpdateProduct(data);
                        else
                            Commandes.InsertOne((Commande)data);
                        break;

                }
            }
        }

        public void AddClient(Data.Classes.Generics.Client client){
            lock (locker){
                
                if (Clients.Find(Builders<Data.Classes.Generics.Client>.Filter.And(new FilterDefinition<Data.Classes.Generics.Client>[]{
                            Builders<Data.Classes.Generics.Client>.Filter.Eq("Nom", (client).Nom),
                            Builders<Data.Classes.Generics.Client>.Filter.Eq("Adresse", (client).Adresse)
                        })).Count() != 0)
                    UpdateClient(client);
                else
                    Clients.InsertOne(client);
            }
        }

        public void UpdateClient(Data.Classes.Generics.Client client){
            lock (locker)
            {
                Clients.UpdateOne(Builders<Data.Classes.Generics.Client>.Filter.Eq("Nom", client.Nom),
                                    Builders<Data.Classes.Generics.Client>.Update.Set("Adresse", client.Adresse).Set("Telephone", client.Telephone));
            }
        }

        public void UpdateProduct(DataStructure data){
            lock (locker)
            {
                String type = data.GetType().Name;

                switch (type)
                {
                    case "Produit":
                        {
                            Produits.UpdateOne(Builders<Produit>.Filter.Eq("Reference", data.Reference),
                                                    Builders<Produit>.Update.Set("Prix", data.Prix)
                                                    .Set("Designation", data.Designation)
                                                    .Set("TotalPresence", ((Produit)data).TotalPresence)
                                                    .Set("Composants", ((Produit)data).Composants));
                        }
                        break;

                    case "Composant":
                        {
                            Composants.UpdateOne(Builders<Composant>.Filter.Eq("Reference", data.Reference),
                                           Builders<Composant>.Update.Set("Prix", data.Prix).Set("Designation", data.Designation));
                        }
                        break;

                    case "Commande":
                        {

                            Commandes.UpdateOne(
                                Builders<Commande>.Filter.And(
                                        Builders<Commande>.Filter.Eq("Designation", data.Designation),
                                        Builders<Commande>.Filter.Eq("Reference", data.Reference),
                                        Builders<Commande>.Filter.Eq("Prix", data.Prix)),
                                           Builders<Commande>.Update.Set("Prix", data.Prix).Set("Designation", data.Designation)
                                                                    .Set("Client", ((Commande)data).Client)
                                                                    .Set("DateCommande", ((Commande)data).DateCommande)
                                                                    .Set("DateLivraison", ((Commande)data).DateLivraison)
                                                                    .Set("DateProduction", ((Commande)data).DateProduction)
                                                                    .Set("Produits", ((Commande)data).Produits)
                                                                    .Set("Etat", ((Commande)data).Etat));
                        }
                        break;

                }
            }
        }

        private void Register(){
            BsonClassMap.RegisterClassMap<Data.Classes.Generics.Client>(cm => {
                cm.MapMember(c => c.Nom);
                cm.MapMember(c => c.Adresse);
                cm.MapMember(c => c.Telephone);
            });

            BsonClassMap.RegisterClassMap<Commande>(cm => {
                cm.MapMember(c => c.Etat);
                cm.MapMember(c => c.Produits);
                cm.MapMember(c => c.Client);
                cm.MapMember(c => c.DateCommande);
                cm.MapMember(c => c.DateLivraison);
                cm.MapMember(c => c.DateProduction);
            });

            BsonClassMap.RegisterClassMap<Composant>(cm => {
                cm.MapMember(c => c.Reference);
                cm.MapMember(c => c.Prix);
                cm.MapMember(c => c.Designation);
            });

            BsonClassMap.RegisterClassMap<Produit>(cm => {
                cm.MapMember(c => c.Reference);
                cm.MapMember(c => c.Designation);
                cm.MapMember(c => c.Composants);
                cm.MapMember(c => c.Prix);
            });
        }

        private void MapReduce(){
  
        }
        
#if DEBUG_1
        private void initialization(){
            Data.Classes.Generics.Client clt;
            AddClient(clt = new Data.Classes.Generics.Client("_VOID", "_VOID", "_VOID", "_VOID"));
            Composant c = new Composant("_VOID", "_VOID", 0.0);
            AddProduct(c, null);
            List<Composant> lc = new List<Composant>();
            lc.Add(c);
            Produit p = new Produit("_VOID", "_VOID", 0.0, lc);

            AddProduct(p, null);
            Dictionary<Produit, Int32> dc = new Dictionary<Produit, int>();
            dc.Add(p, 1);
            AddProduct(new Commande("_VOID", "_VOID", 0.0, clt, dc, DateTime.Now, DateTime.Now, DateTime.Now), null);
        }
#endif
    }
}