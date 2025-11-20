namespace AiMate.Core.Entities;

/// <summary>
/// Organization for multi-tenant access control
/// </summary>
public class Organization
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Name { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// Organization owner (admin user)
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// Is this organization active?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Maximum number of users allowed in this organization
    /// </summary>
    public int MaxUsers { get; set; } = 10;

    /// <summary>
    /// Users in this organization
    /// </summary>
    public List<OrganizationMember> Members { get; set; } = new();

    /// <summary>
    /// Groups within this organization
    /// </summary>
    public List<Group> Groups { get; set; } = new();

    /// <summary>
    /// Organization-level connections (visible to all members)
    /// </summary>
    public List<Connection> Connections { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Membership record linking users to organizations with roles
/// </summary>
public class OrganizationMember
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>
    /// Role within this organization (Owner, Admin, Member, ReadOnly)
    /// </summary>
    public OrganizationRole Role { get; set; } = OrganizationRole.Member;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Roles within an organization
/// </summary>
public enum OrganizationRole
{
    /// <summary>
    /// Read-only access to organization resources
    /// </summary>
    ReadOnly = 0,

    /// <summary>
    /// Standard member - can use connections, cannot manage
    /// </summary>
    Member = 1,

    /// <summary>
    /// Can manage connections, groups, and members
    /// </summary>
    Admin = 2,

    /// <summary>
    /// Full control over organization
    /// </summary>
    Owner = 3
}
