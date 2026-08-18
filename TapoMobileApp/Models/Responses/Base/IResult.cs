namespace TapoMobileApp.Models.Responses.Base
{
    public interface IResult
    {
        string Result();
        bool IsSuccess();
    }
}
