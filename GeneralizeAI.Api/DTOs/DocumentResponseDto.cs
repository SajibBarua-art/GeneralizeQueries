namespace GeneralizeAI.Api.DTOs;

public class DocumentResponseDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    
    // The client expects a regular JSON object, so we use a Dictionary
    public Dictionary<string, object> Data { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
}