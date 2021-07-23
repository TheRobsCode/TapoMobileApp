
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
}
