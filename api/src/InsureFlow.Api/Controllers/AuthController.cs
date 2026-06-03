using InsureFlow.Application.DTOs;
using InsureFlow.Application.Interfaces;
using InsureFlow.Domain.Entities;
using InsureFlow.Infrastructure.Data;
using InsureFlow.Infrastructure.External.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsureFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    GoogleOAuthService googleOAuthService,
    InsureFlowDbContext dbContext,
    IJwtTokenService jwtTokenService,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("google")]
    public async Task<ActionResult<AuthResponse>> Google([FromBody] GoogleAuthRequest request, CancellationToken cancellationToken)
    {
        var tokens = await googleOAuthService.ExchangeCodeAsync(request.Code, cancellationToken);
        var profile = await googleOAuthService.GetProfileAsync(tokens.AccessToken, cancellationToken);

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.GoogleId == profile.Id, cancellationToken);
        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                GoogleId = profile.Id,
                Email = profile.Email,
                Name = profile.Name,
                CreatedAt = DateTimeOffset.UtcNow
            };
            dbContext.Users.Add(user);
        }
        else
        {
            user.Email = profile.Email;
            user.Name = profile.Name;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("User {Email} logged in with Google", user.Email);

        return Ok(new AuthResponse(jwtTokenService.CreateToken(user), new UserDto(user.Id, user.Email, user.Name)));
    }
}
