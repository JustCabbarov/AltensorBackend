using Altensorcrm.Domain.Common;

namespace Altensorcrm.Domain.Entity;

public class User : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    public ICollection<Lead> AssignedLeads { get; set; } = new List<Lead>();
    public ICollection<Deal> AssignedDeals { get; set; } = new List<Deal>();
    public ICollection<Contact> AssignedContacts { get; set; } = new List<Contact>();
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    public ICollection<Note> CreatedNotes { get; set; } = new List<Note>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
