using EnrollMate.Shared.Models;

namespace EnrollMate.Data.Interfaces;

public interface ISchoolRepository
{
    Task<School?> GetByIdAsync(string schoolId);
    Task<IEnumerable<School>> GetAllAsync();
    Task<IEnumerable<School>> GetByYearLevelAsync(int yearLevel);
    Task IncrementEnrolledCountAsync(string schoolId);
    Task IncrementWaitlistedCountAsync(string schoolId);
}
