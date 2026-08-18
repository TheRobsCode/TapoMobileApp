using TapoMobileApp.Events;
using TapoMobileApp.Models.Requests.Base;
using TapoMobileApp.Models.Responses.Base;

namespace TapoMobileApp.Services.Http
{
    public interface ITapoHttpClient
    {
        Task<TResult> DoTapoCommand<TResult, TCall>(int port, TCall callObj) where TCall : ICall
            where TResult : IResult;
        Task<LoginCache> DoLogin(int port, bool useCache);
        Task<LoginCache> DoLogin(int port);
        event EventHandler<TapoServiceEvent> OnChanged;
        Task<bool> Ping(int port);
    }
}
