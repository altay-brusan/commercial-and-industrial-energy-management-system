using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Backend.Domain.Entities;
using Backend.Domain.Entities.Topology;
using Backend.Domain.Enums;

namespace Backend.Application.Devices.Queries;

public class DeviceDto
{
    public int Id { get; init; }
    public required string MachineId { get; set; }
    public string? DeviceName { get; set; }
    public int DeviceTypeId { get; set; }
    public bool GrpcConnection { get; set; }
    public int GrpcDeviceStatus { get; set; }
    public DateTime? GrpcLastDataReviceDate { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Device, DeviceDto>();
        }
    }
}
