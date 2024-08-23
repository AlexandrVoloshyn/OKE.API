using MediatR;
using OKE.API.Filters;
using static OKE.Application.Handlers.Actors.Get;

namespace OKE.API.Endpoints;

public static class Actors
{
    public static void MapActorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(nameof(Actors).ToLower()) // better to have a convertor to kebab case
            .WithTags(nameof(Actors))
            .AddEndpointFilter<ResultFilter>();

        group.MapGet(
            "{name}",
            (IMediator mediator, string name) => mediator.Send(new Query(name)))
        .Produces<ActorDto>()
        .Produces<DetailedError>(404)
        .Produces<DetailedError>(400)
        .WithDisplayName("GetActor");
    }
}