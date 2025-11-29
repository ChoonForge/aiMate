public class UpdateFeedbackRequest
{
    public Guid UserId { get; set; } // Add this property
    public int? Rating { get; set; }
    public string? TextFeedback { get; set; }
    public List<FeedbackTagDto>? Tags { get; set; }
    public string? ModelId { get; set; }
    public long? ResponseTimeMs { get; set; }
}