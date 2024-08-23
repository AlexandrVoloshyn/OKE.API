using FluentResults;
using MediatR;
using OKE.Domain.Repositories;

namespace OKE.Application.Handlers.Movies;

public abstract class List
{
    public record Query : IRequest<Result<List<MovieDto>>>;
    public record MovieDto(string Title, string Description);

    public class Handler(IMovieRepository MovieRepository) : IRequestHandler<Query, Result<List<MovieDto>>>
    {
        public async Task<Result<List<MovieDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var movies = await MovieRepository.GetMoviesAsync(cancellationToken);
            var movieDtos = movies.ConvertAll(x => new MovieDto(x.Title, x.Description));

            return Result.Ok(movieDtos);
        }
    }
}
