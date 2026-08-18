using System.Collections.Concurrent;
using Altensorcrm.Contract.Services.Tenant;
using Altensorcrm.Domain.Repository;
using Altensorcrm.Persistence.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Altensorcrm.Persistence.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly ICurrentTenantService _tenantService;
    private readonly ConcurrentDictionary<string, object> _repositories = new();
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    public ILeadRepository Leads { get; }
    public IDealRepository Deals { get; }
    public IContactRepository Contacts { get; }
    public IOrganizationRepository Organizations { get; }
    public ITaskRepository Tasks { get; }
    public INoteRepository Notes { get; }
    public ICallLogRepository CallLogs { get; }
    public IEmailTemplateRepository EmailTemplates { get; }
    public IEmailLogRepository EmailLogs { get; }

    public UnitOfWork(
        AppDbContext context,
        ICurrentTenantService tenantService,
        ILeadRepository leads,
        IDealRepository deals,
        IContactRepository contacts,
        IOrganizationRepository organizations,
        ITaskRepository tasks,
        INoteRepository notes,
        ICallLogRepository callLogs,
        IEmailTemplateRepository emailTemplates,
        IEmailLogRepository emailLogs)
    {
        _context = context;
        _tenantService = tenantService;
        Leads = leads;
        Deals = deals;
        Contacts = contacts;
        Organizations = organizations;
        Tasks = tasks;
        Notes = notes;
        CallLogs = callLogs;
        EmailTemplates = emailTemplates;
        EmailLogs = emailLogs;
    }

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var typeName = typeof(TEntity).Name;

        return (IGenericRepository<TEntity>)_repositories.GetOrAdd(typeName, _ => new GenericRepository<TEntity>(_context, _tenantService));
    }


    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            return;
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);

            if (_currentTransaction is not null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction is not null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction is not null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            if (_currentTransaction is not null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
        }

        _disposed = true;
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_currentTransaction is not null)
        {
            await _currentTransaction.DisposeAsync();
        }

        await _context.DisposeAsync();
    }
}
