namespace TapoMobileApp.Services.Http
{
    public class LoginCache
    {
        public string Stok { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string CNonce { get; set; }
        public string Nonce { get; set; }
        public int Seq { get; set; }
    }
}
