using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Data.Logging;

public class DataAccessLoggingBuilder
{
    private DataAccessLoggingOptions _dataAccessLoggingOptions = new();

    public DataAccessLoggingBuilder(IServiceCollection services)
    {
        Services = Guard.AgainstNull(services);
    }

    public DataAccessLoggingOptions Options
    {
        get => _dataAccessLoggingOptions;
        set => _dataAccessLoggingOptions = value ?? throw new ArgumentNullException(nameof(value));
    }

    public IServiceCollection Services { get; }
}