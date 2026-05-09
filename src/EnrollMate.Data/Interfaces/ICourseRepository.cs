using EnrollMate.Shared.Models;

namespace EnrollMate.Data.Interfaces;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(string courseId);
    Task<IEnumerable<Course>> GetAllAsync();
    Task<IEnumerable<Course>> GetByYearLevelAsync(int yearLevel);
    Task IncrementEnrolledCountAsync(string courseId);
    Task IncrementWaitlistedCountAsync(string courseId);
}
