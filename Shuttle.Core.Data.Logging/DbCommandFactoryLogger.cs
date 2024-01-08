using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Data.Logging
{
    public class DbCommandFactoryLogger : IHostedService
    {
        private readonly IDbCommandFactory _dbCommandFactory;
        private readonly ILogger<DbCommandFactoryLogger> _logger;
        private readonly DataAccessLoggingOptions _dataAccessLoggingOptions;

        public DbCommandFactoryLogger(IOptions<DataAccessLoggingOptions> dataLoggingOptions, ILogger<DbCommandFactoryLogger> logger, IDbCommandFactory dbCommandFactory)
        {
            Guard.AgainstNull(dataLoggingOptions, nameof(dataLoggingOptions));

            _dataAccessLoggingOptions = Guard.AgainstNull(dataLoggingOptions.Value, nameof(dataLoggingOptions.Value));
            _logger = Guard.AgainstNull(logger, nameof(logger));
            _dbCommandFactory = Guard.AgainstNull(dbCommandFactory, nameof(dbCommandFactory));

            if (!_dataAccessLoggingOptions.DbCommandFactory ||
                !_logger.IsEnabled(LogLevel.Trace))
            {
                return;
            }

            _dbCommandFactory.DbCommandCreated += OnDbCommandCreated;
        }

        private void OnDbCommandCreated(object sender, DbCommandCreatedEventArgs e)
        {
            var parameters = new StringBuilder();

            if (e.DbCommand.Parameters != null && e.DbCommand.Parameters.Count > 0)
            {
                parameters.AppendLine();
                parameters.AppendLine("parameters:");

                foreach (IDataParameter dbCommandParameter in e.DbCommand.Parameters)
                {
                    parameters.AppendLine($"\t{dbCommandParameter.ParameterName} = {dbCommandParameter.Value}");

                }
            }

            _logger.LogTrace($"[IDbCommandFactory.DbCommandCreated] :-\n\rcommand type = '{e.DbCommand.CommandType}'\n\rcommand text\n\r---n\r{e.DbCommand.CommandText}\n\r---{parameters}");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_dataAccessLoggingOptions.DbCommandFactory)
            {
                _dbCommandFactory.DbCommandCreated -= OnDbCommandCreated;
            }
            
            return Task.CompletedTask;
        }
    }
}