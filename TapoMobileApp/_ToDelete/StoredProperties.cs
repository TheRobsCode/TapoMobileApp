using System.Text.Json;
using System.Text.Json.Serialization;

namespace TapoMobileApp
{
    public interface IStoredProperties
    {
        bool ContainsKey(string key);
        string Get(string key);
        T Get<T>(string key);
        T Get<T>(int port);
        void Set(string key, string obj);
        void Set<T>(string key, T obj);
        void Set<T>(int port, T obj);
        void Clear();
        void Clear(int port);
        void StoreLog(string error);
    }

    public class StoredProperties : IStoredProperties
    {
        public void Clear()
        {
            for(var port = 1; port< 255;port++)
            {
                var cacheProp = "CacheProp" + port;
                if (!ContainsKey(cacheProp))
                {
                    continue;
                }
                Preferences.Set(cacheProp,null);
            }
        }

        public void Clear(int port)
        {
            var key = "CacheProp" + port;
            Preferences.Remove(key);
        }

        public bool ContainsKey(string key)
        {
            return Preferences.ContainsKey(key);
        }

        public string Get(string key)
        {
            if (!Preferences.ContainsKey(key))
                return "";
            return Preferences.Get(key,"").ToString();
        }
        public T Get<T>(int port)
        {
            var cacheProp = "CacheProp" + port;
            return Get<T>(cacheProp);
        }
        public T Get<T>(string key)
        {
            if (!Preferences.ContainsKey(key))
                return default;
            if (string.IsNullOrEmpty(Preferences.Get(key,"")))
                return default;

            var json = Preferences.Get(key, "").ToString();
            if (string.IsNullOrEmpty(json))
                return default;
            return Json.Deserialize<T>(json);
        }

        public void Set(string key, string obj)
        {
            if (Preferences.ContainsKey(key))
                Preferences.Remove(key);

            Preferences.Set(key, obj);
        }

        public void Set<T>(string key, T obj)
        {
            var json = Json.Serialize(obj);
            Set(key, json);
        }
        public void Set<T>(int port, T obj)
        {
            var cacheProp = "CacheProp" + port;
            Set(cacheProp, obj);
        }

        public void StoreLog(string error)
        {
            Set("log", error);
        }
    }
}