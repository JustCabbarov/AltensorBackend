using AltensorAuthService.Domain.Entities;
using AltensorAuthService.Domain.Enums;
using AltensorAuthService.Persistence.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AltensorAuthService.Persistence.Data.Seed
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

            try
            {
                logger.LogInformation("Verilənlər bazası seed prosesi başlayır...");

                // 1. Seed System Modules
                var modules = new List<SistemModule>
                {
                    new SistemModule
                    {
                        Code = "crm",
                        Name = "CRM Modulu",
                        Description = "Müştəri münasibətlərinin və satışların idarə edilməsi",
                        IsActive = true
                    },
                    new SistemModule
                    {
                        Code = "inventory",
                        Name = "Anbar (Inventory) Modulu",
                        Description = "Məhsul, anbar qalıqları və transferlərin idarə edilməsi",
                        IsActive = true
                    },
                    new SistemModule
                    {
                        Code = "hr",
                        Name = "İnsan Resursları (HR) Modulu",
                        Description = "İşçilər, davamiyyət və məzuniyyətlərin idarə edilməsi",
                        IsActive = true
                    },
                    new SistemModule
                    {
                        Code = "accounting",
                        Name = "Mühasibatlıq (Accounting) Modulu",
                        Description = "Maliyyə əməliyyatları, hesab-fakturalar və hesabatlar",
                        IsActive = true
                    },
                    new SistemModule
                    {
                        Code = "tms",
                        Name = "Task Management (TMS) Modulu",
                        Description = "Tapşırıqların, iş qruplarının və icraçıların idarə edilməsi",
                        IsActive = true
                    }
                };

                foreach (var module in modules)
                {
                    var existingModule = await context.Modules.FirstOrDefaultAsync(m => m.Code == module.Code);
                    if (existingModule == null)
                    {
                        await context.Modules.AddAsync(module);
                    }
                }
                await context.SaveChangesAsync();

                // Reload modules to get database IDs
                var crmModule = await context.Modules.FirstAsync(m => m.Code == "crm");
                var invModule = await context.Modules.FirstAsync(m => m.Code == "inventory");
                var hrModule = await context.Modules.FirstAsync(m => m.Code == "hr");
                var accModule = await context.Modules.FirstAsync(m => m.Code == "accounting");
                var tmsModule = await context.Modules.FirstAsync(m => m.Code == "tms");

                // 2. Seed Permissions
                var permissions = new List<Permission>
                {
                    // CRM
                    new Permission { Code = "crm.read", Name = "CRM Baxış", Description = "CRM məlumatlarını oxumaq", ModuleId = crmModule.Id },
                    new Permission { Code = "crm.write", Name = "CRM Redaktə", Description = "CRM məlumatları yaratmaq/dəyişmək", ModuleId = crmModule.Id },
                    new Permission { Code = "crm.delete", Name = "CRM Silmə", Description = "CRM qeydlərini silmək", ModuleId = crmModule.Id },
                    new Permission { Code = "crm.approve_deal", Name = "CRM Sövdələşmə Təsdiqi", Description = "Satış sövdələşmələrini təsdiq etmək", ModuleId = crmModule.Id },

                    // Inventory
                    new Permission { Code = "inventory.read", Name = "Anbar Baxış", Description = "Anbar və məhsul qalıqlarını oxumaq", ModuleId = invModule.Id },
                    new Permission { Code = "inventory.write", Name = "Anbar Redaktə", Description = "Məhsul və anbar əməliyyatları yaratmaq", ModuleId = invModule.Id },
                    new Permission { Code = "inventory.delete", Name = "Anbar Silmə", Description = "Anbar qeydlərini silmək", ModuleId = invModule.Id },
                    new Permission { Code = "inventory.approve_transfer", Name = "Transfer Təsdiqi", Description = "Anbarlararası transferləri təsdiq etmək", ModuleId = invModule.Id },

                    // HR
                    new Permission { Code = "hr.read", Name = "HR Baxış", Description = "İşçi məlumatlarına baxış", ModuleId = hrModule.Id },
                    new Permission { Code = "hr.write", Name = "HR Redaktə", Description = "İşçi məlumatlarını redaktə etmək", ModuleId = hrModule.Id },
                    new Permission { Code = "hr.delete", Name = "HR Silmə", Description = "İşçi qeydlərini silmək", ModuleId = hrModule.Id },
                    new Permission { Code = "hr.approve_leave", Name = "Məzuniyyət Təsdiqi", Description = "Məzuniyyət müraciətlərini təsdiq etmək", ModuleId = hrModule.Id },

                    // Accounting
                    new Permission { Code = "accounting.read", Name = "Mühasibatlıq Baxış", Description = "Maliyyə qeydlərinə baxış", ModuleId = accModule.Id },
                    new Permission { Code = "accounting.write", Name = "Mühasibatlıq Redaktə", Description = "Maliyyə qeydlərini redaktə etmək", ModuleId = accModule.Id },
                    new Permission { Code = "accounting.delete", Name = "Mühasibatlıq Silmə", Description = "Maliyyə qeydlərini silmək", ModuleId = accModule.Id },
                    new Permission { Code = "accounting.approve_payment", Name = "Ödəniş Təsdiqi", Description = "Ödənişləri təsdiq etmək", ModuleId = accModule.Id },

                    // TMS (Task Management)
                    new Permission { Code = "tms.tasks.view", Name = "Tapşırıqlara Baxış", Description = "Tapşırıqları oxumaq və görüntüləmək", ModuleId = tmsModule.Id },
                    new Permission { Code = "tms.tasks.create", Name = "Tapşırıq Yaratma", Description = "Yeni tapşırıq yaratmaq", ModuleId = tmsModule.Id },
                    new Permission { Code = "tms.tasks.update", Name = "Tapşırıq Redaktə", Description = "Tapşırıqları yeniləmək", ModuleId = tmsModule.Id },
                    new Permission { Code = "tms.tasks.delete", Name = "Tapşırıq Silmə", Description = "Tapşırıqları silmək", ModuleId = tmsModule.Id },
                    new Permission { Code = "tms.workgroups.view", Name = "İş Qruplarına Baxış", Description = "İş qruplarını oxumaq", ModuleId = tmsModule.Id },
                    new Permission { Code = "tms.workgroups.manage", Name = "İş Qruplarını İdarəetmə", Description = "İş qruplarını yaratmaq və idarə etmək", ModuleId = tmsModule.Id },
                    new Permission { Code = "tms.performance.view", Name = "Performansa Baxış", Description = "Performans statistikasına baxış", ModuleId = tmsModule.Id }
                };

                foreach (var perm in permissions)
                {
                    var existing = await context.Permissions.FirstOrDefaultAsync(p => p.Code == perm.Code);
                    if (existing == null)
                    {
                        await context.Permissions.AddAsync(perm);
                    }
                }
                await context.SaveChangesAsync();

                // 3. Seed System Roles
                var allPermissions = await context.Permissions.ToListAsync();

                // PlatformSuperAdmin
                const string superAdminRoleName = "PlatformSuperAdmin";
                var superAdminRole = await roleManager.FindByNameAsync(superAdminRoleName);
                if (superAdminRole == null)
                {
                    superAdminRole = new ApplicationRole
                    {
                        Name = superAdminRoleName,
                        NormalizedName = superAdminRoleName.ToUpper(),
                        IsSystemRole = true,
                        Description = "Platforma Super Administratoru — tam qlobal idarəetmə hüququ.",
                        TenantId = null
                    };
                    await roleManager.CreateAsync(superAdminRole);

                    // Assign all permissions
                    foreach (var p in allPermissions)
                    {
                        await context.RolePermissions.AddAsync(new RolePermission
                        {
                            RoleId = superAdminRole.Id,
                            PermissionId = p.Id
                        });
                    }
                    await context.SaveChangesAsync();
                }

                // TenantAdmin
                const string tenantAdminRoleName = "TenantAdmin";
                var tenantAdminRole = await roleManager.FindByNameAsync(tenantAdminRoleName);
                if (tenantAdminRole == null)
                {
                    tenantAdminRole = new ApplicationRole
                    {
                        Name = tenantAdminRoleName,
                        NormalizedName = tenantAdminRoleName.ToUpper(),
                        IsSystemRole = true,
                        Description = "Tenant Administratoru — öz müştərisi daxilində tam idarəetmə hüququ.",
                        TenantId = null
                    };
                    await roleManager.CreateAsync(tenantAdminRole);

                    // Assign all permissions to default TenantAdmin
                    foreach (var p in allPermissions)
                    {
                        await context.RolePermissions.AddAsync(new RolePermission
                        {
                            RoleId = tenantAdminRole.Id,
                            PermissionId = p.Id
                        });
                    }
                    await context.SaveChangesAsync();
                }

                // 4. Seed Platform System Tenant & Platform Super Admin User
                var platformTenantSlug = "platform";
                var platformTenant = await context.Tenants.FirstOrDefaultAsync(t => t.Slug == platformTenantSlug);
                if (platformTenant == null)
                {
                    platformTenant = new Tenant
                    {
                        Name = "Altensor Platform HQ",
                        Slug = platformTenantSlug,
                        Domain = "platform.altensor.io",
                        Status = TenantStatus.Active
                    };
                    await context.Tenants.AddAsync(platformTenant);
                    await context.SaveChangesAsync();

                    // Subscribe to all modules
                    foreach (var m in modules)
                    {
                        var dbMod = await context.Modules.FirstAsync(x => x.Code == m.Code);
                        await context.TenantModuleSubscriptions.AddAsync(new TenantModuleSubscription
                        {
                            TenantId = platformTenant.Id,
                            ModuleId = dbMod.Id,
                            Status = SubscriptionStatus.Active,
                            StartsAt = DateTime.UtcNow
                        });
                    }
                    await context.SaveChangesAsync();
                }

                // Platform Super Admin User
                var superAdminEmail = configuration["Platform:SuperAdminEmail"] ?? "superadmin@altensor.io";
                var superAdminPassword = configuration["Platform:SuperAdminPassword"] ?? "SuperAdmin@2026!";

                var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);
                if (superAdminUser == null)
                {
                    superAdminUser = new ApplicationUser
                    {
                        UserName = superAdminEmail,
                        Email = superAdminEmail,
                        FullName = "Platform Super Admin",
                        TenantId = platformTenant.Id,
                        IsActive = true
                    };

                    var result = await userManager.CreateAsync(superAdminUser, superAdminPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(superAdminUser, superAdminRoleName);
                        logger.LogInformation("Platform Super Admin istifadəçisi uğurla yaradıldı: {Email}", superAdminEmail);
                    }
                    else
                    {
                        logger.LogError("Super Admin yaradılarkən xəta: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }

                logger.LogInformation("Verilənlər bazası seed prosesi uğurla tamamlandı.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Seed prosesi zamanı xəta baş verdi: {Message}", ex.Message);
            }
        }
    }
}
