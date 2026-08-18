using AltensorAuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace AltensorAuthService.Persistence.Data
{

    public class AppDbContext
        : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {

        public AppDbContext(
            DbContextOptions<AppDbContext> options) : base(options)
        {
           
        }

        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<SistemModule> Modules => Set<SistemModule>();
        public DbSet<TenantModuleSubscription> TenantModuleSubscriptions => Set<TenantModuleSubscription>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<PasswordResetOtp> PasswordResetOtps => Set<PasswordResetOtp>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ---------- Tenant ----------
            builder.Entity<Tenant>(entity =>
            {
                entity.HasIndex(t => t.Slug).IsUnique();
            });

            // ---------- ApplicationUser ----------
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasOne(u => u.Tenant)
                      .WithMany(t => t.Users)
                      .HasForeignKey(u => u.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Eyni email fərqli tenant-larda təkrarlana bilsin deyə,
                // Identity-nin default qlobal email unique index-ini
                // (TenantId, NormalizedEmail) composite unique index ilə əvəz edirik.
                entity.HasIndex(u => new { u.TenantId, u.NormalizedEmail })
                      .IsUnique()
                      .HasDatabaseName("IX_User_Tenant_Email");

                entity.HasIndex(u => new { u.TenantId, u.NormalizedUserName })
                      .IsUnique()
                      .HasDatabaseName("IX_User_Tenant_UserName");

            });

            // ---------- ApplicationRole ----------
            builder.Entity<ApplicationRole>(entity =>
            {
                entity.HasOne(r => r.Tenant)
                      .WithMany(t => t.Roles)
                      .HasForeignKey(r => r.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);

               
            });

            // ---------- Module (qlobal, tenant filtri yoxdur) ----------
            builder.Entity<SistemModule>(entity =>
            {
                entity.HasIndex(m => m.Code).IsUnique();
            });

            // ---------- TenantModuleSubscription ----------
            builder.Entity<TenantModuleSubscription>(entity =>
            {
                entity.HasOne(s => s.Tenant)
                      .WithMany(t => t.ModuleSubscriptions)
                      .HasForeignKey(s => s.TenantId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Module)
                      .WithMany(m => m.Subscriptions)
                      .HasForeignKey(s => s.ModuleId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(s => new { s.TenantId, s.ModuleId }).IsUnique();
            });

            // ---------- Permission (qlobal, tenant filtri yoxdur) ----------
            builder.Entity<Permission>(entity =>
            {
                entity.HasOne(p => p.Module)
                      .WithMany(m => m.Permissions)
                      .HasForeignKey(p => p.ModuleId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(p => p.Code).IsUnique();
            });

            // ---------- RolePermission (composite key) ----------
            builder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });

                entity.HasOne(rp => rp.Role)
                      .WithMany(r => r.RolePermissions)
                      .HasForeignKey(rp => rp.RoleId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rp => rp.Permission)
                      .WithMany(p => p.RolePermissions)
                      .HasForeignKey(rp => rp.PermissionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------- RefreshToken ----------
            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasOne(rt => rt.User)
                      .WithMany()
                      .HasForeignKey(rt => rt.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(rt => rt.TokenHash).IsUnique();

                // RefreshToken bilavasitə TenantId saxlamır, amma User üzərindən
                // əlaqəli olduğu üçün tenant izolasiyası User filtrindən keçərək təmin olunur.
            });

            // ---------- PasswordResetOtp ----------
            builder.Entity<PasswordResetOtp>(entity =>
            {
                entity.HasOne(o => o.User)
                      .WithMany()
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // OTP sorğusu üçün sürətli axtarış indeksi
                entity.HasIndex(o => new { o.UserId, o.Code, o.IsUsed })
                      .HasDatabaseName("IX_PasswordResetOtp_UserId_Code_IsUsed");
            });
        }
    }
}
