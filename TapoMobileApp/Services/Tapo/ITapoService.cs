using TapoMobileApp.Events;

namespace TapoMobileApp.Services.Tapo
{
    public interface ITapoService
    {
        Task CheckState(int[] ports);
        Task ChangeState(int[] ports, bool toggleOnOrOff);
        Task<int[]> Scan();
        Task Initialize(int[] ports);
        event EventHandler<TapoServiceEvent> OnChanged;
    }
}
