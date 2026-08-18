namespace TapoMobileApp.Models.Requests.Secure
{
    public class MultipleRequest<T>
    {
        public string method { get; set; } = "multipleRequest";
        public MultipleRequestParams<T> @params { get; set; }
    }
}
