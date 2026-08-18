namespace TapoMobileApp.Models.Responses.Privacy
{
    using TapoMobileApp.Models.Responses.Base;

    public class PrivacyCheckResult : IResult
    {
        public LensMaskResult lens_mask { get; set; }
        public int error_code { get; set; }

        public bool IsSuccess()
        {
            return error_code >= 0 && lens_mask != null && lens_mask.lens_mask_info != null;
        }

        public string Result()
        {
            if (lens_mask == null || lens_mask.lens_mask_info == null)
                return "Error";
            return lens_mask.lens_mask_info.enabled;
        }

    }
}
