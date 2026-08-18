namespace Altensorcrm.Domain.Repository;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    ILeadRepository Leads { get; }
    IDealRepository Deals { get; }
    IContactRepository Contacts { get; }
    IOrganizationRepository Organizations { get; }
    ITaskRepository Tasks { get; }
    INoteRepository Notes { get; }
    ICallLogRepository CallLogs { get; }
    IEmailTemplateRepository EmailTemplates { get; }
    IEmailLogRepository EmailLogs { get; }

    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
