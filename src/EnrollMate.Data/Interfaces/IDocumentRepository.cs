using EnrollMate.Shared.Models;

namespace EnrollMate.Data.Interfaces;

public interface IDocumentRepository
{
    Task<IEnumerable<Document>> GetByApplicationIdAsync(string applicationId);
    Task<Document?> GetByIdAsync(string documentId);
    Task AddAsync(Document document);
    Task MarkUploadedAsync(string documentId, string fileName, string storagePath);
}
