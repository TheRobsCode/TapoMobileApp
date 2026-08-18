namespace TapoMobileApp.Models.Requests.Privacy
{
    using TapoMobileApp.Models.Requests.Base;

    public class PrivacyCheck : ICall
    {
        public string method { get; set; }
        public LensMaskName lens_mask { get; set; }
        public string Call()
        {
            return "Privacy";
        }
    }
}
