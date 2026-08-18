using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Altensorcrm.Domain.Entity;

namespace Altensorcrm.Domain.Repository
{
    public interface IEmailLogRepository : IGenericRepository<EmailLog>
    {
        Task<List<EmailLog>> GetByLeadIdAsync(Guid leadId);
        Task<List<EmailLog>> GetByDealIdAsync(Guid dealId);
    }
}
