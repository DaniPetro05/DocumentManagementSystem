using Microsoft.EntityFrameworkCore;

namespace DocumentManagementSystem.Endpoints;
using DocumentManagementSystem.Models;

public static class DocumentEndpoints
{
    private static readonly List<Document> _documents = new();
    public static void MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/documents", (Document sampleDocument) =>
        {
            _documents.Add(sampleDocument);
            return Results.Created($"/documents/{sampleDocument.Id}", sampleDocument); //201
        });
        app.MapGet("/documents", () => Results.Ok(_documents));
    }
}