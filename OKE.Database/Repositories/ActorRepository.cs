using Microsoft.EntityFrameworkCore;
using OKE.Domain.Models;
using OKE.Domain.Repositories;

namespace OKE.Database.Repositories;
public class ActorRepository : IActorRepository
{
    private readonly Context _context;

    public ActorRepository(Context context)
    {
        _context = context;
    }

    public Task<bool> AnyAsync(string name, CancellationToken token) =>
        _context.Actors.AnyAsync(x => x.FullName == name);

    public Task<Actor> GetAsync(string name, CancellationToken token) =>
        _context.Actors.Include(x => x.Movies).FirstAsync(x => x.FullName == name, token);
}
