using System;
using Altensorcrm.Contract.Services.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Altensorcrm.Persistence.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=CRM;Username=postgres;Password=Reporting123@");

        return new AppDbContext(optionsBuilder.Options, new DesignTimeTenantService());
    }

    private class DesignTimeTenantService : ICurrentTenantService
    {
        public Guid? TenantId => null;
        public Guid? UserId => null;
        public string? TenantStatus => "Active";
        public bool IsAuthenticated => false;
        public bool IsPlatformSuperAdmin => false;
        public bool IsTenantAdmin => false;
    }
}
