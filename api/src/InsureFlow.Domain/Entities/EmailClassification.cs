using InsureFlow.Domain.Enums;

namespace InsureFlow.Domain.Entities;

public sealed class EmailClassification
{
    public Guid Id { get; set; }
    public Guid EmailMessageId { get; set; }
    public EmailCategory Category { get; set; }
    public decimal ConfidenceScore { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public DateTimeOffset ClassifiedAt { get; set; }

    public EmailMessage EmailMessage { get; set; } = null!;
}
