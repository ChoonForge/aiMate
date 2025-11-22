using System.ComponentModel.DataAnnotations;

namespace AlphaSurvey.Models;

public class UseCaseCategory
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation property
    public ICollection<UseCaseOption> Options { get; set; } = new List<UseCaseOption>();
}
