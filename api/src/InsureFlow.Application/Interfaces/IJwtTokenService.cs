using InsureFlow.Domain.Entities;

namespace InsureFlow.Application.Interfaces;

public interface IJwtTokenService
{
    string CreateToken(User user);
}
