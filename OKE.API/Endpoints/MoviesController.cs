using MediatR;
using OKE.API.Filters;
using static OKE.Application.Handlers.Movies.List;

namespace OKE.API.Endpoints;

public static class Movies
{
    public static void MapMoviesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(nameof(Movies).ToLower()) // better to have a convertor to kebab case
            .WithTags(nameof(Movies))
            .AddEndpointFilter<ResultFilter>();

        group.MapGet(
            "",
            (IMediator mediator) => mediator.Send(new Query()))
        .Produces<MovieDto[]>()
        .Produces<DetailedError>(404)
        .Produces<DetailedError>(400)
        .WithDisplayName("ListMovies");
    }
}