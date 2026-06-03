using InsureFlow.Domain.Enums;

namespace InsureFlow.Application.Common;

public sealed record EmailClassificationResult(
    EmailCategory Category,
    decimal ConfidenceScore,
    string Reasoning);
