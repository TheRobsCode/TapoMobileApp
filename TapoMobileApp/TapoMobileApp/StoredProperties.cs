using Newtonsoft.Json;
using Xamarin.Forms;

namespace TapoMobileApp
{
    public interface IStoredProperties
    {
        bool ContainsKey(string key);
        string Get(string key);
        T Get<T>(string key);
        void Set(string key, string obj);
        void Set<T>(string key, T obj);
        void Clear();
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
                Application.Current.Properties[cacheProp] = null;
            }
        }

        public bool ContainsKey(string key)
        {
            return Application.Current.Properties.ContainsKey(key);
        }

        public string Get(string key)
        {
            return Application.Current.Properties[key].ToString();
        }

        public T Get<T>(string key)
        {
            if (!Application.Current.Properties.ContainsKey(key))
                return default;
            if (Application.Current.Properties[key] == null)
                return default;

            var json = Application.Current.Properties[key].ToString();
            if (string.IsNullOrEmpty(json))
                return default;
            return JsonConvert.DeserializeObject<T>(json);
        }

        public void Set(string key, string obj)
        {
            Application.Current.Properties.Remove(key);
            Application.Current.Properties.Add(key, obj);
            //await Application.Current.SavePropertiesAsync();
        }

        public void Set<T>(string key, T obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            Set(key, json);
            //await Application.Current.SavePropertiesAsync();
        }
    }
}