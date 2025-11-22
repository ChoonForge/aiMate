using System.ComponentModel.DataAnnotations;

namespace AlphaSurvey.Models;

public class UseCaseOption
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public UseCaseCategory Category { get; set; } = null!;
    public ICollection<SurveyResponseUseCase> SurveyResponses { get; set; } = new List<SurveyResponseUseCase>();
}
