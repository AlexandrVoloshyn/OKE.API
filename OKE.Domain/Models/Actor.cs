using OKE.Doamin.Models;

namespace OKE.Domain.Models;

public class Actor
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public ICollection<Movie> Movies { get; set; }
}
