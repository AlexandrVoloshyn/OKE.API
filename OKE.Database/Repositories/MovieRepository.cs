using Microsoft.EntityFrameworkCore;
using OKE.Doamin.Models;
using OKE.Domain.Repositories;

namespace OKE.Database.Repositories;
public class MovieRepository : IMovieRepository
{
    private readonly Context _context;

    public MovieRepository(Context context)
    {
        _context = context;
    }

    public Task<List<Movie>> GetMoviesAsync(CancellationToken token) =>
        _context.Movies.AsNoTracking().ToListAsync(token);
}
