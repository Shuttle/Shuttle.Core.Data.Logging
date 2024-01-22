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
            var message = new StringBuilder();

            message.AppendLine("[IDbCommandFactory.DbCommandCreated] :-");
            message.AppendLine($"\tcommand type = '{e.DbCommand.CommandType}'");
            message.AppendLine("\tcommand text:");
            message.AppendLine("---");
            message.AppendLine($"{e.DbCommand.CommandText}");
            message.AppendLine("---");

            if (e.DbCommand.Parameters != null && e.DbCommand.Parameters.Count > 0)
            {
                message.AppendLine();
                message.AppendLine("\tparameters:");

                foreach (IDataParameter dbCommandParameter in e.DbCommand.Parameters)
                {
                    message.AppendLine($"\t\t{dbCommandParameter.ParameterName} = {dbCommandParameter.Value}");

                }
            }

            _logger.LogTrace(message.ToString());
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_dataAccessLoggingOptions.DbCommandFactory)
            {
                _dbCommandFactory.DbCommandCreated -= OnDbCommandCreated;
            }
            
            await Task.CompletedTask;
        }
    }
}