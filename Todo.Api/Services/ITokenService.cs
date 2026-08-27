using Todo.Api.Models;

namespace Todo.Api.Services;

public interface ITokenService
{
    string GenerateToken(User user);
}
