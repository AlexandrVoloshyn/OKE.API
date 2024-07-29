using FluentResults;
using MediatR;
using OKE.Domain.Repositories;
using StackExchange.Redis;
using System.Text.Json;

namespace OKE.Application.Handlers.Movies;

public abstract class List
{
    public record Query : IRequest<Result<List<MovieDto>>>;
    public record MovieDto(string Title, string Description);

    public class Handler : IRequestHandler<Query, Result<List<MovieDto>>>
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private const string _cacheKey = "movie_list";

        public Handler(IMovieRepository movieRepository, IConnectionMultiplexer connectionMultiplexer)
        {
            _movieRepository = movieRepository;
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async Task<Result<List<MovieDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Better to create service which handling the cache
            var db = _connectionMultiplexer.GetDatabase();
            var cachedMovies = await db.StringGetAsync(_cacheKey);
            if (!cachedMovies.IsNullOrEmpty)
            {
                var movieDtos = JsonSerializer.Deserialize<List<MovieDto>>(cachedMovies!);
                return Result.Ok(movieDtos!);
            }

            var movies = await _movieRepository.GetMoviesAsync(cancellationToken);
            var movieDtosFromDb = movies.ConvertAll(x => new MovieDto(x.Title, x.Description));

            await db.StringSetAsync(_cacheKey, JsonSerializer.Serialize(movieDtosFromDb));

            return Result.Ok(movieDtosFromDb);
        }
    }
}
