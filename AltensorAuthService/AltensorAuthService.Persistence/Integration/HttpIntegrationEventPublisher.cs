using AltensorAuthService.Contract.Events;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace AltensorAuthService.Persistence.Integration
{
    /// <summary>
    /// IIntegrationEventPublisher-in HTTP implementasiyası.
    /// User yaradıldığında tenant-ın aktiv modullarına paralel HTTP POST göndərir.
    /// Hər hansı bir modul əlçatmaz olsa, Auth service-in əməliyyatı fail etmir —
    /// xəta yalnız log-a yazılır.
    /// 
    /// Gələcəkdə RabbitMQ / MassTransit istifadə etmək üçün bu sinfi
    /// RabbitMqIntegrationEventPublisher ilə əvəz etmək kifayətdir.
    /// </summary>
    public class HttpIntegrationEventPublisher : IIntegrationEventPublisher
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ModuleEndpointRegistry _endpointRegistry;
        private readonly ILogger<HttpIntegrationEventPublisher> _logger;

        public HttpIntegrationEventPublisher(
            IHttpClientFactory httpClientFactory,
            ModuleEndpointRegistry endpointRegistry,
            ILogger<HttpIntegrationEventPublisher> logger)
        {
            _httpClientFactory = httpClientFactory;
            _endpointRegistry = endpointRegistry;
            _logger = logger;
        }

        public async Task PublishUserCreatedAsync(UserCreatedIntegrationEvent @event)
        {
            _logger.LogInformation(
                "UserCreated event publish edilir: UserId={UserId}, TenantId={TenantId}, Email='{Email}'",
                @event.UserId, @event.TenantId, @event.Email);

            var endpoints = await _endpointRegistry.GetUserCreatedEndpointsAsync(@event.TenantId);

            if (endpoints.Count == 0)
            {
                _logger.LogDebug("Tenant {TenantId} üçün aktiv modul endpoint-i tapılmadı. Event göndərilmir.", @event.TenantId);
                return;
            }

            // Bütün modullara paralel göndər
            var tasks = endpoints.Select(endpoint => SendToModuleAsync(endpoint, @event));
            await Task.WhenAll(tasks);

            _logger.LogInformation(
                "UserCreated event {Count} modul endpoint-inə göndərildi: UserId={UserId}",
                endpoints.Count, @event.UserId);
        }

        private async Task SendToModuleAsync(string endpoint, UserCreatedIntegrationEvent @event)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ModuleInternal");

                _logger.LogDebug("Modul sync sorğusu göndərilir: Endpoint={Endpoint}, UserId={UserId}", endpoint, @event.UserId);

                var response = await client.PostAsJsonAsync(endpoint, @event);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Modul sync uğurlu: Endpoint={Endpoint}, UserId={UserId}", endpoint, @event.UserId);
                }
                else
                {
                    _logger.LogWarning(
                        "Modul sync uğursuz (HTTP {StatusCode}): Endpoint={Endpoint}, UserId={UserId}",
                        (int)response.StatusCode, endpoint, @event.UserId);
                }
            }
            catch (HttpRequestException ex)
            {
                // Modul servisi əlçatmaz olsa, Auth service fail etmir
                _logger.LogError(ex,
                    "Modul sync xətası (servis əlçatmaz ola bilər): Endpoint={Endpoint}, UserId={UserId}",
                    endpoint, @event.UserId);
            }
            catch (TaskCanceledException ex)
            {
                // Timeout
                _logger.LogError(ex,
                    "Modul sync timeout: Endpoint={Endpoint}, UserId={UserId}",
                    endpoint, @event.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Modul sync gözlənilməz xətası: Endpoint={Endpoint}, UserId={UserId}",
                    endpoint, @event.UserId);
            }
        }
    }
}
