using System;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;
using TapoMobileApp;

namespace TestTapoMobileApp
{
    public class TestTapoService
    {
        private readonly IStoredProperties _storedProperties;
        public TestTapoService()
        {
            _storedProperties = new StoredProperties();
        }
        [Fact]
        public async Task TestChangeState()
        {
            var fakeHttpClient = A.Fake<ITapoHttpClient>();
            A.CallTo(fakeHttpClient)
                .Where(call => call.Method.Name == "DoTapoCommand").WithNonVoidReturnType()
                .Returns(Task.FromResult(new TapoResult {result = new Result {stok = "test"}}));
            var tapoService = new TapoService(fakeHttpClient, _storedProperties);
            //var result = await tapoService.ChangeState(new[] {1}, true);

            //Assert.True(result.Count == 0);
        }

        [Fact]
        public async Task TestChangeManyStates()
        {
            var fakeSettingsService = A.Fake<ISettingsService>();
            A.CallTo(() => fakeSettingsService.UserName).Returns("UserName");
            A.CallTo(() => fakeSettingsService.Password).Returns("Password");
            var fakeStoredProperties = A.Fake<IStoredProperties>();
            A.CallTo(fakeStoredProperties)
                .Where(call => call.Method.Name == "Get").WithNonVoidReturnType().Returns(new LoginCache
                { Stok = "Stok", ExpiryDate = DateTime.Now.AddDays(1) });

            A.CallTo(() => fakeStoredProperties.ContainsKey(A<string>.Ignored)).Returns(true);
            var client = new MockTapoHttpClient(fakeSettingsService, fakeStoredProperties);
            client.SetFakeCommandReturns("HappyPathCachedLogin");
            var message = "";
            var numCalls = 0;
            client.OnChanged += (o, h) =>
            {
                message = h.Message;
                numCalls++;
            };
            
            var tapoService = new TapoService(client, _storedProperties);

            await tapoService.ChangeState(Enumerable.Range(1, 250).ToArray(), true);
            Assert.Equal("Privacy on ", message);
            Assert.Equal(750, numCalls);
        }

        [Fact]
        public async Task TestCheckState()
        {
            var fakeHttpClient = A.Fake<ITapoHttpClient>();
            A.CallTo(fakeHttpClient)
                .Where(call => call.Method.Name == "DoTapoCommand").WithNonVoidReturnType().Returns(
                    Task.FromResult(new PrivacyCheckResult
                        {lens_mask = new Lens_MaskResult {lens_mask_info = new Lens_Mask_Info {enabled = "on"}}}));
            var tapoService = new TapoService(fakeHttpClient, _storedProperties);
            await tapoService.CheckState(new[] {1});

            //Assert.True(result.Count == 1);
            //var portDetails = result.First();
            //Assert.Equal(1, portDetails.Port);
            //Assert.Equal("Privacy on", portDetails.Message);
            //Assert.Equal("1- Privacy on", result.First());
        }
    }
}