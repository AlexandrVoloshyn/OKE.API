using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OKE.Application.Errors;
public class ProcessingError : Error
{
    public const string ErrorCode = "500";

    public ProcessingError(string message) : base(message)
    {
    }
}
