using FluentAssertions;
using ServiceStarter.Api.Features.Auth.Login;
using Xunit;

namespace ServiceStarter.UnitTests.Features.Auth.Login;

public sealed class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void WhenRequestIsValid_ShouldPassValidation()
    {
        var request = new LoginRequest("admin@admin.local", "password123");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void WhenEmailIsInvalid_ShouldFailValidation()
    {
        var request = new LoginRequest("not-an-email", "password123");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void WhenPasswordIsEmpty_ShouldFailValidation()
    {
        var request = new LoginRequest("user@example.com", string.Empty);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }
}