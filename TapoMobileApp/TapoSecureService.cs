namespace TapoMobileApp
{
    
    public class TapoSecureService : TapoService
    {
        public TapoSecureService(ITapoHttpClient tapoHttpClient, IStoredProperties storedProperties) : base(tapoHttpClient, storedProperties)
        { 
        }


        public async Task CheckState(int[] ports)
        {
            //var results = new List<TapoServiceEvent>();
            var tasks = new List<Task>();
            foreach (var port in ports)
            {
                tasks.Add(LoginAndCheckPrivacy(port));
            }
            await Task.WhenAll(tasks);

            //return await Task.FromResult(results);
        }


        protected override async Task LoginAndCheckPrivacy(int port)
        {
            try
            {
                await CheckPrivacy(port);
            }
            catch (Exception e)
            {
            }
        }

        protected override async Task LoginAndChangePrivacy(int port, bool toggleOnOrOff, List<int> errors)
        {

            await ChangePrivacy(port, toggleOnOrOff);

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