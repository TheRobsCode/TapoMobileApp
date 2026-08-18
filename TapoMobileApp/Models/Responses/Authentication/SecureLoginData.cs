namespace TapoMobileApp.Models.Responses.Authentication
{
    public class SecureLoginData
    {
        public int code { get; set; }
        public string[] encrypt_type { get; set; }
        public string key { get; set; }
        public string nonce { get; set; }
        public string device_confirm { get; set; }
    }
}
