using Backend.Domain.Entities;

namespace Backend.Application.Users.Queries.GetUsersWithPagination;

public class UserBriefDto
{
    public string Id { get; set; } = default!;
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ApplicationUser, UserBriefDto>();
        }
    }
}
