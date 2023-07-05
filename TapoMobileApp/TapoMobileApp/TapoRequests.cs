namespace TapoMobileApp
{
    public interface ICall
    {
        string Call();
    }
    public class LoginCall : ICall
    {
        public string method { get; set; }
        public Params @params { get; set; }

        public string Call()
        {
            return "Log in";
        }
    }

    public class Params
    {
        public bool hashed { get; set; }
        public string password { get; set; }
        public string username { get; set; }
    }

    public class PrivacyCall : ICall
    {
        public string method { get; set; }
        public LensMask lens_mask { get; set; }
        public string Call()
        {
            return "Privacy " + lens_mask.lens_mask_info.enabled;
        }
    }

    public class LensMask
    {
        public LensMaskInfo lens_mask_info { get; set; }
    }

    public class LensMaskInfo
    {
        public string enabled { get; set; }
    }

    public class PrivacyCheck : ICall
    {
        public string method { get; set; }
        public Lens_Mask lens_mask { get; set; }
        public string Call()
        {
            return "Privacy";
        }
    }

    public class Lens_Mask
    {
        public string[] name { get; set; }
    }
}