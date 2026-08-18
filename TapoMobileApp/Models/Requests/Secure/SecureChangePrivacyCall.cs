namespace TapoMobileApp.Models.Requests.Secure
{
    using TapoMobileApp.Models.Requests.Base;

    public class SecureChangePrivacyCall : ICall
    {
        public string method { get; set; } = "setLensMaskConfig";
        public SecureChangePrivacyParams @params { get; set; } = new SecureChangePrivacyParams();

        public string Call()
        {
            return "Privacy";
        }
    }
}
