namespace AlphaSurvey.Models;

public class SurveyResponseUseCase
{
    public int SurveyResponseId { get; set; }
    public int UseCaseOptionId { get; set; }

    // Navigation properties
    public SurveyResponse SurveyResponse { get; set; } = null!;
    public UseCaseOption UseCaseOption { get; set; } = null!;
}
