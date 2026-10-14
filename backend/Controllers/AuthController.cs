using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public ActionResult<LoginResponse> Login(
        LoginRequest request)
    {
        var response = _authService.Login(request);

        if (response is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Login failed.",
                detail: "Invalid email or password.");
        }

        return Ok(response);
    }
}