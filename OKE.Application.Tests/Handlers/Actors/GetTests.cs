using NSubstitute.Core;
using NSubstitute;
using OKE.Domain.Repositories;
using FluentValidation.TestHelper;
using static OKE.Application.Handlers.Actors.Get;
using Query = OKE.Application.Handlers.Actors.Get.Query;
using Handler = OKE.Application.Handlers.Actors.Get.Handler;
using FluentAssertions;
using OKE.Application.Errors;
using StackExchange.Redis;
using OKE.Doamin.Models;
using OKE.Domain.Models;
using static OKE.Application.Handlers.Movies.List;
using System.Text.Json;

namespace OKE.Application.Tests.Handlers.Actors;
public class GetTests
{
    private readonly IActorRepository _actorRepository;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDatabase _database;

    private readonly Validator _validator;
    private readonly Handler _handler;

    public GetTests()
    {
        _actorRepository = Substitute.For<IActorRepository>();
        _connectionMultiplexer = Substitute.For<IConnectionMultiplexer>();
        _database = Substitute.For<IDatabase>();
        _connectionMultiplexer.GetDatabase().Returns(_database);
        _handler = new Handler(_actorRepository, _connectionMultiplexer);
        _validator = new Validator(_actorRepository);
    }

    [Fact]
    public void ShouldHaveErrorWhenActorNameIsNull()
    {
        // Arrange
        var query = new Query(null!);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.ActorName)
              .WithErrorMessage("Actor Name is required.");
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Actor Name is required.");
    }

    [Fact]
    public void ShouldHaveErrorWhenActorNameIsEmpty()
    {
        // Arrange
        var query = new Query("");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.ActorName)
              .WithErrorMessage("Actor Name can not be empty.");
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Actor Name can not be empty.");
    }

    [Fact]
    public async Task ShouldHaveErrorWhenActorNotFound()
    {
        // Arrange
        var query = new Query("NonExistentActor");
        _actorRepository.AnyAsync("NonExistentActor", Arg.Any<CancellationToken>())
                        .Returns(Task.FromResult(false));

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.ActorName)
              .WithErrorMessage("Actor not found.")
              .WithErrorCode(NotFoundError.ErrorCode);
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Actor not found." && e.ErrorCode == NotFoundError.ErrorCode);
    }

    [Fact]
    public async Task ShouldNotHaveErrorWhenActorExists()
    {
        // Arrange
        var query = new Query("ExistingActor");
        _actorRepository.AnyAsync("ExistingActor", Arg.Any<CancellationToken>())
                        .Returns(Task.FromResult(true));

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.ActorName);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleShouldReturnCachedActor()
    {
        // Arrange
        var query = new Query("ExistingActor");
        var cachedActorDto = new ActorDto("ExistingActor", new List<MovieDto>());
        var cachedActorJson = JsonSerializer.Serialize(cachedActorDto);
        _database.StringGetAsync(Arg.Is<RedisKey>("actor_ExistingActor")).Returns(cachedActorJson);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(cachedActorDto);
        await _actorRepository.DidNotReceive().GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleShouldReturnActorFromDatabaseAndCacheIt()
    {
        // Arrange
        var query = new Query("NewActor");
        var actor = new Actor { FullName = "NewActor", Movies = new List<Movie> { new Movie { Title = "Movie1", Description = "Description1" } } };
        var actorDto = new ActorDto("NewActor", new List<MovieDto> { new MovieDto("Movie1", "Description1") });
        _database.StringGetAsync(Arg.Is<RedisKey>("actor_ExistingActor")).Returns(new RedisValue());
        _actorRepository.GetAsync(query.ActorName, Arg.Any<CancellationToken>()).Returns(actor);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(actorDto);
        await _actorRepository.Received(1).GetAsync(Arg.Is<string>("NewActor"), Arg.Any<CancellationToken>());
        await _database.Received(1).StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>());
    }
}
