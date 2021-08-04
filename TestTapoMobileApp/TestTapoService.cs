using FakeItEasy;
using System;
using System.Linq;
using System.Threading.Tasks;
using TapoMobileApp;
using Xunit;

namespace TestTapoMobileApp
{
    public class TestTapoService
    {
        [Fact]
        public async Task TestChangeState()
        {
            var fakeHttpClient = A.Fake<ITapoHttpClient>();
            A.CallTo(fakeHttpClient)
            .Where(call => call.Method.Name == "DoTapoCommand").WithNonVoidReturnType().Returns(Task.FromResult( new TapoResult() { result = new Result() { stok = "test" } }));
            var tapoService = new TapoService(fakeHttpClient);
            var result = await tapoService.ChangeState(new[] { 1 }, true);

            Assert.True(result.Count == 0);
        }
        [Fact]
        public async Task TestChangeManyStates()
        {
            var fakeHttpClient = A.Fake<ITapoHttpClient>();
            A.CallTo(fakeHttpClient)
            .Where(call => call.Method.Name == "DoTapoCommand").WithNonVoidReturnType().Returns(Task.FromResult(new TapoResult() { result = new Result() { stok = "test" } }));
            var tapoService = new TapoService(fakeHttpClient);
            
            var result = await tapoService.ChangeState(Enumerable.Range(1, 250).ToArray(), true);

            Assert.True(result.Count == 0);
        }
    }
}
