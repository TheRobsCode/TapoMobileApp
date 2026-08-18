using TapoMobileApp.Models.Requests.Secure;
using TapoMobileApp.Models.Responses.Base;
using TapoMobileApp.Models.Responses.Secure;
using TapoMobileApp.Services.Http;
using TapoMobileApp.Services.Storage;

namespace TapoMobileApp.Services.Tapo
{
    public class TapoSecureService : TapoService
    {
        public TapoSecureService(ITapoHttpClient tapoHttpClient, IStoredProperties storedProperties) : base(tapoHttpClient, storedProperties)
        {
        }

        protected override async Task LoginAndCheckPrivacy(int port)
        {
            try
            {
                await CheckPrivacy(port);
            }
            catch (Exception ex)
            {
                _storedProperties.StoreLog($"Error in {nameof(LoginAndCheckPrivacy)} for port {port}: {ex.Message}{ex.InnerException}");
            }
        }

        protected override async Task LoginAndChangePrivacy(int port, bool toggleOnOrOff, List<int> errors)
        {
            try
            {
                await ChangePrivacy(port, toggleOnOrOff);
            }
            catch (Exception ex)
            {
                _storedProperties.StoreLog($"Error in {nameof(LoginAndChangePrivacy)} for port {port}: {ex.Message}{ex.InnerException}");
            }
        }

        private async Task CheckPrivacy(int port)
        {
            var obj = new SecurePrivacyCall();

            await _httpClient.DoTapoCommand<CheckPrivacyDecrypted, SecurePrivacyCall>(port, obj);
        }

        protected override async Task<bool> ChangePrivacy(int port, bool toggleOnOrOff)
        {
            var obj = new SecureChangePrivacyCall
            { @params = new SecureChangePrivacyParams { lens_mask = new SecureLensMask { lens_mask_info = new SecureLensMaskInfo { enabled = "off" } } } };
            if (toggleOnOrOff) obj.@params.lens_mask.lens_mask_info.enabled = "on";
            var ret = await _httpClient.DoTapoCommand<TapoResult, SecureChangePrivacyCall>(port, obj);
            return ret.IsSuccess();
        }


    }
}
