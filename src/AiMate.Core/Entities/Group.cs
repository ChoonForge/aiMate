using System.ComponentModel.DataAnnotations;

namespace AiMate.Core.Entities;

/// <summary>
/// Group for team-based access control within an organization
/// </summary>
public class Group
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Organization this group belongs to (null for global groups)
    /// </summary>
    public Guid? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    /// <summary>
    /// Group owner/creator
    /// </summary>
    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    /// <summary>
    /// Is this group active?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Users in this group
    /// </summary>
    public List<GroupMember> Members { get; set; } = new();

    /// <summary>
    /// Connections shared with this group
    /// </summary>
    public List<Connection> Connections { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Membership record linking users to groups
/// </summary>
public class GroupMember
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>
    /// Role within this group (Owner, Admin, Member)
    /// </summary>
    public GroupRole Role { get; set; } = GroupRole.Member;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Roles within a group
/// </summary>
public enum GroupRole
{
    /// <summary>
    /// Standard member - can use group resources
    /// </summary>
    Member = 0,

    /// <summary>
    /// Can manage group members and resources
    /// </summary>
    Admin = 1,

    /// <summary>
    /// Full control over group
    /// </summary>
    Owner = 2
}
