using System.Text.Json.Serialization;

namespace TapoMobileApp
{
    //{"method":"securePassthrough","params":{"request":"secure"}}
    public class SecurePassthrough : ICall
    {
        public string method { get; set; } = "securePassthrough";
        [JsonPropertyName("params")]
        public SecureParams @params { get; set; }

        public string Call()
        {
            return "Privacy";
        }
    }

    public class SecureParams
    {
        public string request { get; set; }
    }


    //{"method":"multipleRequest","params":{"requests":[{"method":"getLensMaskConfig","params":{"lens_mask":{"name":["lens_mask_info"]}}}]}}
    public class MultipleRequest<T>
    {
        public string method { get; set; } = "multipleRequest";
        public MultipleRequestParams<T> @params { get; set; }
        //public List<T> @params { get; set; }
    }
    //var request = "{\"method\":\"multipleRequest\",\"params\":{\"requests\":[{\"method\":\"getLensMaskConfig\",\"params\":{\"lens_mask\":{\"name\":[\"lens_mask_info\"]}}}]}}";

    public class MultipleRequestParams<T>
    {
        public List<T> requests { get; set; }
    }

    public class SecureLoginCall : ICall
    {
        public string method { get; set; } = "login";
        public SecureLoginParams @params { get; set; }
        public string Call()
        {
            return "Log in";
        }
    }
    public class SecureLoginParams
    {
        public string cnonce { get; set; }
        public int encrypt_type { get; set; }
        public string username { get; set; }
    }

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

    public class SecureLoginResult
    {
        public SecureLoginData data { get; set; }
    }

    public class SecureLoginData
    {
        public int code { get; set; }
        public string[] encrypt_type { get; set; }
        public string key { get; set; }
        public string nonce { get; set; }
        public string device_confirm { get; set; }
    }
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

    public class DigestLoginResult
    {
        public string stok { get; set; }
        public string user_group { get; set; }
        public int start_seq { get; set; }
    }
    public class DigestLoginRequest : ICall
    {
        public string method { get; set; } = "login";
        public DigestLoginParams @params { get; set; }

        public string Call()
        {
            return "Digest Login";
        }
    }
    public class DigestLoginParams
    {
        public string digest_passwd { get; set; }

        public string cnonce { get; set; }
        public int encrypt_type { get; set; } = 3;
        public string username { get; set; } = "admin";
    }

    public class SecureChangePrivacyCall : ICall
    {
        public string method { get; set; } = "setLensMaskConfig";
        public SecureChangePrivacyParams @params { get; set; } = new SecureChangePrivacyParams();

        public string Call()
        {
            return "Privacy";
        }
    }


    public class SecureChangePrivacyParams
    {
        public SecureLensMask lens_mask { get; set; }
    }

    public class SecureLensMask
    {
        public SecureLensMaskInfo lens_mask_info { get; set; }
    }

    public class SecureLensMaskInfo
    {
        public string enabled { get; set; }
    }


    public class SecurePrivacyCall : ICall
    {
        public string method { get; set; } = "getLensMaskConfig";
        public SecurePrivacyParams @params { get; set; } = new SecurePrivacyParams();

        public string Call()
        {
            return "Privacy";
        }
    }

    public class SecurePrivacyParams
    {
        public SecureLens_Mask lens_mask { get; set; } = new SecureLens_Mask();
    }

    public class SecureLens_Mask
    {
        public string[] name { get; set; } = new[] { "lens_mask_info" };
    }


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

    public class SecureData
    {
        public string response { get; set; }
    }
    //public class SecureDataResponse
    //{
    //    public string msg { get; set; }
    //    public int error_code { get; set; }
    //}


    public class CheckPrivacyDecrypted : IResult
    {
        public CheckPrivacyDecryptedResult result { get; set; }
        public int error_code { get; set; }

        public bool IsSuccess()
        {
            return error_code >= 0 && result.responses[0].result.lens_mask != null && result.responses[0].result.lens_mask.lens_mask_info != null;
        }

        public string Result()
        {
            if (result.responses.Length > 0 && !string.IsNullOrEmpty(result.responses[0].msg))
                return result.responses[0].msg;
            return result.responses[0].result.lens_mask.lens_mask_info.enabled;
        }
    }

    public class CheckPrivacyDecryptedResult
    {
        public CheckPrivacyDecryptedResponse[] responses { get; set; }
    }

    public class CheckPrivacyDecryptedResponse
    {
        public string method { get; set; }
        public CheckPrivacyDecryptedData result { get; set; }
        public int error_code { get; set; }
        public string msg { get; set; }
    }

    public class CheckPrivacyDecryptedData
    {
        public Lens_MaskResult lens_mask { get; set; }
    }




}