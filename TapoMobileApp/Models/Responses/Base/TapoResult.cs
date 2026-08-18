namespace TapoMobileApp.Models.Responses.Base
{
    using TapoMobileApp.Models.Responses.Authentication;

    public class TapoResult : IResult
    {
        public int error_code { get; set; }
        public LoginResult result { get; set; }

        public bool IsSuccess()
        {
            return error_code >= 0;
        }

        public string Result()
        {
            return "";
        }
    }
}
