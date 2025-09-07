using GeneralizeQueries.Api.DTOs;
using GeneralizeQueries.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using GeneralizeQueries.Core.Entities;

namespace GeneralizeQueries.Api.Controllers;

[ApiController]
[Route("api/service/{serviceId}/templates")]
public class QueryTemplatesController : ControllerBase
{
    private readonly IQueryTemplateService _templateService;
    private readonly IQueryTemplateRepositoryFactory _repositoryFactory;

    public QueryTemplatesController(IQueryTemplateService templateService, IQueryTemplateRepositoryFactory repositoryFactory)
    {
        _templateService = templateService;
        _repositoryFactory = repositoryFactory;
    }

    // A private helper to reduce code duplication in every action method.
    private async Task<IQueryTemplateRepository?> GetRepositoryFromFactory(string serviceId)
    {
        var repo = await _repositoryFactory.CreateRepositoryAsync(serviceId);
        return repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(string serviceId)
    {
        var repo = await GetRepositoryFromFactory(serviceId);
        if (repo == null)
        {
            return NotFound(new { message = $"Service with ID '{serviceId}' not found in configuration." });
        }

        var templates = await _templateService.GetAllTemplatesAsync(repo);
        return Ok(templates.Select(MapToDto));
    }

    [HttpGet("{id}", Name = "GetTemplateById")]
    public async Task<IActionResult> GetById(string serviceId, string id)
    {
        var repo = await GetRepositoryFromFactory(serviceId);
        if (repo == null)
        {
            return NotFound(new { message = $"Service with ID '{serviceId}' not found in configuration." });
        }

        var template = await _templateService.GetTemplateByIdAsync(repo, id);
        if (template == null)
        {
            return NotFound(new { message = $"Template with ID '{id}' not found in service '{serviceId}'." });
        }
        return Ok(MapToDto(template));
    }

    [HttpPost]
    public async Task<IActionResult> Create(string serviceId, CreateQueryTemplateDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
    
        var repo = await GetRepositoryFromFactory(serviceId);
        if (repo == null)
        {
            return NotFound(new { message = $"Service with ID '{serviceId}' not found in configuration." });
        }
    
        var existingTemplate = await _templateService.GetTemplateByIdAsync(repo, createDto.Id);
        if (existingTemplate != null)
        {
            return Conflict($"A template with ID '{createDto.Id}' already exists in service '{serviceId}'.");
        }
    
        var template = MapFromCreateDtoToEntity(createDto);
        await _templateService.CreateTemplateAsync(repo, template);
    
        var responseDto = MapToDto(template);
        return CreatedAtRoute("GetTemplateById", new { serviceId = serviceId, id = responseDto.Id }, responseDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string serviceId, string id, UpdateQueryTemplateDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
    
        var repo = await GetRepositoryFromFactory(serviceId);
        if (repo == null)
        {
            return NotFound(new { message = $"Service with ID '{serviceId}' not found in configuration." });
        }
        
        var existingTemplate = await _templateService.GetTemplateByIdAsync(repo, id);
        if (existingTemplate is null)
        {
            return NotFound(new { message = $"Template with ID '{id}' not found in service '{serviceId}'." });
        }
    
        var templateToUpdate = MapFromUpdateDtoToEntity(id, updateDto);
        await _templateService.UpdateTemplateAsync(repo, id, templateToUpdate);
    
        return NoContent(); // Standard response for a successful PUT
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string serviceId, string id)
    {
        var repo = await GetRepositoryFromFactory(serviceId);
        if (repo == null)
        {
            return NotFound(new { message = $"Service with ID '{serviceId}' not found in configuration." });
        }
    
        var existingTemplate = await _templateService.GetTemplateByIdAsync(repo, id);
        if (existingTemplate is null)
        {
            return NotFound(new { message = $"Template with ID '{id}' not found in service '{serviceId}'." });
        }
    
        await _templateService.DeleteTemplateAsync(repo, id);
    
        return NoContent(); // Standard response for a successful DELETE
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
                IsDynamicFilter = dto.Template.IsDynamicFilter
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
                IsDynamicFilter = dto.Template.IsDynamicFilter
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
                IsDynamicFilter = template.Template.IsDynamicFilter
            }
        };
    }
}