using Microsoft.EntityFrameworkCore;
using DocumentManagementSystem.DAL;
using Microsoft.AspNetCore.Mvc;
using DocumentManagementSystem.Models;
using DocumentManagementSystem.Services;
using DocumentManagementSystem.Exceptions;

namespace DocumentManagementSystem.Controllers;

[Route("api/documentmanagement")]
[ApiController]
public class APIController : ControllerBase
{
    private readonly AppDbContext _appDbContext;
    private readonly RabbitMqPublisher _publisher;
    private readonly ILogger<APIController> _logger;

    public APIController(AppDbContext appDbContext, RabbitMqPublisher publisher, ILogger<APIController> logger)
    {
        _appDbContext = appDbContext;
        _publisher = publisher;
        _logger = logger;
    }

    /*[HttpPost]
    public async Task<ActionResult<Document>> CreateDocument([FromBody] Document document)
    {
        _appDbContext.Documents.Add(document);
        await _appDbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDocumentById), new { id = document.Id }, document);
    }*/

    [HttpPost]
    public async Task<ActionResult<Document>> CreateDocument([FromBody] Document document)
    {
        _appDbContext.Documents.Add(document);
        await _appDbContext.SaveChangesAsync();

        var message = new DocumentMessage
        {
            DocumentId = document.Id,
            Title = document.Title,
            Format = document.Format
        };
        await _publisher.PublishDocumentAsync(message);

        return CreatedAtAction(nameof(GetDocumentById), new { id = document.Id }, document);
    }

    [HttpGet]
    public async Task<ActionResult<List<Document>>> GetDocument()
    {
        return Ok(await _appDbContext.Documents.ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Document>> GetDocumentById(int id)
    {
        var document = await _appDbContext.Documents.FindAsync(id);

        if (document == null)
        {
            return NotFound();
        }

        return Ok(document);
    }


    /*[HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateDocument(int id, [FromBody] Document updatedDocument)
    {
        var document = await _appDbContext.Documents.FindAsync(id);

        if (document == null)
        {
            return NotFound();
        }

        document.Title = updatedDocument.Title;

        await _appDbContext.SaveChangesAsync();

        return NoContent();
    }*/

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateDocument(int id, [FromBody] Document updatedDocument)
    {
        var document = await _appDbContext.Documents.FindAsync(id);

        if (document == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(updatedDocument.Title))
            document.Title = updatedDocument.Title;

        if (!string.IsNullOrWhiteSpace(updatedDocument.Format))
            document.Format = updatedDocument.Format;

        try
        {
            await _appDbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new { error = ex.InnerException?.Message ?? ex.Message });
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        var document = await _appDbContext.Documents.FindAsync(id);

        if (document == null)
        {
            return NotFound();
        }

        _appDbContext.Documents.Remove(document);
        await _appDbContext.SaveChangesAsync();

        return NoContent();
    }
}