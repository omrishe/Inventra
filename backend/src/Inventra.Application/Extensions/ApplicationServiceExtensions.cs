using Inventra.Application.Interfaces;
using Inventra.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Inventra.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
