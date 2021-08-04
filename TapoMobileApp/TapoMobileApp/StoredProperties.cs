using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace TapoMobileApp
{
    public interface IStoredProperties
    {
        bool ContainsKey(string key);
        object Get(string key);
        void Set(string key, object obj);
    }
    public class StoredProperties : IStoredProperties
    {
        public bool ContainsKey(string key)
        {
            return Application.Current.Properties.ContainsKey(key);
        }

        public object Get(string key)
        {
            return Application.Current.Properties[key];
        }

        public void Set(string key, object obj)
        {
            Application.Current.Properties.Remove(key);
            Application.Current.Properties.Add(key, obj);
        }
    }
}
