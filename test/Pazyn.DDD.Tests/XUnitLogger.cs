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
            private readonly ITestOutputHelper _output;

            public CustomLogger(ITestOutputHelper output)
            {
                _output = output;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                _output.WriteLine($"{logLevel} {eventId} {state} {exception?.Message}");
            }

            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable BeginScope<TState>(TState state)
            {
                return new DummyDisposable();
            }
        }

        private class CustomLoggerProvider : ILoggerProvider
        {
            private readonly ITestOutputHelper _output;

            public CustomLoggerProvider(ITestOutputHelper output)
            {
                _output = output;
            }

            public void Dispose()
            {
            }

            public ILogger CreateLogger(string categoryName) => new CustomLogger(_output);
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