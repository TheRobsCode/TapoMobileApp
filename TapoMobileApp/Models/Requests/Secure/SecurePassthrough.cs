using System.Text.Json.Serialization;

namespace TapoMobileApp.Models.Requests.Secure
{
    using TapoMobileApp.Models.Requests.Base;

    public class SecurePassthrough : ICall
    {
        public string method { get; set; } = "securePassthrough";
        [JsonPropertyName("params")]
        public SecureParams @params { get; set; }

        public string Call()
        {
            return "Privacy";
        }
    }
}
