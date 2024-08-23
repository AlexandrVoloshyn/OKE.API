using FluentAssertions;
using FluentValidation.TestHelper;
using NSubstitute;
using OKE.Application.Errors;
using OKE.Doamin.Models;
using OKE.Domain.Models;
using OKE.Domain.Repositories;
using static OKE.Application.Handlers.Actors.Get;
using static OKE.Application.Handlers.Movies.List;
using Handler = OKE.Application.Handlers.Actors.Get.Handler;
using Query = OKE.Application.Handlers.Actors.Get.Query;

namespace OKE.Application.Tests.Handlers.Actors;
public class GetTests
{
    private readonly IActorRepository _actorRepository;

    private readonly Validator _validator;
    private readonly Handler _handler;

    public GetTests()
    {
        _actorRepository = Substitute.For<IActorRepository>();
        _handler = new Handler(_actorRepository);
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
    public async Task HandleShouldReturnActorFromDatabase()
    {
        // Arrange
        var query = new Query("NewActor");
        var actor = new Actor { FullName = "NewActor", Movies = [new() { Title = "Movie1", Description = "Description1" }] };
        var actorDto = new ActorDto("NewActor", [new("Movie1", "Description1") ]);
        _actorRepository.GetAsync(query.ActorName, Arg.Any<CancellationToken>()).Returns(actor);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(actorDto);
        await _actorRepository.Received(1).GetAsync(Arg.Is("NewActor"), Arg.Any<CancellationToken>());
    }
}
