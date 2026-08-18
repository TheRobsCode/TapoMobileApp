namespace TapoMobileApp.Models.Requests.Authentication
{
    using TapoMobileApp.Models.Requests.Base;

    public class DigestLoginRequest : ICall
    {
        public string method { get; set; } = "login";
        public DigestLoginParams @params { get; set; }

        public string Call()
        {
            return "Digest Login";
        }
    }
}
