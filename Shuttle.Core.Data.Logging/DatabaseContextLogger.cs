using System;
using System.Diagnostics.Tracing;
using System.Linq;
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
        private readonly IDatabaseContextService _databaseContextService;
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly ILogger<DatabaseContextLogger> _logger;
        private readonly DataAccessLoggingOptions _dataAccessLoggingOptions;

        public DatabaseContextLogger(IOptions<DataAccessLoggingOptions> dataLoggingOptions, ILogger<DatabaseContextLogger> logger, IDatabaseContextFactory databaseContextFactory, IDatabaseContextService databaseContextService)
        {
            Guard.AgainstNull(dataLoggingOptions, nameof(dataLoggingOptions));

            _dataAccessLoggingOptions = Guard.AgainstNull(dataLoggingOptions.Value, nameof(dataLoggingOptions.Value));
            _logger = Guard.AgainstNull(logger, nameof(logger));
            _databaseContextFactory = Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            _databaseContextService = Guard.AgainstNull(databaseContextService, nameof(databaseContextService));

            if (!_dataAccessLoggingOptions.DatabaseContext)
            {
                return;
            }

            _databaseContextFactory.DatabaseContextCreated += OnDatabaseContextCreated;
            _databaseContextService.DatabaseContextAsyncLocalValueChanged += DatabaseContextAsyncLocalValueChanged;
            _databaseContextService.DatabaseContextAsyncLocalValueAssigned += OnDatabaseContextAsyncLocalValueAssigned;
        }

        private void OnDatabaseContextAsyncLocalValueAssigned(object sender, DatabaseContextAsyncLocalValueAssignedEventArgs e)
        {
            _logger.LogTrace($"[database-context/async-local-value-assigned] : active name = '{e.AmbientData.ActiveDatabaseContext?.Name ?? "(no active database context)"}' / database context count = {e.AmbientData.DatabaseContexts.Count()} / managed thread id = {Thread.CurrentThread.ManagedThreadId}");
        }

        private void DatabaseContextAsyncLocalValueChanged(object sender, DatabaseContextAsyncLocalValueChangedEventArgs e)
        {
            _logger.LogTrace($"[database-context/async-local-value-changed] : current name = '{e.AsyncLocalValueChangedArgs.CurrentValue?.ActiveDatabaseContext?.Name}' / current key = '{e.AsyncLocalValueChangedArgs.CurrentValue?.ActiveDatabaseContext?.Key}' / previous name = '{e.AsyncLocalValueChangedArgs.PreviousValue?.ActiveDatabaseContext?.Name}' / previous key = '{e.AsyncLocalValueChangedArgs.PreviousValue?.ActiveDatabaseContext?.Key}' / managed thread id = {Thread.CurrentThread.ManagedThreadId} / thread context changed = {e.AsyncLocalValueChangedArgs.ThreadContextChanged}");
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