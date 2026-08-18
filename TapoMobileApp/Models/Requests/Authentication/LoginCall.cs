namespace TapoMobileApp.Models.Requests.Authentication
{
    using TapoMobileApp.Models.Requests.Base;

    public class LoginCall : ICall
    {
        public string method { get; set; }
        public LoginParams @params { get; set; }

        public string Call()
        {
            return "Log in";
        }
    }
}
