using FluentAssertions;
using ServiceStarter.Api.Features.Users.Create;
using Xunit;

namespace ServiceStarter.UnitTests.Features.Users.Create;

public sealed class CreateUserRequestValidatorTests
{
    private readonly CreateUserRequestValidator _validator = new();

    [Fact]
    public void WhenRequestIsValid_ShouldPassValidation()
    {
        var request = new CreateUserRequest("user@example.com", "Jane Doe");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void WhenEmailIsMissing_ShouldFailValidation()
    {
        var request = new CreateUserRequest(string.Empty, "Jane Doe");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void WhenDisplayNameIsMissing_ShouldFailValidation()
    {
        var request = new CreateUserRequest("user@example.com", string.Empty);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }
}