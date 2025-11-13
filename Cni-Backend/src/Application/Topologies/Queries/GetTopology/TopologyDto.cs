using System.Text.Json;
using Backend.Domain.Entities.Topology;

namespace Backend.Application.Topologies.Queries.GetTopology;
public class TopologyDto
{
    private static object? DeserializeJson(string? json)
    {
        return json != null ? JsonSerializer.Deserialize<object>(json) : null;
    }

    public List<TopologyNodeDto> Nodes { get; set; } = new();
    public List<TopologyEdgeDto> Edges { get; set; } = new();
    private class Mapping : Profile
    {
        public Mapping()
        {
            // TopologyNode -> TopologyNodeDto
            CreateMap<TopologyNode, TopologyNodeDto>()
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => DeserializeJson(src.Data)))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => DeserializeJson(src.Position)))
                .ForMember(dest => dest.Style, opt => opt.MapFrom(src => DeserializeJson(src.Style)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

            CreateMap<TopologyEdge, TopologyEdgeDto>()
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => DeserializeJson(src.Data)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.SourceId))
                .ForMember(dest => dest.Target, opt => opt.MapFrom(src => src.TargetId));

        }
    }
}
public class TopologyNodeDto
{
    public string? Id { get; set; }
    public string? Type { get; set; }
    public string? Extent { get; set; }
    public string? ParentId { get; set; }
    public object? Data { get; set; }
    public object? Position { get; set; }
    public object? Style { get; set; }
    public int? TargetHandleCount { get; set; }
    public int? SourceHandleCount { get; set; }
}
public class TopologyEdgeDto
{
    public string? Id { get; set; }
    public string? Source { get; set; }
    public string? Target { get; set; }
    public string? Type { get; set; }
    public object? Data { get; set; }
    public object? Style { get; set; }
    public string? SourceHandle { get; set; }
    public string? TargetHandle { get; set; }
}
