using DocumentManagementSystem.DAL;
using Microsoft.AspNetCore.Mvc;
using DocumentManagementSystem.Models;
namespace DocumentManagementSystem.Controllers;

[Route("api/documentmanagement")]
[ApiController]
public class APIController : ControllerBase
{
    private readonly AppDbContext _appDbContext;
    public APIController(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    [HttpPost]
    public IActionResult CreateDocument([FromBody] Document document)
    {
        _appDbContext.Documents.Add(document);
        _appDbContext.SaveChanges();
        return CreatedAtAction(nameof(CreateDocument), new { id = document.Id }, document);
    }

    [HttpGet]
    public IActionResult GetDocument()
    {
        return Ok(_appDbContext.Documents.ToList());
    }
    
    //TODO: Bring the endpoints to the Endpoints file 
}