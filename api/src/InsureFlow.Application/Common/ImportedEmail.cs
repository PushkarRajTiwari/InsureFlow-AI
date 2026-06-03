namespace InsureFlow.Application.Common;

public sealed record ImportedEmail(
    string ExternalMessageId,
    string ThreadId,
    string Sender,
    string Subject,
    string BodyPreview,
    DateTimeOffset ReceivedAt);
