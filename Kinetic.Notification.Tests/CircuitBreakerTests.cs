
using Polly;
using Polly.CircuitBreaker;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Kinetic.Notification.Tests
{
    public class CircuitBreakerTests
    {
        [Fact]
        public async Task CircuitBreaker_OpenCircuitAfterTwoFailures()
        {
            var breaker = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(2, TimeSpan.FromSeconds(10));

            await Assert.ThrowsAsync<Exception>(() => breaker.ExecuteAsync(() => Task.FromException(new Exception("Primer intento"))));

            await Assert.ThrowsAsync<Exception>(() => breaker.ExecuteAsync(() => Task.FromException(new Exception("Segundo intento"))));

            // Ahora el circuito tendría que estar abierto
            await Assert.ThrowsAsync<BrokenCircuitException>(() => breaker.ExecuteAsync(() => Task.CompletedTask));
        }
    }
}
