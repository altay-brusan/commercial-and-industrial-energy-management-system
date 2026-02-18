namespace Backend.Application.Graphs.Queries.GetPowerGeneralGraph;

/// <summary>
/// DTO for general power graph data points.
/// Aggregates power data from multiple device types.
/// </summary>
public class PowerGeneralGraphDto
{
    public Guid FacilityId { get; set; }
    public DateTime Time { get; set; }
    public double? GeneratorPower { get; set; }
    public double? GridPower { get; set; }
    public double? BessPower { get; set; }
    public double? SolarPower { get; set; }
}
