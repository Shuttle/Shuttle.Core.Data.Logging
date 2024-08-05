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

        private void OnDatabaseContextCreated(object sender, DatabaseContextEventArgs e)
        {
            _logger.LogTrace($"[IDatabaseContextFactory.DatabaseContextCreated] : name = '{e.DatabaseContext.Name}' / managed thread id = {Thread.CurrentThread.ManagedThreadId}");

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

            _logger.LogTrace($"[DatabaseContext.Disposed] : Name = '{databaseContext.Name}' / managed thread id = {Thread.CurrentThread.ManagedThreadId}");

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

            _logger.LogTrace($"[DatabaseContext.TransactionCommitted] : Name = '{databaseContext.Name}' / managed thread id = {Thread.CurrentThread.ManagedThreadId}");
        }

        private void OnTransactionStarted(object sender, TransactionEventArgs e)
        {
            if (!(sender is IDatabaseContext databaseContext))
            {
                return;
            }

            _logger.LogTrace($"[DatabaseContext.TransactionStarted] : Name = '{databaseContext.Name}' / managed thread id = {Thread.CurrentThread.ManagedThreadId}");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_dataAccessLoggingOptions.DatabaseContext)
            {
                _databaseContextFactory.DatabaseContextCreated -= OnDatabaseContextCreated;
            }

            await Task.CompletedTask;
        }
    }
}