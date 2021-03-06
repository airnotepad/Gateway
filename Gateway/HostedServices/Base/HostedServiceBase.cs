namespace Gateway.HostedServices.Base
{
    /// <summary>
    /// Background service (Hosted service) as base for all services
    /// </summary>
    public abstract class HostedServiceBase : IHostedService
    {
        private Task? _executingTask;
        private readonly CancellationTokenSource _stoppingCancellationTokenSource = new CancellationTokenSource();

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            _executingTask = ExecuteAsync(_stoppingCancellationTokenSource.Token);
            return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                _stoppingCancellationTokenSource.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        protected virtual async Task ExecuteAsync(CancellationToken token)
        {
            do
            {
                await ProcessAsync(token);
                await Task.Delay(5000, token);
            }
            while (!token.IsCancellationRequested);
        }

        protected abstract Task ProcessAsync(CancellationToken token);

    }
}