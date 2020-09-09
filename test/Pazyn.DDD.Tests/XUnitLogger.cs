using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Pazyn.DDD.Tests
{
    public class XUnitLogger
    {
        private class DummyDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }

        private class CustomLogger : ILogger
        {
            private readonly ITestOutputHelper output;

            public CustomLogger(ITestOutputHelper output)
            {
                this.output = output;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, String> formatter)
            {
                output.WriteLine($"{logLevel} {eventId} {state} {exception?.Message}");
            }

            public Boolean IsEnabled(LogLevel logLevel) => true;

            public IDisposable BeginScope<TState>(TState state)
            {
                return new DummyDisposable();
            }
        }

        private class CustomLoggerProvider : ILoggerProvider
        {
            private readonly ITestOutputHelper output;

            public CustomLoggerProvider(ITestOutputHelper output)
            {
                this.output = output;
            }

            public void Dispose()
            {
            }

            public ILogger CreateLogger(String categoryName) => new CustomLogger(output);
        }

        private ITestOutputHelper Output { get; }

        public XUnitLogger(ITestOutputHelper output)
        {
            Output = output;
        }

        public ILoggerFactory ToLoggerFactory()
        {
            return new LoggerFactory(
                new[] { new CustomLoggerProvider(Output) }
            );
        }
    }
}