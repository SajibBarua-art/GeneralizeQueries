using GeneralizeAI.Api.DTOs;
using GeneralizeAI.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using GeneralizeAI.Core.Entities;

namespace GeneralizeAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueryTemplatesController : ControllerBase
{
    private readonly IQueryTemplateService _templateService;

    public QueryTemplatesController(IQueryTemplateService templateService)
    {
        _templateService = templateService;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<QueryTemplateDto>>> GetAll()
    {
        var templates = await _templateService.GetAllTemplatesAsync();
        
        // Use LINQ's Select to map each entity to its DTO using our helper method
        var dtos = templates.Select(MapToDto);
        
        return Ok(dtos);
    }

    [HttpGet("{id}", Name = "GetTemplateById")]
    public async Task<ActionResult<QueryTemplateDto>> GetById(string id)
    {
        var template = await _templateService.GetTemplateByIdAsync(id);

        if (template is null)
        {
            return NotFound();
        }

        // Manually map from the entity to the DTO
        var dto = new QueryTemplateDto
        {
            Id = template.Id,
            RolesAndIdsAllowedToRead = template.RolesAndIdsAllowedToRead,
            IsMarkedToDelete = template.IsMarkedToDelete,
            Template = new TemplateDetailsDto
            {
                Source = template.Template.Source,
                DynamicFilter = template.Template.DynamicFilter,
                Fields = template.Template.Fields,
                AllowedSorts = template.Template.AllowedSorts,
                Locale = template.Template.Locale,
                IsDynamicFilter = template.Template.IsDynamicFilter,
                CountOnly = template.Template.CountOnly
            }
        };

        return Ok(dto);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(CreateQueryTemplateDto createDto)
    {
        var existingTemplate = await _templateService.GetTemplateByIdAsync(createDto.Id);
        if (existingTemplate != null)
        {
            return Conflict($"A template with ID '{createDto.Id}' already exists.");
        }

        var template = MapFromCreateDtoToEntity(createDto);

        await _templateService.CreateTemplateAsync(template);

        var responseDto = MapToDto(template);
        return CreatedAtRoute("GetTemplateById", new { id = responseDto.Id }, responseDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateQueryTemplateDto updateDto)
    {
        var existingTemplate = await _templateService.GetTemplateByIdAsync(id);
        if (existingTemplate is null)
        {
            return NotFound();
        }

        var templateToUpdate = MapFromUpdateDtoToEntity(id, updateDto);

        var success = await _templateService.UpdateTemplateAsync(id, templateToUpdate);
        
        return success ? NoContent() : StatusCode(500, "An error occurred during the update.");
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var existingTemplate = await _templateService.GetTemplateByIdAsync(id);
        if (existingTemplate is null)
        {
            return NotFound();
        }
        
        var success = await _templateService.DeleteTemplateAsync(id);
        
        return success ? NoContent() : StatusCode(500, "An error occurred during deletion.");
    }


    // --- v-- MAPPING HELPERS --v ---

    private QueryTemplate MapFromCreateDtoToEntity(CreateQueryTemplateDto dto)
    {
        return new QueryTemplate
        {
            Id = dto.Id,
            RolesAndIdsAllowedToRead = dto.RolesAndIdsAllowedToRead,
            IsMarkedToDelete = dto.IsMarkedToDelete,
            Template = new TemplateDetails
            {
                Source = dto.Template.Source,
                DynamicFilter = dto.Template.DynamicFilter,
                Fields = dto.Template.Fields,
                AllowedSorts = dto.Template.AllowedSorts,
                Locale = dto.Template.Locale,
                IsDynamicFilter = dto.Template.IsDynamicFilter,
                CountOnly = dto.Template.CountOnly
            }
        };
    }

    private QueryTemplate MapFromUpdateDtoToEntity(string id, UpdateQueryTemplateDto dto)
    {
        // Similar to the create mapping, but we get the ID from the URL parameter.
        return new QueryTemplate
        {
            Id = id, 
            RolesAndIdsAllowedToRead = dto.RolesAndIdsAllowedToRead,
            IsMarkedToDelete = dto.IsMarkedToDelete,
            Template = new TemplateDetails
            {
                Source = dto.Template.Source,
                DynamicFilter = dto.Template.DynamicFilter,
                Fields = dto.Template.Fields,
                AllowedSorts = dto.Template.AllowedSorts,
                Locale = dto.Template.Locale,
                IsDynamicFilter = dto.Template.IsDynamicFilter,
                CountOnly = dto.Template.CountOnly
            }
        };
    }
    
    private QueryTemplateDto MapToDto(QueryTemplate template)
    {
        return new QueryTemplateDto
        {
            Id = template.Id,
            RolesAndIdsAllowedToRead = template.RolesAndIdsAllowedToRead,
            IsMarkedToDelete = template.IsMarkedToDelete,
            Template = new TemplateDetailsDto
            {
                Source = template.Template.Source,
                DynamicFilter = template.Template.DynamicFilter,
                Fields = template.Template.Fields,
                AllowedSorts = template.Template.AllowedSorts,
                Locale = template.Template.Locale,
                IsDynamicFilter = template.Template.IsDynamicFilter,
                CountOnly = template.Template.CountOnly
            }
        };
    }
}