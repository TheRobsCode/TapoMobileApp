namespace TapoMobileApp.Services.Storage
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
}
