namespace TapoMobileApp
{
    public class TapoResult
    {
        public int error_code { get; set; }
        public Result result { get; set; }
    }

    public class Result
    {
        public string stok { get; set; }
        public string user_group { get; set; }
    }

    public class PrivacyCheckResult
    {
        public Lens_MaskResult lens_mask { get; set; }
        public int error_code { get; set; }
    }

    public class Lens_MaskResult
    {
        public Lens_Mask_Info lens_mask_info { get; set; }
    }

    public class Lens_Mask_Info
    {
        public string name { get; set; }
        public string type { get; set; }
        public string enabled { get; set; }
    }
}