namespace Backend.Domain.Enums;

public enum AlmJobStatus
{
    Queued = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Timeout = 4
}
