using AltensorAuthService.Contract.Events;

namespace AltensorAuthService.Contract.Events
{
    /// <summary>
    /// Digər modullara integration event-lər göndərmək üçün abstraksiya.
    /// Hazırda HTTP, gələcəkdə RabbitMQ/MassTransit ilə əvəz edilə bilər.
    /// </summary>
    public interface IIntegrationEventPublisher
    {
        /// <summary>
        /// Yeni user yaradıldığında bütün aktiv modullara xəbər verir.
        /// </summary>
        Task PublishUserCreatedAsync(UserCreatedIntegrationEvent @event);
    }
}
