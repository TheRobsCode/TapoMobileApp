namespace TapoMobileApp.Events
{
    public class TapoServiceEvent : EventArgs
    {
        public string Message { get; set; }
        public int Port { get; set; }
    }
}
