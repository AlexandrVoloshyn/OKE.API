using OKE.Doamin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OKE.Domain.Repositories;
public interface IMovieRepository
{
    Task<List<Movie>> GetMoviesAsync(CancellationToken token);
}
