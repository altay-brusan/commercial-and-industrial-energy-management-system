using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Mappings;
using Backend.Application.Common.Models;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
namespace Backend.Application.Devices.Queries.GetDeviceList;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetDevicesWithPaginationQuery : IRequest<PaginatedList<DeviceDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
   
    public string? DeviceTypeList { get; init; }
    public string? Facilities { get; set; }
}

public class GetDevicesWithPaginationQueryHandler : IRequestHandler<GetDevicesWithPaginationQuery, PaginatedList<DeviceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDevicesWithPaginationQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<DeviceDto>> Handle(GetDevicesWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Devices.AsQueryable();

        List<int>? facilityIds = null;
        List<int>? deviceTypes = null;

        if (!string.IsNullOrEmpty(request.Facilities))
        {
            facilityIds = request.Facilities.Split(',').Select(int.Parse).ToList();
        }
        if (facilityIds is { Count: > 0 }) // Null değilse ve içinde eleman varsa filtre uygula
        {
            query = query.Where(x => facilityIds.Contains(x.FacilityId));
        }
        if (!string.IsNullOrEmpty(request.DeviceTypeList))
        {
            deviceTypes = request.DeviceTypeList.Split(',').Select(int.Parse).ToList();
        }
        if (deviceTypes is { Count: > 0 }) // Eğer DeviceTypeList doluysa filtre uygula
        {
            query = query.Where(x => deviceTypes.Contains(x.DeviceTypeId));
        }


        return await query
            .OrderBy(x => x.DeviceName)
            .ProjectTo<DeviceDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}

