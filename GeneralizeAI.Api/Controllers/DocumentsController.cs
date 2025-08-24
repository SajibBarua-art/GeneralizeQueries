// GeneralizeAI.Api/Controllers/DocumentsController.cs

using System.Text.Json;
using GeneralizeAI.Api.DTOs;
using GeneralizeAI.Core.Entities;
using GeneralizeAI.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace GeneralizeAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IGenericDocumentService _documentService;

    public DocumentsController(IGenericDocumentService documentService)
    {
        _documentService = documentService;
    }
    
    // A private helper method to perform the mapping
    private DocumentResponseDto MapToDto(GenericDocument doc)
    {
        return new DocumentResponseDto
        {
            Id = doc.Id!,
            Name = doc.Name,
            CreatedAt = doc.CreatedAt,
            // Convert BsonDocument back to a Dictionary<string, object>
            // This is the reverse of what we do in the Create method
            Data = BsonSerializer.Deserialize<Dictionary<string, object>>(doc.Data)
        };
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var documents = await _documentService.GetAllDocumentsAsync();
        var dtos = documents.Select(MapToDto);
        return Ok(dtos);
    }

    [HttpGet("{id:length(24)}", Name = "GetDocumentById")]
    public async Task<ActionResult<DocumentResponseDto>> GetById(string id)
    {
        var document = await _documentService.GetDocumentByIdAsync(id);
        
        if (document is null)
        {
            return NotFound();
        }

        return Ok(MapToDto(document)); // Use the helper method
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateDocumentDto dto)
    {
        var jsonData = JsonSerializer.Serialize(dto.Data);
        var bsonData = BsonSerializer.Deserialize<BsonDocument>(jsonData);

        var document = new GenericDocument
        {
            Name = dto.Name,
            Data = bsonData
        };

        await _documentService.CreateDocumentAsync(document);

        // Map the created document to the response DTO before returning
        var responseDto = MapToDto(document);

        return CreatedAtRoute("GetDocumentById", new { id = responseDto.Id }, responseDto);
    }
    
    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var success = await _documentService.DeleteDocumentAsync(id);
        return success ? NoContent() : NotFound();
    }
}