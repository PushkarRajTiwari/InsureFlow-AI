using InsureFlow.Domain.Enums;

namespace InsureFlow.Application.DTOs;

public sealed record AuthResponse(string Token, UserDto User);
public sealed record UserDto(Guid Id, string Email, string Name);
public sealed record EmailListItemDto(Guid Id, string Sender, string Subject, DateTimeOffset Date, EmailCategory? Category, decimal? Confidence);
public sealed record EmailDetailDto(Guid Id, string Sender, string Subject, DateTimeOffset Date, string BodyPreview, string ThreadId, EmailCategory? Category, decimal? Confidence, string? Reasoning);
public sealed record DashboardStatsDto(int TotalEmails, int Claims, int Billing, int PolicyChanges, int CoverageQuestions);
