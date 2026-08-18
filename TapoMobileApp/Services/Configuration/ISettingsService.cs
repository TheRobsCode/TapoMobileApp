namespace TapoMobileApp.Services.Configuration
{
    public interface ISettingsService
    {
        string UserName { get; }
        string Password { get; }
        string IpPrefix { get; }
    }
}
