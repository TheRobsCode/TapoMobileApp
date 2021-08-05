using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace TapoMobileApp
{
    public interface ITapoService
    {
        Task<List<string>> CheckState(int[] ports);
        Task<List<int>> ChangeState(int[] ports, bool toggleOnOrOff);
        Task<int[]> Scan();
    }

    public class TapoService : ITapoService
    {
        private readonly ITapoHttpClient _httpClient;

        public TapoService(ITapoHttpClient tapoHttpClient)
        {
            _httpClient = tapoHttpClient;
        }

        public async Task<List<int>> ChangeState(int[] ports, bool toggleOnOrOff)
        {
            var errors = new List<int>();
            var tasks = new List<Task>();
            foreach (var port in ports) tasks.Add(LoginAndChangePrivacy(port, toggleOnOrOff, errors));
            await Task.WhenAll(tasks);
            return errors;
        }

        public async Task<List<string>> CheckState(int[] ports)
        {
            var results = new List<string>();
            var tasks = new List<Task>();
            foreach (var port in ports) tasks.Add(LoginAndCheckPrivacy(port, results));
            await Task.WhenAll(tasks);

            return await Task.FromResult(results);
        }

        public async Task<int[]> Scan()
        {
            var result = new List<int>();
            var tasks = new List<Task>();
            for (var port = 2; port < 254; port++) tasks.Add(ScanPort(result, port));
            await Task.WhenAll(tasks);
            return await Task.FromResult(result.ToArray());
        }

        private async Task LoginAndCheckPrivacy(int port, List<string> results)
        {
            try
            {
                var checkPrivacy = await CheckPrivacy(port);
                results.Add(port + "- Privacy " + (checkPrivacy ? "on" : "off"));
            }
            catch (Exception e)
            {
            }
        }

        private async Task LoginAndChangePrivacy(int port, bool toggleOnOrOff, List<int> errors)
        {
            try
            {
                var changePrivacy = await ChangePrivacy(port, toggleOnOrOff);
                if (!changePrivacy)
                    errors.Add(port);
            }
            catch (Exception e)
            {
                errors.Add(port);
            }
        }

        private async Task<bool> CheckPrivacy(int port)
        {
            var obj = new PrivacyCheck {method = "get", lens_mask = new Lens_Mask {name = new[] {"lens_mask_info"}}};

            var result = await _httpClient.DoTapoCommand<PrivacyCheckResult, PrivacyCheck>(port, obj);
            return result.lens_mask.lens_mask_info.enabled == "on";
        }

        private async Task<bool> ChangePrivacy(int port, bool toggleOnOrOff)
        {
            var obj = new PrivacyCall
                {method = "set", lens_mask = new LensMask {lens_mask_info = new LensMaskInfo {enabled = "off"}}};
            if (toggleOnOrOff) obj.lens_mask.lens_mask_info.enabled = "on";
            var ret = await _httpClient.DoTapoCommand<TapoResult, PrivacyCall>(port, obj);
            if (ret == null)
                return false;
            return true;
        }

        private bool Ping(string url)
        {
            var p = new Ping();
            PingReply r;
            r = p.Send(url, 1000);

            return r.Status == IPStatus.Success;
        }

        private async Task<bool> ScanPort(List<int> result, int port)
        {
            try
            {
                var url = "192.168.1." + port;
                if (!Ping(url))
                    return false;

                var login = await _httpClient.DoLogin(port, false);
                if (!string.IsNullOrEmpty(login))
                {
                    result.Add(port);
                    return true;
                }
            }
            catch (Exception e)
            {
            }

            return false;
        }
    }
}