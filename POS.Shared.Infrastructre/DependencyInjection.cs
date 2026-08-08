using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using POS.Shared.Application.Database;
using POS.Shared.Application.IService;
using POS.Shared.Infrastructure.Database;
using POS.Shared.Infrastructure.Services;
using POS.Shared.Infrastructure.Database;

namespace POS.Shared.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));

            #region Dapper
            services.AddSingleton<ISqlConnectionFactory>(_ => new SqlConnectionFactory(connectionString));
            #endregion

            services.AddScoped<IFileService, FileService>();

            return services;
        }
    }
}
