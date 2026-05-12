using EnrollMate.Data.Interfaces;
using EnrollMate.Shared.Models;

namespace EnrollMate.Data.Mock;

public class MockSchoolRepository(List<School> schools) : ISchoolRepository
{
    public Task<School?> GetByIdAsync(string schoolId)
    {
        var result = schools.FirstOrDefault(s => s.Id == schoolId);
        return Task.FromResult(result);
    }

    public Task<IEnumerable<School>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<School>>(schools);
    }

    public Task<IEnumerable<School>> GetByYearLevelAsync(int yearLevel)
    {
        var result = schools.Where(s => s.ServesYearLevel(yearLevel));
        return Task.FromResult<IEnumerable<School>>(result);
    }

    public Task IncrementEnrolledCountAsync(string schoolId)
    {
        var school = schools.FirstOrDefault(s => s.Id == schoolId);
        if (school is not null)
            school.EnrolledCount++;

        return Task.CompletedTask;
    }

    public Task IncrementWaitlistedCountAsync(string schoolId)
    {
        var school = schools.FirstOrDefault(s => s.Id == schoolId);
        if (school is not null)
            school.WaitlistedCount++;

        return Task.CompletedTask;
    }
}
