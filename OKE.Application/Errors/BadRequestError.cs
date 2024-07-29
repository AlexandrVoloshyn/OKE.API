using FluentResults;

namespace OKE.Application.Errors;
public class BadRequestError : Error
{
    public BadRequestError(string message) : base(message)
    {
    }
}
