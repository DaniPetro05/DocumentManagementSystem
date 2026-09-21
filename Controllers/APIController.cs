using Microsoft.AspNetCore.Mvc;
using DocumentManagementSystem.Models;
namespace DocumentManagementSystem.Controllers;

[Route("api/documentmanagement")]
[ApiController]
public class APIController : ControllerBase
{
    //Dummy object for validity
    static private Document _document = new Document
    {
        Id = 1,
        Title = "Document Title",
        Format = "Document Format"
    };

    [HttpGet]
    public IActionResult GetDocument()
    {
        return Ok(_document);
    }
}