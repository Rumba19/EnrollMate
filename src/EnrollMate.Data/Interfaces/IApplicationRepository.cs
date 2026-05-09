using EnrollMate.Shared.Enums;
using EnrollMate.Shared.Models;

namespace EnrollMate.Data.Interfaces;

public interface IApplicationRepository
{
    Task<Application?> GetByIdAsync(string applicationId);
    Task<IEnumerable<Application>> GetAllAsync();
    Task<IEnumerable<Application>> GetPendingAsync();
    Task AddAsync(Application application);
    Task UpdateStatusAsync(string applicationId, ApplicationStatus status);
    Task UpdateAsync(Application application);
}
