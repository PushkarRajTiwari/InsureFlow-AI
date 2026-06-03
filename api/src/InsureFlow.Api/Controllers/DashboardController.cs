using InsureFlow.Api.Extensions;
using InsureFlow.Application.DTOs;
using InsureFlow.Domain.Enums;
using InsureFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsureFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController(InsureFlowDbContext dbContext) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> Stats(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var query = dbContext.EmailMessages
            .Include(x => x.Classification)
            .Include(x => x.Mailbox)
            .Where(x => x.Mailbox.UserId == userId);

        return Ok(new DashboardStatsDto(
            await query.CountAsync(cancellationToken),
            await query.CountAsync(x => x.Classification != null && x.Classification.Category == EmailCategory.Claim, cancellationToken),
            await query.CountAsync(x => x.Classification != null && x.Classification.Category == EmailCategory.Billing, cancellationToken),
            await query.CountAsync(x => x.Classification != null && x.Classification.Category == EmailCategory.PolicyChange, cancellationToken),
            await query.CountAsync(x => x.Classification != null && x.Classification.Category == EmailCategory.CoverageQuestion, cancellationToken)));
    }
}
