namespace ServiceStarter.Api.Infrastructure.Persistence.Entities;

public sealed class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = Auth.AuthRoles.User;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
