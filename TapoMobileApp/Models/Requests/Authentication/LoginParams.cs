namespace TapoMobileApp.Models.Requests.Authentication
{
    public class LoginParams
    {
        public bool hashed { get; set; }
        public string password { get; set; }
        public string username { get; set; }
    }
}
