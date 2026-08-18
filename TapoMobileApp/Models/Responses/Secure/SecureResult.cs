using TapoMobileApp.Utilities.Cryptography;
using TapoMobileApp.Utilities.Serialization;

namespace TapoMobileApp.Models.Responses.Secure
{
    using TapoMobileApp.Models.Responses.Base;

    public class SecureResult<T> : IResult
    {
        public int error_code { get; set; }
        public int seq { get; set; }
        public SecureData result { get; set; }

        public bool IsSuccess()
        {
            return error_code >= 0;
        }

        public string Result()
        {
            return "";
        }
        public bool TryGetResult(byte[] lsk, byte[] ivb, out T res)
        {
            res = default;
            if (result == null || result.response == null)
                return false;

            var from64Bit = Convert.FromBase64String(result.response);
            var decrypt = CryptoServices.Decrypt(from64Bit, lsk, ivb);
            res = Json.Deserialize<T>(decrypt);
            return true;
        }
    }
}
