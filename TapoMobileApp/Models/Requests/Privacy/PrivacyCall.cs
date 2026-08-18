namespace TapoMobileApp.Models.Requests.Privacy
{
    using TapoMobileApp.Models.Requests.Base;

    public class PrivacyCall : ICall
    {
        public string method { get; set; }
        public LensMask lens_mask { get; set; }
        public string Call()
        {
            return "Privacy " + lens_mask.lens_mask_info.enabled;
        }
    }
}
