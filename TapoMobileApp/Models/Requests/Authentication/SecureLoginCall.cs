namespace TapoMobileApp.Models.Requests.Authentication
{
    using TapoMobileApp.Models.Requests.Base;

    public class SecureLoginCall : ICall
    {
        public string method { get; set; } = "login";
        public SecureLoginParams @params { get; set; }
        public string Call()
        {
            return "Log in";
        }
    }
}
