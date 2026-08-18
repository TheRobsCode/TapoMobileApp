namespace TapoMobileApp.Models.Requests.Authentication
{
    public class DigestLoginParams
    {
        public string digest_passwd { get; set; }

        public string cnonce { get; set; }
        public int encrypt_type { get; set; } = 3;
        public string username { get; set; } = "admin";
    }
}
