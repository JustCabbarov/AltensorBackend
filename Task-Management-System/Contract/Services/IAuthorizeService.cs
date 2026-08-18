using Domain.Entities;
using System;
using System.Collections.Generic;

namespace Contract.Services
{
    /// <summary>
    /// Login/Register/Logout bu modula aid deyil — Auth Service idarə edir.
    /// Bu servis yalnız lokal AppUser məlumatlarını idarə edir.
    /// </summary>
    public interface IAuthorizeService
    {
        Task<List<AppUser>> GetAllUsersAsync();
        Task<AppUser?> GetUserByIdAsync(Guid userId);
    }
}
