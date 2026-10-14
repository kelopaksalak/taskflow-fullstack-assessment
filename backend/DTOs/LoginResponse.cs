namespace TaskFlow.Api.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }
}