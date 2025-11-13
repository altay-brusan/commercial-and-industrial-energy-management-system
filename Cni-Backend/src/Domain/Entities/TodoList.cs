namespace Backend.Domain.Entities;

public class TodoList : BaseAuditableEntity , IMultiTenantEntity
{
    public string? Title { get; set; }

    public Colour Colour { get; set; } = Colour.White;

    public IList<TodoItem> Items { get; private set; } = new List<TodoItem>();
    public int TenantId { get; set; }
}
