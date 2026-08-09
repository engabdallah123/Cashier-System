using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Audit.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAuditApplication(this IServiceCollection services)
        {
            services.AddMediatR(configuration =>
            {
                configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            });

            services.AddValidatorsFromAssembly(
                typeof(DependencyInjection).Assembly,
                includeInternalTypes: true);

            return services;
        }
    }
}
