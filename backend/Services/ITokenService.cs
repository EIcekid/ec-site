using EcSite.Api.Models;

namespace EcSite.Api.Services;

public interface ITokenService
{
    string GenerateToken(User user);
}
