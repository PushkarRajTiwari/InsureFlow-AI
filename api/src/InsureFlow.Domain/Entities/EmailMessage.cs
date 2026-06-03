namespace InsureFlow.Domain.Entities;

public sealed class EmailMessage
{
    public Guid Id { get; set; }
    public Guid MailboxId { get; set; }
    public string ExternalMessageId { get; set; } = string.Empty;
    public string ThreadId { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BodyPreview { get; set; } = string.Empty;
    public DateTimeOffset ReceivedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public ConnectedMailbox Mailbox { get; set; } = null!;
    public EmailClassification? Classification { get; set; }
}
