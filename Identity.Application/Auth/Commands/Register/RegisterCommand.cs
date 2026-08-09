using POS.Shared.Application.Messaging;

namespace Identity.Application.Auth.Commands.Register
{
    public sealed record RegisterCommand(
        string FullName,
        string UserName,
        string Email,
        string Password,
        string? Phone,
        string Role) : ICommand<AuthResponse>;
}
