namespace TapoMobileApp.Models.Responses.Secure
{
    using TapoMobileApp.Models.Responses.Base;

    public class CheckPrivacyDecrypted : IResult
    {
        public CheckPrivacyDecryptedResult result { get; set; }
        public int error_code { get; set; }

        public bool IsSuccess()
        {
            return error_code >= 0
                && result?.responses != null
                && result.responses.Length > 0
                && result.responses[0]?.result?.lens_mask?.lens_mask_info != null;
        }

        public string Result()
        {
            if (result?.responses != null && result.responses.Length > 0 && !string.IsNullOrEmpty(result.responses[0].msg))
                return result.responses[0].msg;
            return result?.responses?[0]?.result?.lens_mask?.lens_mask_info?.enabled ?? string.Empty;
        }
    }
}
