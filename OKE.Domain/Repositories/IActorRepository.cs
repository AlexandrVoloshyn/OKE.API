using OKE.Domain.Models;

namespace OKE.Domain.Repositories;
public interface IActorRepository
{
    Task<bool> AnyAsync(string name, CancellationToken token);
    Task<Actor> GetAsync(string name, CancellationToken token);
}
