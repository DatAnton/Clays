using Hangfire.States;

namespace Clays.Services
{
    public sealed class HangfireExceptionLoggingFilter(IServiceProvider services) : IElectStateFilter
    {
        public void OnStateElection(ElectStateContext context)
        {
            if (context.CandidateState is FailedState { Exception: not null } failedState)
            {
                ExceptionLogWriter.TryWriteAsync(
                    services,
                    failedState.Exception,
                    "Hangfire background job").GetAwaiter().GetResult();
            }
        }
    }
}