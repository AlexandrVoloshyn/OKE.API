using Microsoft.EntityFrameworkCore;
using OKE.Doamin.Models;
using OKE.Domain.Models;

namespace OKE.Database;
public class Context : DbContext
{
    public Context(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Movie> Movies { get; set; }
    public DbSet<Actor> Actors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Movie>(builder =>
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Title).IsRequired();

            builder.HasMany(e => e.Cast).WithMany(e => e.Movies);

            builder.HasData(new Movie[]
            {
                new Movie()
                {
                    Id = -1,
                    Title = "Matrix",
                    Description = "The Matrix is a 1999 science fiction action film written and directed by the Wachowskis.",
                },
                new Movie()
                {
                    Id = -2,
                    Title = "John Wick",
                    Description = "John Wick, a retired hitman, is forced to return to his old ways after a group of Russian gangsters steal his car and kill a puppy gifted to him by his late wife.",
                },
                new Movie()
                {
                    Id = -3,
                    Title = "Boyz n the Hood",
                    Description = "Three childhood friends, Darrin, Tre and Ricky, who struggle to cope with the distractions and dangers of growing up in a Los Angeles ghetto.",
                }
            });
        });

        modelBuilder.Entity<Actor>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.HasMany(e => e.Movies).WithMany(e => e.Cast);
            builder.Property(e => e.FullName).IsRequired();

            builder.HasData(new Actor[] {
                new Actor() {
                    Id = -1,
                    FullName = "Keanu Reeves",
                },
                new Actor()
                {
                    Id = -2,
                    FullName = "Laurence Fishburne",
                }
            });
        });
    }
}
