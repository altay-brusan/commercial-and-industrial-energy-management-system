namespace Backend.Application.BatteryStorage.Queries.GetBessCells;

public record BessCellsResponseDto
{
    public List<CellDto> Cells { get; init; } = new();
}

public record CellDto
{
    public string Id { get; init; } = string.Empty;
    public int CellNumber { get; init; }
    public string Status { get; init; } = "normal";
    public double? Voltage { get; init; }
    public double? Current { get; init; }
    public double? Temperature { get; init; }
    public double? Soc { get; init; }
    public double? Soh { get; init; }
    public double? InternalResistance { get; init; }
    public bool? Balancing { get; init; }
    public int? CycleCount { get; init; }
}
