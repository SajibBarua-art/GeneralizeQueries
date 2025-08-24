using System.ComponentModel.DataAnnotations;

namespace GeneralizeAI.Api.DTOs;

// DTO for the POST request body
public class CreateQueryTemplateDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Id { get; set; } = null!; // The client provides the string ID on creation

    [Required]
    public TemplateDetailsDto Template { get; set; } = null!;
    
    [Required]
    public List<string> RolesAndIdsAllowedToRead { get; set; } = [];

    [Required]
    public string IsMarkedToDelete { get; set; } = "false";
}