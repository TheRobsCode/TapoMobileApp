using Newtonsoft.Json;

namespace TapoMobileApp.Utilities.Serialization
{
    public static class Json
    {
        public static string Serialize<T>(T obj) =>
            JsonConvert.SerializeObject(obj);

        public static T Deserialize<T>(string s) =>
            JsonConvert.DeserializeObject<T>(s);
    }
}
