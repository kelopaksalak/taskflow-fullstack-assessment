using TaskFlow.Api.DTOs;

namespace TaskFlow.Api.Services;

public interface IAuthService
{
    LoginResponse? Login(LoginRequest logReq);
}