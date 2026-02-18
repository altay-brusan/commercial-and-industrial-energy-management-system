using Backend.Domain.Entities;

namespace Backend.Application.Devices.Queries;

public class DeviceDto
{
    public Guid Id { get; init; }
    public Guid FacilityId { get; set; }
    public Guid DeviceTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string Ports { get; set; } = "[]";

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Device, DeviceDto>();
        }
    }
}
