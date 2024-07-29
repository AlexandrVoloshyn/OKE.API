using FluentResults;
using FluentValidation;
using MediatR;
using OKE.Application.Errors;
using OKE.Domain.Repositories;
using StackExchange.Redis;
using System.Text.Json;
using MovieDto = OKE.Application.Handlers.Movies.List.MovieDto;

namespace OKE.Application.Handlers.Actors;

// Know that it's not single responsibility, but in this case it's much quicker to write code when everything in one place.
// Also prefere to have handler and validation in on class, to share already read entities from validators to handler. But it's too much for test project.
public abstract class Get
{
    public record Query(string ActorName) : IRequest<Result<ActorDto>>;

    public record ActorDto(string ActorName, List<MovieDto> Movie);

    public class Validator : AbstractValidator<Query>
    {
        public Validator(IActorRepository actorRepository)
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            RuleFor(c => c.ActorName)
                .NotNull()
                    .WithMessage("{PropertyName} is required.")
                .NotEmpty()
                    .WithMessage("{PropertyName} can not be empty.")
                .MustAsync(actorRepository.AnyAsync)
                .WithMessage("Actor not found.")
                .WithErrorCode(NotFoundError.ErrorCode);
        }
    }

    public class Handler : IRequestHandler<Query, Result<ActorDto>>
    {
        private readonly IActorRepository _actorRepository;
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public Handler(IActorRepository actorRepository, IConnectionMultiplexer connectionMultiplexer)
        {
            _actorRepository = actorRepository;
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async Task<Result<ActorDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var cacheKey = $"actor_{request.ActorName}";

            var cachedActor = await db.StringGetAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedActor))
            {
                var actorDto = JsonSerializer.Deserialize<ActorDto>(cachedActor!);
                return Result.Ok(actorDto!);
            }

            var actor = await _actorRepository.GetAsync(request.ActorName, cancellationToken);
            var actorDtoFromDb = new ActorDto(
                actor.FullName,
                actor.Movies
                    .Select(x => new MovieDto(x.Title, x.Description))
                    .ToList());

            await db.StringSetAsync(cacheKey, JsonSerializer.Serialize(actorDtoFromDb));

            return Result.Ok(actorDtoFromDb);
        }
    }
}
