using InsureFlow.Application.Interfaces;
using InsureFlow.Infrastructure.Auth;
using InsureFlow.Infrastructure.Data;
using InsureFlow.Infrastructure.External.Google;
using InsureFlow.Infrastructure.External.OpenAI;
using InsureFlow.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace InsureFlow.Api.Extensions;

public static class InfrastructureRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InsureFlowDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        services.AddHttpClient<GoogleOAuthService>();
        services.AddHttpClient<IGmailService, GmailService>();
        services.AddHttpClient<IAIClassificationService, OpenAIClassificationService>();
        services.AddScoped<ITokenEncryptionService, TokenEncryptionService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
