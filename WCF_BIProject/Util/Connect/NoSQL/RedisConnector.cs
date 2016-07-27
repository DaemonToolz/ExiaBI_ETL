using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCF_BIProject.Util.Connect.NoSQL{
    public class RedisConnector : IDisposable{
        //This is the URL of your redis, if it is running your local machine it must me like this
        private const string RedisUrl = "localhost:6379";

        //Define the lazy connector configurations
        private static readonly Lazy<ConfigurationOptions> ConfigOptions = new Lazy<ConfigurationOptions>(() => {
            var configOptions = new ConfigurationOptions()
            {
                EndPoints = { RedisUrl },
                ClientName = "BI_RedisDatabase",
                ConnectTimeout = 100000,
                SyncTimeout = 100000,
                AbortOnConnectFail = false
            };

            return configOptions;
        });

        //Create the connection
        private static readonly Lazy<ConnectionMultiplexer> Conn = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(ConfigOptions.Value));

        //Connect to Redis cache
        public static ConnectionMultiplexer Connect{
            get { return Conn.Value; }
        }

        public void Dispose(){
            Connect.Dispose();
        }
    }
}