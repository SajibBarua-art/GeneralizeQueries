// GeneralizeAI.Api/DTOs/CreateDocumentDto.cs
using System.ComponentModel.DataAnnotations;

namespace GeneralizeAI.Api.DTOs;

public class CreateDocumentDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    // Allows the client to send any valid JSON object
    public Dictionary<string, object> Data { get; set; } = new();
}