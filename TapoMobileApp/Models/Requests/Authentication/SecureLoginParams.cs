namespace TapoMobileApp.Models.Requests.Authentication
{
    public class SecureLoginParams
    {
        public string cnonce { get; set; }
        public int encrypt_type { get; set; }
        public string username { get; set; }
    }
}
