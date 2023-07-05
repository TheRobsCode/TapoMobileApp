using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace TapoMobileApp
{
    public class TapoServiceEvent : EventArgs
    {
        public string Message { get; set; }
        public int Port { get; set; }
    }
    public interface ITapoService
    {
        Task CheckState(int[] ports);
        Task ChangeState(int[] ports, bool toggleOnOrOff);
        Task<int[]> Scan();
        Task Initialize(int[] ports);
        event EventHandler<TapoServiceEvent> OnChanged;
    }

    public class TapoService : ITapoService
    {
        public event EventHandler<TapoServiceEvent> OnChanged;

        private readonly ITapoHttpClient _httpClient;
        private readonly IStoredProperties _storedProperties;

        public TapoService(ITapoHttpClient tapoHttpClient, IStoredProperties storedProperties)
        {
            _storedProperties = storedProperties;
            _httpClient = tapoHttpClient;
        }
        public async Task Initialize(int[] ports)
        {
            if (ports == null || !ports.Any())
                return;
            foreach(var port in ports)
            {
                await _httpClient.DoLogin(port);
            }
        }
        public async Task ChangeState(int[] ports, bool toggleOnOrOff)
        {
            var errors = new List<int>();
            var tasks = new List<Task>();
            foreach (var port in ports)
            {
                tasks.Add(LoginAndChangePrivacy(port, toggleOnOrOff, errors));
            }
            await Task.WhenAll(tasks);
            //return errors;
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

        public async Task<int[]> Scan()
        {
            var result = new List<int>();
            var tasks = new List<Task>();
            for (var port = 2; port < 254; port++)
            {
                tasks.Add(ScanPort(result, port));
            }
            await Task.WhenAll(tasks);
            return await Task.FromResult(result.ToArray());
        }

        private async Task LoginAndCheckPrivacy(int port)
        {
            try
            {
                await CheckPrivacy(port);
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

        private async Task CheckPrivacy(int port)
        {
            var obj = new PrivacyCheck {method = "get", lens_mask = new Lens_Mask {name = new[] {"lens_mask_info"}}};

            await _httpClient.DoTapoCommand<PrivacyCheckResult, PrivacyCheck>(port, obj);
        }

        private async Task<bool> ChangePrivacy(int port, bool toggleOnOrOff)
        {
            var obj = new PrivacyCall
                {method = "set", lens_mask = new LensMask {lens_mask_info = new LensMaskInfo {enabled = "off"}}};
            if (toggleOnOrOff) obj.lens_mask.lens_mask_info.enabled = "on";
            var ret = await _httpClient.DoTapoCommand<TapoResult, PrivacyCall>(port, obj);
            return ret.success;
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