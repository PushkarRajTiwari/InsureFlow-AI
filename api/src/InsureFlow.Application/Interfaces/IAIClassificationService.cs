using InsureFlow.Application.Common;

namespace InsureFlow.Application.Interfaces;

public interface IAIClassificationService
{
    Task<EmailClassificationResult> ClassifyAsync(string subject, string body);
}
