namespace Backend.Application.Graphs.Queries.GetInverterGraph;

/// <summary>
/// DTO for inverter/PCS graph data points.
/// Data is extracted from DevicePcsData JSON payload.
/// </summary>
public class InverterGraphDto
{
    public DateTimeOffset Time { get; set; }
    public int PcsId { get; set; }
    public double? ActivePower { get; set; }
    public double? ReactivePower { get; set; }
    public double? PowerFactor { get; set; }
    public double? AcVoltage { get; set; }
    public double? AcCurrent { get; set; }
    public double? DcVoltage { get; set; }
    public double? DcCurrent { get; set; }
    public double? DcPower { get; set; }
    public double? Temperature { get; set; }
}
