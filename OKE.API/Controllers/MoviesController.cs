using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static OKE.Application.Handlers.Movies.List;

namespace OKE.API.Controllers;
[ApiController]
[Route("[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MoviesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet(Name = "ListMovies")]
    public async Task<Result<List<MovieDto>>> GetAsync()
    {
        return await _mediator.Send(new Query());
    }
}
