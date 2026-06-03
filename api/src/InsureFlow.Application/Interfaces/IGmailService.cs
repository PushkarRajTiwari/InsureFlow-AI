using InsureFlow.Application.Common;

namespace InsureFlow.Application.Interfaces;

public interface IGmailService
{
    Task<IReadOnlyList<ImportedEmail>> GetLatestEmailsAsync(string accessToken, int maxResults, CancellationToken cancellationToken);
}
