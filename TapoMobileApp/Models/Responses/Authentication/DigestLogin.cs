namespace TapoMobileApp.Models.Responses.Authentication
{
    using TapoMobileApp.Models.Responses.Base;

    public class DigestLogin : IResult
    {
        public int error_code { get; set; }
        public DigestLoginResult result { get; set; }

        public bool IsSuccess()
        {
            return error_code > 0;
        }

        public string Result()
        {
            return "Logged In";
        }
    }
}
