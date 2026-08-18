namespace TapoMobileApp.Models.Responses.Authentication
{
    using TapoMobileApp.Models.Responses.Base;

    public class SecureLogin : IResult
    {
        public int error_code { get; set; }
        public SecureLoginResult result { get; set; }
        public bool IsSuccess()
        {
            return result.data?.nonce != null;
        }

        public string Result()
        {
            return "";
        }
    }
}
