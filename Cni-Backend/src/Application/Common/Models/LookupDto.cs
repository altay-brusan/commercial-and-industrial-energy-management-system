using Backend.Domain.Entities;

namespace Backend.Application.Common.Models;

public class LookupDto
{
    public Guid Id { get; init; }

    public string? Title { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            // Add mappings for entities that need lookup functionality
            // Example: CreateMap<SomeEntity, LookupDto>();
        }
    }
}
