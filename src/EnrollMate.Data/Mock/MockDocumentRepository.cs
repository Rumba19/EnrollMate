using EnrollMate.Data.Interfaces;
using EnrollMate.Shared.Models;

namespace EnrollMate.Data.Mock;

public class MockDocumentRepository : IDocumentRepository
{
    private readonly List<Document> _documents;

    public MockDocumentRepository(List<Document> documents)
    {
        _documents = documents;
    }

    public Task<IEnumerable<Document>> GetByApplicationIdAsync(string applicationId)
    {
        var result = _documents.Where(d => d.ApplicationId == applicationId);
        return Task.FromResult<IEnumerable<Document>>(result);
    }

    public Task<Document?> GetByIdAsync(string documentId)
    {
        var result = _documents.FirstOrDefault(d => d.Id == documentId);
        return Task.FromResult(result);
    }

    public Task AddAsync(Document document)
    {
        _documents.Add(document);
        return Task.CompletedTask;
    }

    public Task MarkUploadedAsync(string documentId, string fileName, string storagePath)
    {
        var document = _documents.FirstOrDefault(d => d.Id == documentId);
        if (document is not null)
        {
            document.IsUploaded = true;
            document.FileName = fileName;
            document.StoragePath = storagePath;
            document.UploadedAt = DateTime.UtcNow;
        }

        return Task.CompletedTask;
    }
}
