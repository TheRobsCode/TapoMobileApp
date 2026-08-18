namespace TapoMobileApp.Models.Responses.Secure
{
    public class CheckPrivacyDecryptedResponse
    {
        public string method { get; set; }
        public CheckPrivacyDecryptedData result { get; set; }
        public int error_code { get; set; }
        public string msg { get; set; }
    }
}
