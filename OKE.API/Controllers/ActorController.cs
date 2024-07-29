using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static OKE.Application.Handlers.Actors.Get;

namespace OKE.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ActorController : ControllerBase
{
    private readonly IMediator _mediator;

    public ActorController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{name}", Name = "GetActor")]
    public async Task<Result<ActorDto>> GetAsync(string name)
    {
        return await _mediator.Send(new Query(name));
    }
}
