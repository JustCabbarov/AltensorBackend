using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AltensorAuthService.Domain.Entities
{
    public static class AppClaimTypes
    {
        public const string TenantId = "tenant_id";
        public const string TenantStatus = "tenant_status";
        public const string Module = "module";       // çoxlu ola bilər: bir neçə "module" claim-i
        public const string Permission = "permission"; // çoxlu ola bilər: bir neçə "permission" claim-i
    }
}
