using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Hubs
{


    using Microsoft.AspNetCore.SignalR;

    public class NameUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            var user = connection.User;
            if (user == null) return null;

            // 1. Ən vacibi: Sizin token-dəki "nameid" sahəsi
            var userId = user.FindFirst("nameid")?.Value;

            // 2. Əgər .NET onu avtomatik ClaimTypes.NameIdentifier-ə çeviribsə (Uzun URL)
            if (string.IsNullOrEmpty(userId))
            {
                userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }

            // 3. Əgər standart "sub" (Subject) sahəsindədirsə
            if (string.IsNullOrEmpty(userId))
            {
                userId = user.FindFirst("sub")?.Value;
            }

            // 4. Debug üçün: Konsola yazdıraq ki, ID tapıldı yoxsa yox
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("❌ NameUserIdProvider: ID tapılmadı! Claims: " + string.Join(", ", user.Claims.Select(c => c.Type)));
            }
            else
            {
                Console.WriteLine($"✅ NameUserIdProvider: ID tapıldı -> {userId}");
            }

            return userId;
        }
    }

}
