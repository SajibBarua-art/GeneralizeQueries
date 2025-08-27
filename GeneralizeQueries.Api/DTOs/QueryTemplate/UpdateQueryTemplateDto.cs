using System.ComponentModel.DataAnnotations;

namespace GeneralizeQueries.Api.DTOs;

// DTO for the PUT request body. Note the absence of the Id field.
public class UpdateQueryTemplateDto
{
    [Required]
    public TemplateDetailsDto Template { get; set; } = null!;
    
    [Required]
    public List<string> RolesAndIdsAllowedToRead { get; set; } = [];

    [Required]
    public string IsMarkedToDelete { get; set; } = null!;
}