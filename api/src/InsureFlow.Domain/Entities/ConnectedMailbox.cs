namespace InsureFlow.Domain.Entities;

public sealed class ConnectedMailbox
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<EmailMessage> EmailMessages { get; set; } = new List<EmailMessage>();
}
