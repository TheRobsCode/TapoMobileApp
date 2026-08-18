namespace TapoMobileApp
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
            catch (Exception e)
            {
                _storedProperties.StoreLog(e.Message + e.InnerException);
            }
        }

        protected override async Task LoginAndChangePrivacy(int port, bool toggleOnOrOff, List<int> errors)
        {
            try
            {
                await ChangePrivacy(port, toggleOnOrOff);
            }
            catch (Exception e)
            {
                _storedProperties.StoreLog(e.Message + e.InnerException);
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