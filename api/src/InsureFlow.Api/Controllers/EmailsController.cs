using InsureFlow.Api.Extensions;
using InsureFlow.Application.DTOs;
using InsureFlow.Application.Interfaces;
using InsureFlow.Domain.Entities;
using InsureFlow.Domain.Enums;
using InsureFlow.Infrastructure.Data;
using InsureFlow.Infrastructure.External.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsureFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/emails")]
public sealed class EmailsController(
    InsureFlowDbContext dbContext,
    IGmailService gmailService,
    ITokenEncryptionService encryptionService,
    IAIClassificationService classificationService,
    GoogleOAuthService googleOAuthService,
    ILogger<EmailsController> logger) : ControllerBase
{
    [HttpPost("sync")]
    public async Task<IActionResult> Sync(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var mailbox = await dbContext.ConnectedMailboxes.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (mailbox is null)
        {
            return BadRequest(new { error = "Connect Gmail before syncing." });
        }

        var accessToken = encryptionService.Decrypt(mailbox.AccessToken);
        var refreshToken = encryptionService.Decrypt(mailbox.RefreshToken);
        if (!string.Equals(accessToken, refreshToken, StringComparison.Ordinal))
        {
            var refreshed = await googleOAuthService.RefreshAccessTokenAsync(refreshToken, cancellationToken);
            accessToken = refreshed.AccessToken;
            mailbox.AccessToken = encryptionService.Encrypt(accessToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var imported = await gmailService.GetLatestEmailsAsync(accessToken, 100, cancellationToken);
        var existingIds = await dbContext.EmailMessages
            .Where(x => x.MailboxId == mailbox.Id)
            .Select(x => x.ExternalMessageId)
            .ToListAsync(cancellationToken);
        var existing = existingIds.ToHashSet(StringComparer.Ordinal);

        var newMessages = imported
            .Where(x => !existing.Contains(x.ExternalMessageId))
            .Select(x => new EmailMessage
            {
                Id = Guid.NewGuid(),
                MailboxId = mailbox.Id,
                ExternalMessageId = x.ExternalMessageId,
                ThreadId = x.ThreadId,
                Sender = x.Sender,
                Subject = x.Subject,
                BodyPreview = x.BodyPreview,
                ReceivedAt = x.ReceivedAt,
                CreatedAt = DateTimeOffset.UtcNow
            })
            .ToList();

        dbContext.EmailMessages.AddRange(newMessages);
        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var message in newMessages)
        {
            var result = await classificationService.ClassifyAsync(message.Subject, message.BodyPreview);
            dbContext.EmailClassifications.Add(new EmailClassification
            {
                Id = Guid.NewGuid(),
                EmailMessageId = message.Id,
                Category = result.Category,
                ConfidenceScore = result.ConfidenceScore,
                Reasoning = result.Reasoning,
                ClassifiedAt = DateTimeOffset.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Gmail sync completed for user {UserId}. Imported {Count} new emails.", userId, newMessages.Count);

        return Ok(new { imported = newMessages.Count, totalChecked = imported.Count });
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EmailListItemDto>>> GetEmails(
        [FromQuery] EmailCategory? category,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var query = UserEmailQuery().OrderByDescending(x => x.ReceivedAt).AsQueryable();

        if (category is not null)
        {
            query = query.Where(x => x.Classification != null && x.Classification.Category == category);
        }

        if (from is not null)
        {
            query = query.Where(x => x.ReceivedAt >= from);
        }

        if (to is not null)
        {
            query = query.Where(x => x.ReceivedAt <= to);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => EF.Functions.ILike(x.Subject, $"%{search}%") || EF.Functions.ILike(x.Sender, $"%{search}%"));
        }

        var emails = await query.Take(100).Select(x => new EmailListItemDto(
            x.Id,
            x.Sender,
            x.Subject,
            x.ReceivedAt,
            x.Classification == null ? null : x.Classification.Category,
            x.Classification == null ? null : x.Classification.ConfidenceScore)).ToListAsync(cancellationToken);

        return Ok(emails);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmailDetailDto>> GetEmail(Guid id, CancellationToken cancellationToken)
    {
        var email = await UserEmailQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (email is null)
        {
            return NotFound();
        }

        return Ok(new EmailDetailDto(
            email.Id,
            email.Sender,
            email.Subject,
            email.ReceivedAt,
            email.BodyPreview,
            email.ThreadId,
            email.Classification?.Category,
            email.Classification?.ConfidenceScore,
            email.Classification?.Reasoning));
    }

    private IQueryable<EmailMessage> UserEmailQuery()
    {
        var userId = User.GetUserId();
        return dbContext.EmailMessages
            .Include(x => x.Classification)
            .Include(x => x.Mailbox)
            .Where(x => x.Mailbox.UserId == userId);
    }
}
