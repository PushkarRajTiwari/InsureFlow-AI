using InsureFlow.Api.Extensions;
using InsureFlow.Application.DTOs;
using InsureFlow.Application.Interfaces;
using InsureFlow.Domain.Entities;
using InsureFlow.Infrastructure.Data;
using InsureFlow.Infrastructure.External.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsureFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/mailbox")]
public sealed class MailboxController(
    GoogleOAuthService googleOAuthService,
    InsureFlowDbContext dbContext,
    ITokenEncryptionService encryptionService,
    ILogger<MailboxController> logger) : ControllerBase
{
    [HttpPost("connect")]
    public async Task<IActionResult> Connect([FromBody] MailboxConnectRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var tokens = await googleOAuthService.ExchangeCodeAsync(request.Code, cancellationToken);
        var profile = await googleOAuthService.GetProfileAsync(tokens.AccessToken, cancellationToken);

        var mailbox = await dbContext.ConnectedMailboxes.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (mailbox is null)
        {
            mailbox = new ConnectedMailbox
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTimeOffset.UtcNow
            };
            dbContext.ConnectedMailboxes.Add(mailbox);
        }

        mailbox.EmailAddress = profile.Email;
        mailbox.AccessToken = encryptionService.Encrypt(tokens.AccessToken);
        mailbox.RefreshToken = encryptionService.Encrypt(tokens.RefreshToken ?? tokens.AccessToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Gmail mailbox connected for user {UserId}", userId);

        return Ok(new { mailbox.EmailAddress });
    }
}
