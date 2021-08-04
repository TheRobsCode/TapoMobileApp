using FakeItEasy;
using System;
using System.Linq;
using System.Threading.Tasks;
using TapoMobileApp;
using Xunit;

namespace TestTapoMobileApp
{
    public class TestTapoHttpClient
    {
        private ISettingsService _fakeSettingsService;
        public TestTapoHttpClient()
        {
            _fakeSettingsService = A.Fake<ISettingsService>();
            A.CallTo(() => _fakeSettingsService.UserName).Returns("UserName");
            A.CallTo(() => _fakeSettingsService.Password).Returns("Password");
        }
        [Fact]
        public async Task TestChangeStateHappyPathCachedLogin()
        {
            var fakeStoredProperties = A.Fake<IStoredProperties>();
            A.CallTo(() => fakeStoredProperties.Get(A<string>.Ignored)).Returns(new LoginCache { Stok = "Stok", ExpiryDate = DateTime.Now.AddDays(1)});
            A.CallTo(() => fakeStoredProperties.ContainsKey(A<string>.Ignored)).Returns(true);

            var client = new MockTapoHttpClient(_fakeSettingsService, fakeStoredProperties);
            client.SetFakeCommandReturns("HappyPathCachedLogin");
            var obj = new PrivacyCall { method = "set", lens_mask = new LensMask { lens_mask_info = new LensMaskInfo { enabled = "off" } } };
            var ret = await client.DoTapoCommand<TapoResult, PrivacyCall>(1, obj);

            Assert.NotNull(ret);
            Assert.Equal(0, ret.error_code);
        }
        [Fact]
        public async Task TestChangeStateHappyPathNoCachedLogin()
        {
            var fakeStoredProperties = A.Fake<IStoredProperties>();
            A.CallTo(() => fakeStoredProperties.Get(A<string>.Ignored)).Returns(new LoginCache { Stok = "Stok", ExpiryDate = DateTime.Now.AddDays(-1) });
            A.CallTo(() => fakeStoredProperties.ContainsKey(A<string>.Ignored)).Returns(true);

            var client = new MockTapoHttpClient(_fakeSettingsService, fakeStoredProperties);
            client.SetFakeCommandReturns("HappyPathNoCachedLogin");
            var obj = new PrivacyCall { method = "set", lens_mask = new LensMask { lens_mask_info = new LensMaskInfo { enabled = "off" } } };
            var ret = await client.DoTapoCommand<TapoResult, PrivacyCall>(1, obj);

            Assert.NotNull(ret);
            Assert.Equal(0, ret.error_code);
            A.CallTo(() => fakeStoredProperties.Set(A<string>.Ignored, A<object>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public async Task TestChangeStateLoginFails()
        {
            var fakeStoredProperties = A.Fake<IStoredProperties>();
            A.CallTo(() => fakeStoredProperties.Get(A<string>.Ignored)).Returns(new LoginCache { Stok = "Stok", ExpiryDate = DateTime.Now.AddDays(-1) });
            A.CallTo(() => fakeStoredProperties.ContainsKey(A<string>.Ignored)).Returns(false);

            var client = new MockTapoHttpClient(_fakeSettingsService, fakeStoredProperties);
            client.SetFakeCommandReturns("LoginFails");
            var obj = new PrivacyCall { method = "set", lens_mask = new LensMask { lens_mask_info = new LensMaskInfo { enabled = "off" } } };
            var ret = await client.DoTapoCommand<TapoResult, PrivacyCall>(1, obj);

            Assert.Null(ret);
            A.CallTo(() => fakeStoredProperties.Set(A<string>.Ignored, A<object>.Ignored)).MustNotHaveHappened();
        }
        [Fact]
        public async Task TestChangeStateCachedLoginFails()
        {
            var fakeStoredProperties = A.Fake<IStoredProperties>();
            A.CallTo(() => fakeStoredProperties.Get(A<string>.Ignored)).Returns(new LoginCache { Stok = "Stok", ExpiryDate = DateTime.Now.AddDays(1) });
            A.CallTo(() => fakeStoredProperties.ContainsKey(A<string>.Ignored)).Returns(true);

            var client = new MockTapoHttpClient(_fakeSettingsService, fakeStoredProperties);
            client.SetFakeCommandReturns("LoginFails");
            var obj = new PrivacyCall { method = "set", lens_mask = new LensMask { lens_mask_info = new LensMaskInfo { enabled = "off" } } };
            var ret = await client.DoTapoCommand<TapoResult, PrivacyCall>(1, obj);

            Assert.Null(ret);
            A.CallTo(() => fakeStoredProperties.Set(A<string>.Ignored, A<object>.Ignored)).MustNotHaveHappened();
        }
    }
}
