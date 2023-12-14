namespace TapoMobileApp
{
    public interface IResult
    {
        string Result();
        bool IsSuccess();
    }
    public class TapoResult : IResult
    {
        public int error_code { get; set; }
        public Result result { get; set; }

        public bool IsSuccess()
        {
            return error_code >= 0;
        }

        public string Result()
        {
            return "";
        }
    }

    public class Result
    {
        public string stok { get; set; }
        public string user_group { get; set; }
    }

    public class PrivacyCheckResult : IResult
    {
        public Lens_MaskResult lens_mask { get; set; }
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