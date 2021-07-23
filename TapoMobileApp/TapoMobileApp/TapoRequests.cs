namespace TapoMobileApp
{
    public class LoginCall
    {
        public string method { get; set; }
        public Params @params { get; set; }
    }

    public class Params
    {
        public bool hashed { get; set; }
        public string password { get; set; }
        public string username { get; set; }
    }

    public class PrivacyCall
    {
        public string method { get; set; }
        public LensMask lens_mask { get; set; }
    }

    public class LensMask
    {
        public LensMaskInfo lens_mask_info { get; set; }
    }
    public class LensMaskInfo
    {
        public string enabled { get; set; }
    }
}
