using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Booth.DockerVolumeBackup.Test.Fixtures.Mocks
{
    public abstract class LoggerMock<T> : ILogger<T>
    {
        public abstract IDisposable? BeginScope<TState>(TState state)
            where TState : notnull;

        public abstract bool IsEnabled(LogLevel logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            this.Log(logLevel, eventId, state?.ToString(), exception);
        }

        public abstract void Log(LogLevel logLevel, EventId eventId, string? state, Exception? exception);
    }
}
