using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using System;
using System.Net;
using System.Net.Http;

namespace CircuitBreakerPattern.API.Configuration
{
    public static class PollyConfig
    {
        public static IServiceCollection AddPollyConfiguration(this IServiceCollection services)
        {

            var retryPolicy = GetRetryPolicy();
            var circuitBreaker = GetCircuitBreakerPolicy();

            var policyWraper = Policy.WrapAsync(retryPolicy, circuitBreaker);

            services.AddHttpClient("PollyTest", c =>
            {
                c.BaseAddress = new Uri("http://localhost:6000");
            })
            .AddPolicyHandler(policyWraper);

            return services;
        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                }, (message, timespan, retryCount, context) =>
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Out.WriteLine($"Content: {message.Result.Content.ReadAsStringAsync().Result}");
                    Console.Out.WriteLine($"ReasonPhrase: {message.Result.ReasonPhrase}");
                    Console.Out.WriteLine($"Tentando pela {retryCount} vez!");
                    Console.ForegroundColor = ConsoleColor.White;
                });
        }

        static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
                .Or<TimeoutRejectedException>()
                // O circuito será cortado se 25% das solicitações falharem em uma janela de 60 segundos, com um mínimo de 3 solicitações na janela de 60 segundos, então o circuito deve ser cortado por 30 segundos.
                //.AdvancedCircuitBreakerAsync(0.25, TimeSpan.FromSeconds(60), 3, TimeSpan.FromSeconds(30), OnBreak, OnReset, OnHalfOpen);
                .CircuitBreakerAsync(3, TimeSpan.FromSeconds(15), OnBreak, OnReset, OnHalfOpen);
        }

        private static void OnHalfOpen()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Out.WriteLine("Circuito em modo de teste, uma solicitação será permitida!");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void OnReset()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Out.WriteLine("Circuito fechado, as solicitações estão sendo recebidas!");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void OnBreak(DelegateResult<HttpResponseMessage> result, TimeSpan ts)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Out.WriteLine("Circuito aberto, as solicitações estão sendo bloqueadas!");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}