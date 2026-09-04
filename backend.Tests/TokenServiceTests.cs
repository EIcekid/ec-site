using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EcSite.Api.Models;
using EcSite.Api.Services;
using Microsoft.Extensions.Configuration;

namespace EcSite.Api.Tests;

public class TokenServiceTests
{
    private static TokenService BuildSut()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Jwt:Key"] = "unit-test-only-super-secret-key-32bytes-min",
            })
            .Build();

        return new TokenService(config);
    }

    [Fact]
    public void GenerateToken_embeds_user_id_email_and_role_as_claims()
    {
        var sut = BuildSut();
        var user = new User { Id = 42, Email = "admin@ec-site.local", Name = "管理者", Role = UserRole.Admin };

        var token = sut.GenerateToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("42", jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("admin@ec-site.local", jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal("Admin", jwt.Claims.Single(c => c.Type == ClaimTypes.Role).Value);
        Assert.Equal("test-issuer", jwt.Issuer);
        Assert.Contains("test-audience", jwt.Audiences);
    }

    [Fact]
    public void GenerateToken_sets_customer_role_for_non_admin_users()
    {
        var sut = BuildSut();
        var user = new User { Id = 1, Email = "customer@ec-site.local", Name = "Customer", Role = UserRole.Customer };

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(sut.GenerateToken(user));

        Assert.Equal("Customer", jwt.Claims.Single(c => c.Type == ClaimTypes.Role).Value);
    }
}
