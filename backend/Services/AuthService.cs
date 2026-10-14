using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Configuration;

namespace TaskFlow.Api.Services;

public class AuthService : IAuthService
{
    private readonly JwtOptions _jwtOptions;
    private readonly string _email; 
    private readonly string _password;

    public AuthService(JwtOptions jwtOptions, IConfiguration configuration)
    {
        _jwtOptions = jwtOptions;
        _email = configuration["Auth:Email"] ?? throw new InvalidOperationException("Auth:Email is missing");
        _password = configuration["Auth:Password"] ?? throw new InvalidOperationException("Auth: Password is missing");
    }

    public LoginResponse? Login(LoginRequest logReq)
    {
        var validEmail = string.Equals(logReq.Email.Trim(), _email, StringComparison.OrdinalIgnoreCase);
        var validPassword = string.Equals(logReq.Password, _password, StringComparison.Ordinal);

        if (!validEmail || !validPassword)
        {
            return null;
        }

        var now = DateTime.UtcNow;

        var expiresAt = now.AddMinutes(_jwtOptions.ExpiryMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "admin"),
            new Claim(JwtRegisteredClaimNames.Email, _email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAt
        };   
    }

}