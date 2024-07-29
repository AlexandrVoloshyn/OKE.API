using OKE.Domain.Models;

namespace OKE.Doamin.Models;

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public ICollection<Actor> Cast { get; set; }
}
