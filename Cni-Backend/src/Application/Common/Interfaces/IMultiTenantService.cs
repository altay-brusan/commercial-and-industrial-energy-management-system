using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Application.Common.Interfaces;

public interface IMultiTenantService
{
    int? GetCurrentTenantId();
    int? SetCurrentTenantId(int? tenantId);
    bool SetSuperAdmin(bool isAdmin);
    bool IsSuperAdmin();
}
