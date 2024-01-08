using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Data.Logging
{
    public class DataAccessLoggingBuilder
    {
        private DataAccessLoggingOptions _dataAccessLoggingOptions = new DataAccessLoggingOptions();

        public DataAccessLoggingOptions Options
        {
            get => _dataAccessLoggingOptions;
            set => _dataAccessLoggingOptions = value ?? throw new ArgumentNullException(nameof(value));
        }

        public IServiceCollection Services { get; }

        public DataAccessLoggingBuilder(IServiceCollection services)
        {
            Guard.AgainstNull(services, nameof(services));

            Services = services;
        }
    }
}