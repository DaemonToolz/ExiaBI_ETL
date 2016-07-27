using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using StackExchange.Redis;
using System.Web;
using System.Web.Services;
using System.Web.UI.WebControls;
using ServiceStack.Redis;
using System.Data;
using System.Runtime.Remoting;
using Newtonsoft.Json;
using WCF_BIProject.Data.Classes.Generics;
using WCF_BIProject.Util.Connect.NoSQL;
using System.Data.OleDb;
using WCF_BIProject.Data.Classes;
using WCF_BIProject.Data.Classes.Products;
using WCF_BIProject.Data.Classes.Orders;
using WCF_BIProject.Comm;
using MySql.Data.MySqlClient;

namespace WCF_BIProject.Util.Connect{
    public class GenericConnector : IGenericConnector, IDisposable{
        // Temporary map
        // Following format:
        //  - Table -> Row -> Column -> value:type
        private int localId; 
        private MySqlConnection connection;
        private MySqlCommand lastCommand_g;
        //private OleDbConnection connection;
        //private OleDbCommand lastCommand;

        private List<String>    tables;
        private List<DataStructure> cache;
        private static MongoDBConnector mongoDB = null;
        private ClassMapper mapper; 

        private static Local.Local etl = null;
        private string database;
        private static int connectors = 0;  // Counting connections
        private System.Timers.Timer timer = new System.Timers.Timer();
        private String address;
        // Produit, Composant, ComProd, ProdComp, Client, Commande
        private Dictionary<String,String> timestamps;

        public Int32 LocalId{
            get { return localId; }
        }
        public MySqlConnection CreateNewConnection(string cString){
            return new MySqlConnection(cString);
        }

        public OleDbConnection  CreateNewOleDbConnection(string cString){
            return new OleDbConnection(cString);
        }

        public GenericConnector(String database, String cstr, GenericConnector comparator, Local.Local etllocal, ref MongoDBConnector rmongodb){
            this.database = database;
            mapper = new ClassMapper();
            timestamps = new Dictionary<string, string>();
            tables = new List<String>();

            if (comparator != null){
                comparator.Dispose();
                connectors--;
            }

            connection = CreateNewConnection(address = cstr);
            connection.Open();
            
            localId = connectors++;

            if(etllocal != null && etl == null)
                etl = etllocal;

            if(etl != null)
                etl.events.Add("Creating Connector #" + (connectors));

            if (mongoDB == null && rmongodb != null){
                mongoDB = rmongodb;
            }

            if (cache == null)
                cache = new List<DataStructure>();

            /*
            using (MySqlCommand command = new MySqlCommand("SELECT table_name FROM information_schema.tables", connection))
            {
                MySqlDataReader temp = command.ExecuteReader();
                using (var treader = command.ExecuteReader())
                {
                    temp.Read();
                    var tableSchema = temp.GetSchemaTable();
                    foreach (DataRow row in tableSchema.Rows)
                    {
                        headers.Add(row["ColumnName"].ToString());
                    }
                }
            }
            */
            // Produit, Composant, ComProd, ProdComp, Client, Commande

            tables.Add(Tables.SqlTableComp);
            tables.Add(Tables.SqlTableClient);
            tables.Add(Tables.SqlTableProduct);
            tables.Add(Tables.SqlTableLProdComp);
            tables.Add(Tables.SqlTableCommand);
            tables.Add(Tables.SqlTableLComProd);
        }

        public void redirectMongo(ref MongoDBConnector rmongodb){
            if(rmongodb != null)
                mongoDB = rmongodb;
        }

        private void OnTimer(object sender, System.Timers.ElapsedEventArgs args){
            try{
                foreach (var tablename in tables)
                    Read(tablename);
            }
            catch (Exception e){
                etl.events.Add("An error occured, database #" + localId + " is unreachable:" +e);
                EtlService.Failures++;
            }
        }


        public void Read(String table){
            
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            if (!timestamps.ContainsKey(table))
                timestamps.Add(table, "");

            // Produit, Composant, ComProd, ProdComp, Client, Commande
            lastCommand_g = new MySqlCommand();
            lastCommand_g.Connection = connection;
            lastCommand_g.CommandText = ("SELECT * FROM " + table);
            if (timestamps.Count != 0) {
                lastCommand_g.CommandText += " WHERE lastUpdate > @lastUpdate";
                lastCommand_g.Parameters.AddWithValue("@lastUpdate", DateTime.Parse(timestamps[table]));
            }
            {
                MySqlDataReader reader = lastCommand_g.ExecuteReader();

                while (reader.Read())
                {
                    switch (table)
                    {
                        case "Composant":
                            //cache.Add(createComposant(ref reader));
                            if (timestamps[table].Equals("") || DateTime.Parse(timestamps[table]) < DateTime.Parse((Convert.ToString(reader[4]))))
                                timestamps[table] = (Convert.ToString(reader[4]));

                            mongoDB.AddProduct(createComposant(ref reader), table);

                            break;

                        case "Produit":
                            //cache.Add(createProduct(ref reader));
                            if (timestamps[table].Equals("") || DateTime.Parse(timestamps[table]) < DateTime.Parse((Convert.ToString(reader[4]))))
                                timestamps[table] = (Convert.ToString(reader[4]));

                            mongoDB.AddProduct(createProduct(ref reader), table);
                            break;

                        case "ComProd":
                            {
                                Int32 order_id = Convert.ToInt32(reader[0]), product_id = Convert.ToInt32(reader[1]), totalqt = Convert.ToInt32(reader[3]);
                                if (!mapper.Orders[order_id].Produits.ContainsKey(mapper.Products[product_id]))
                                    mapper.Orders[order_id].Produits.Add(mapper.Products[product_id], totalqt);
                                else
                                    mapper.Orders[order_id].Produits[mapper.Products[product_id]] = totalqt;


                                mapper.Products[Convert.ToInt32(reader[1])].TotalPresence++;
                                mongoDB.UpdateProduct(mapper.Orders[order_id]);
                                mongoDB.UpdateProduct(mapper.Products[product_id]);

                                if (timestamps[table].Equals("") || DateTime.Parse(timestamps[table]) < DateTime.Parse((Convert.ToString(reader[5]))))
                                    timestamps[table] = (Convert.ToString(reader[5]));
                            }
                            break;

                        case "ProdComp":
                            {
                                Int32 product_id = Convert.ToInt32(reader[1]), comp_id = Convert.ToInt32(reader[2]);
                                if (!mapper.Products[product_id].Composants.Contains(mapper.Components[comp_id]))
                                {
                                    mapper.Products[product_id].Composants.Add(mapper.Components[comp_id]);
                                    mongoDB.UpdateProduct(mapper.Products[product_id]);
                                }
                                if (timestamps[table].Equals("") || DateTime.Parse(timestamps[table]) < DateTime.Parse((Convert.ToString(reader[3]))))
                                    timestamps[table] = (Convert.ToString(reader[3]));
                            }
                            break;

                        case "Client":
                            mongoDB.AddClient(createClient(ref reader));

                            if (timestamps[table].Equals("") || DateTime.Parse(timestamps[table]) < DateTime.Parse((Convert.ToString(reader[4]))))
                                timestamps[table] = (Convert.ToString(reader[4]));
                            break;

                        case "Commande":
                            //cache.Add(createCommande(ref reader, mapper.Clients.First(clt => clt.Key == Convert.ToInt32(reader[0])).Value));
                            mongoDB.AddProduct(createCommande(ref reader, mapper.Clients.First(clt => clt.Key == Convert.ToInt32(reader[0])).Value), table);

                            if (timestamps[table].Equals("") || DateTime.Parse(timestamps[table]) < DateTime.Parse((Convert.ToString(reader[6]))))
                                timestamps[table] = (Convert.ToString(reader[6]));
                            //timestamps[table] = (Convert.ToString(reader[6]));
                            break;
                    }

                }

                reader.Close();
                connection.Close();
                lastCommand_g.Dispose();

            }
            //if(cache.Count != 0)
            //    Write(table);
        }

        private static dynamic ConvertObject(dynamic source, Type dest){
            return Convert.ChangeType(source, dest);
        }

        public void Write(String table){
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            if (cache == null) cache = new List<DataStructure>();
            //ObjectHandle o;
            
            foreach (var listPerTable in cache){
                //o = Activator.CreateInstance(Type.GetType(table,true,false)) as ObjectHandle;
                //var temp = ConvertObject(o.Unwrap(), Type.GetType(table,true,false));
                mongoDB.AddProduct(listPerTable,table);
                EtlService.Transactions++;
            }
            
            cache.Clear();
            //var redis = WCF_BIProject.Util.Connect.NoSQL.RedisConnector.Connect.GetDatabase();
            //Uses Json.NET to serialize the object before sending it to Redis
            //Uses the Redis class to communicate with Redis
            //redis.StringSet("null", JsonConvert.SerializeObject(null));
        }

        public void Dispose(){
            if(connection.State != System.Data.ConnectionState.Closed)
                connection.Close();
            connection.Dispose();
            lastCommand_g.Dispose();
            connectors--;
        }

        public void checkAvailability(){
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Interval = 360000;
            timer.Enabled = true;
            timer.Start();
        }

        public void signalEvent(String str){
            etl.events.Add(str);
        }

        public static int Connectors{
            get { return connectors; }
        }

        private Produit createProduct(ref MySqlDataReader reader){
            return new Produit(
                        Convert.ToString(reader[1]),
                        Convert.ToString(reader[2]),
                        Convert.ToDouble(reader[3]),
                        new List<Composant>()); 
        }

        private Client createClient(ref MySqlDataReader reader){
            return new Client(
                        Convert.ToString(reader[0]),
                        Convert.ToString(reader[1]),
                        Convert.ToString(reader[2]),
                        Convert.ToString(reader[3]));
        }

        private Composant createComposant(ref MySqlDataReader reader){
            return new Composant(
                        Convert.ToString(reader[1]),
                        Convert.ToString(reader[2]),
                        Convert.ToDouble(reader[3]));
        }

        private Commande createCommande(ref MySqlDataReader reader, Client c){
            return new Commande(
                        Convert.ToString(reader[0]) + Convert.ToString(reader[5]),
                        Convert.ToString(reader[0]),
                        0.0,
                        c,
                        new Dictionary<Produit, Int32>(),
                        Convert.ToDateTime(reader[1]),
                        Convert.ToDateTime(reader[2]),
                        Convert.ToDateTime(reader[3]),
                        Convert.ToString(reader[4]));
        }

        public void _init() {
            if (connection == null) return;

            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            String compProdKey = Tables.SqlTableLProdComp, prodCommKey = Tables.SqlTableLComProd;
            MySqlDataReader reader;

            Composant cp;
            timestamps.Add(Tables.SqlTableComp, "");
            Dictionary<Int32, Composant> mapComposants = new Dictionary<Int32, Composant>();
            using (MySqlCommand lastCommand = new MySqlCommand("SELECT * FROM " + Tables.SqlTableComp, connection)) {
                reader = lastCommand.ExecuteReader();
                while (reader.Read()){
                    if (timestamps[Tables.SqlTableComp].Equals("") || DateTime.Parse(timestamps[Tables.SqlTableComp]) < DateTime.Parse((Convert.ToString(reader[4]))))
                        timestamps[Tables.SqlTableComp] = (Convert.ToString(reader[4]));

                    mapComposants.Add(Convert.ToInt32(reader[0]), cp = createComposant(ref reader));
                }
                EtlService.Transactions++;

            }
            reader.Close();
            Produit pt;
            Dictionary<Int32, Produit> mapProduits = new Dictionary<int, Produit>();

            timestamps.Add(Tables.SqlTableProduct, "");
            using (MySqlCommand lastCommand = new MySqlCommand("SELECT * FROM " + Tables.SqlTableProduct, connection)){
                reader = lastCommand.ExecuteReader();

                while (reader.Read()){
                    if (timestamps[Tables.SqlTableProduct].Equals("") || DateTime.Parse(timestamps[Tables.SqlTableProduct]) < DateTime.Parse((Convert.ToString(reader[4]))))
                        timestamps[Tables.SqlTableProduct] = (Convert.ToString(reader[4]));

                    mapProduits.Add(Convert.ToInt32(reader[0]), pt = createProduct(ref reader));
                }
                EtlService.Transactions++;

            }

            reader.Close();
            Client ct;
            Dictionary<Int32, Client> mapClients = new Dictionary<int, Client>();

            timestamps.Add(Tables.SqlTableClient, "");

            using (MySqlCommand lastCommand = new MySqlCommand("SELECT * FROM " + Tables.SqlTableClient, connection)){
                reader = lastCommand.ExecuteReader();
                while (reader.Read()){

                    if (timestamps[Tables.SqlTableClient].Equals("") || DateTime.Parse(timestamps[Tables.SqlTableClient]) < DateTime.Parse((Convert.ToString(reader[4]))))
                        timestamps[Tables.SqlTableClient] = (Convert.ToString(reader[4]));

                    mapClients.Add(Convert.ToInt32(reader[0]), ct = createClient(ref reader));
                }
                EtlService.Transactions++;

            }
            reader.Close();
            Commande cm;
            Dictionary<Int32, Commande> mapCommandes = new Dictionary<int, Commande>();

            timestamps.Add(Tables.SqlTableCommand, "");
            using (MySqlCommand lastCommand = new MySqlCommand("SELECT * FROM " + Tables.SqlTableCommand, connection)){
                reader = lastCommand.ExecuteReader();
                while (reader.Read()){
                    mapCommandes.Add(Convert.ToInt32(reader[0]), cm = createCommande(ref reader, mapClients[Convert.ToInt32(reader[5])]));
                    if (timestamps[Tables.SqlTableCommand].Equals("") || DateTime.Parse(timestamps[Tables.SqlTableCommand]) < DateTime.Parse((Convert.ToString(reader[6]))))
                        timestamps[Tables.SqlTableCommand] = (Convert.ToString(reader[6]));

                }
                EtlService.Transactions++;

            }
            reader.Close();
            // id_prod_comp / id_produit / id_composant
            // id_com_prod / id_produit / id_commande / quantiteTotal_com_prod / quantiteGerer_com_prod
            timestamps.Add(compProdKey, "");
            using (MySqlCommand lastCommand = new MySqlCommand("SELECT * FROM " + compProdKey, connection)){
                reader = lastCommand.ExecuteReader();
                while (reader.Read()){
                    mapProduits[Convert.ToInt32(reader[1])].Composants.Add(mapComposants[Convert.ToInt32(reader[2])]);
                    if (timestamps[compProdKey].Equals("") || DateTime.Parse(timestamps[compProdKey]) < DateTime.Parse((Convert.ToString(reader[3]))))
                        timestamps[compProdKey] = (Convert.ToString(reader[3]));
                }
                EtlService.Transactions++;

            }
            reader.Close();
            try {
                timestamps.Add(prodCommKey, "");
                using (MySqlCommand lastCommand = new MySqlCommand("SELECT * FROM " + prodCommKey, connection)) {
                    reader = lastCommand.ExecuteReader();
                    while (reader.Read()) {

                        if (timestamps[prodCommKey].Equals("") || DateTime.Parse(timestamps[prodCommKey]) < DateTime.Parse((Convert.ToString(reader[5]))))
                            timestamps[prodCommKey] = (Convert.ToString(reader[5]));
                        
                        mapCommandes[Convert.ToInt32(reader[0])].Produits.Add(mapProduits[Convert.ToInt32(reader[1])], Convert.ToInt32(reader[3]));
                        mapProduits[Convert.ToInt32(reader[1])].TotalPresence++;
                    }
                    EtlService.Transactions++;

                }
                reader.Close();
            }
            catch{

            }

            foreach(var temporary in mapComposants){
                mongoDB.AddProduct(temporary.Value, "Composant");
                mapper.Components.Add(temporary.Key, temporary.Value);
                EtlService.Transactions++;

            }

            foreach (var temporary in mapProduits){
                mongoDB.AddProduct(temporary.Value, "Produit");
                mapper.Products.Add(temporary.Key, temporary.Value);
                EtlService.Transactions++;

            }

            foreach (var temporary in mapClients){
                mongoDB.AddClient(temporary.Value);
                mapper.Clients.Add(temporary.Key, temporary.Value);
                EtlService.Transactions++;

            }

            foreach (var temporary in mapCommandes){
                foreach (var pdt in temporary.Value.Produits)
                    temporary.Value.Prix += pdt.Key.Prix * pdt.Value;
                mongoDB.AddProduct(temporary.Value, "Commande");
                mapper.Orders.Add(temporary.Key, temporary.Value);
                EtlService.Transactions++;

            }
            connection.Close();
            if(!reader.IsClosed)
                reader.Close();
            checkAvailability();
            
        }

        public MySqlConnection Connection{
            get{ return connection; }
        }
    }
}