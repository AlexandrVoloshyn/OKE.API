using FluentResults;

namespace OKE.Application.Errors;
public class BadRequestError(string message) : Error(message);
