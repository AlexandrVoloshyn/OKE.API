using FluentResults;
using FluentValidation;
using MediatR;
using OKE.Application.Errors;
using OKE.Domain.Repositories;
using MovieDto = OKE.Application.Handlers.Movies.List.MovieDto;

namespace OKE.Application.Handlers.Actors;

// advantage that this abstract class have resposibility of getting an actor, and inside we have all necessary query, dto, validator, and handler.
// It's only an organization of code, of course we can do it based on filesystem. Means create a folder with 4 different classes inside ↑.
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

    public class Handler(IActorRepository ActorRepository) : IRequestHandler<Query, Result<ActorDto>>
    {
        public async Task<Result<ActorDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var actor = await ActorRepository.GetAsync(request.ActorName, cancellationToken);

            return Result.Ok(
                new ActorDto(
                    actor.FullName,
                    actor.Movies
                        .Select(x => new MovieDto(x.Title, x.Description))
                        .ToList()));
        }
    }
}
