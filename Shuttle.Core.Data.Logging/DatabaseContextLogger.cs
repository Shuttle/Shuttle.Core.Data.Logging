using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Data.Logging
{
    public class DatabaseContextLogger : IHostedService
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly ILogger<DatabaseContextLogger> _logger;
        private readonly DataAccessLoggingOptions _dataAccessLoggingOptions;

        public DatabaseContextLogger(IOptions<DataAccessLoggingOptions> dataLoggingOptions, ILogger<DatabaseContextLogger> logger, IDatabaseContextFactory databaseContextFactory)
        {
            Guard.AgainstNull(dataLoggingOptions, nameof(dataLoggingOptions));

            _dataAccessLoggingOptions = Guard.AgainstNull(dataLoggingOptions.Value, nameof(dataLoggingOptions.Value));
            _logger = Guard.AgainstNull(logger, nameof(logger));
            _databaseContextFactory = Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));

            if (!_dataAccessLoggingOptions.DatabaseContext)
            {
                return;
            }

            _databaseContextFactory.DatabaseContextCreated += OnDatabaseContextCreated;
        }

        private void OnDatabaseContextReferenced(object sender, DatabaseContextEventArgs e)
        {
            _logger.LogTrace($"[IDatabaseContextFactory.DatabaseContextReference] : name = '{e.DatabaseContext.Name}' / key = '{e.DatabaseContext.Key}'");

            e.DatabaseContext.TransactionStarted += OnTransactionStarted;
            e.DatabaseContext.TransactionCommitted += OnTransactionCommitted;
            e.DatabaseContext.Disposed += OnDisposed;
        }

        private void OnDatabaseContextCreated(object sender, DatabaseContextEventArgs e)
        {
            _logger.LogTrace($"[IDatabaseContextFactory.DatabaseContextCreated] : name = '{e.DatabaseContext.Name}' / key = '{e.DatabaseContext.Key}' / managed thread id = {Thread.CurrentThread.ManagedThreadId}");

            e.DatabaseContext.TransactionStarted += OnTransactionStarted;
            e.DatabaseContext.TransactionCommitted += OnTransactionCommitted;
            e.DatabaseContext.Disposed += OnDisposed;
        }

        private void OnDisposed(object sender, EventArgs e)
        {
            if (!(sender is IDatabaseContext databaseContext))
            {
                return;
            }

            _logger.LogTrace($"[DatabaseContext.Disposed] : Name = '{databaseContext.Name}' / Key = '{databaseContext.Key}' / managed thread id = {Thread.CurrentThread.ManagedThreadId}");

            databaseContext.TransactionStarted -= OnTransactionStarted;
            databaseContext.TransactionCommitted -= OnTransactionCommitted;
            databaseContext.Disposed -= OnDisposed;
        }

        private void OnTransactionCommitted(object sender, TransactionEventArgs e)
        {
            if (!(sender is IDatabaseContext databaseContext))
            {
                return;
            }

            _logger.LogTrace($"[DatabaseContext.TransactionCommitted] : Name = '{databaseContext.Name}' / Key = ' {databaseContext.Key}'");
        }

        private void OnTransactionStarted(object sender, TransactionEventArgs e)
        {
            if (!(sender is IDatabaseContext databaseContext))
            {
                return;
            }

            _logger.LogTrace($"[DatabaseContext.TransactionStarted] : Name = '{databaseContext.Name}' / Key = ' {databaseContext.Key}'");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_dataAccessLoggingOptions.DatabaseContext)
            {
                _databaseContextFactory.DatabaseContextCreated -= OnDatabaseContextCreated;
            }
            
            return Task.CompletedTask;
        }
    }
}