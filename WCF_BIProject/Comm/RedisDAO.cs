using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WCF_BIProject.Data.Classes.Generics;

namespace WCF_BIProject.Comm{
    public static class RedisDAO<T> where T : DataStructure{
        // Add a new object to the database
        // Whatever the type
        public static void Add(T obj){
            var redis = Util.Connect.NoSQL.RedisConnector.Connect.GetDatabase();
            //Uses Json.NET to serialize the object before sending it to Redis
            //Uses the Redis class to communicate with Redis
            redis.StringSet(obj.Reference, JsonConvert.SerializeObject(obj));
        }

        // Add a list of objects
        public static void Add(List<T> listObj){
            foreach (var obj in listObj){
                Add(obj);
            }
        }

        // Get one object through its reference
        public static T Get(String objRef){
            var redis = Util.Connect.NoSQL.RedisConnector.Connect.GetDatabase();
 
            //Uses Json.NET to serialize the object before sending it to Redis
            //Uses the Redis class to communicate with Redis
            return JsonConvert.DeserializeObject<T>(redis.StringGet(objRef));
        }

        // Get a list of objects
        public static List<T> Get(List<String> listOfObjIds){
            List<T> objList = new List<T>();

            foreach (var objId in listOfObjIds){
                var obj = Get(objId);
                objList.Add(obj);
            }
 
            return objList;
        } 
 
 
    }
}