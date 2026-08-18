namespace TapoMobileApp.Models.Requests.Secure
{
    using TapoMobileApp.Models.Requests.Base;

    public class SecurePrivacyCall : ICall
    {
        public string method { get; set; } = "getLensMaskConfig";
        public SecurePrivacyParams @params { get; set; } = new SecurePrivacyParams();

        public string Call()
        {
            return "Privacy";
        }
    }
}
