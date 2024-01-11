using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Data.Logging
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataAccessLogging(this IServiceCollection services, Action<DataAccessLoggingBuilder> builder = null)
        {
            Guard.AgainstNull(services, nameof(services));

            var serviceBusLoggingBuilder = new DataAccessLoggingBuilder(services);

            builder?.Invoke(serviceBusLoggingBuilder);

            services.AddOptions<DataAccessLoggingOptions>().Configure(options =>
            {
                options.DatabaseContext = serviceBusLoggingBuilder.Options.DatabaseContext;
                options.DbCommandFactory = serviceBusLoggingBuilder.Options.DbCommandFactory;
            });

            services.AddHostedService<DatabaseContextLogger>();
            services.AddHostedService<DbCommandFactoryLogger>();

            return services;
        }
    }
}
