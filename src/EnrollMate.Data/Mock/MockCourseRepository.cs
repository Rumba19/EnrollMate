using EnrollMate.Data.Interfaces;
using EnrollMate.Shared.Models;

namespace EnrollMate.Data.Mock;

public class MockCourseRepository : ICourseRepository
{
    private readonly List<Course> _courses;

    public MockCourseRepository(List<Course> courses)
    {
        _courses = courses;
    }

    public Task<Course?> GetByIdAsync(string courseId)
    {
        var result = _courses.FirstOrDefault(c => c.Id == courseId);
        return Task.FromResult(result);
    }

    public Task<IEnumerable<Course>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Course>>(_courses);
    }

    public Task<IEnumerable<Course>> GetByYearLevelAsync(int yearLevel)
    {
        var result = _courses.Where(c => c.EligibleYearLevels.Contains(yearLevel));
        return Task.FromResult<IEnumerable<Course>>(result);
    }

    public Task IncrementEnrolledCountAsync(string courseId)
    {
        var course = _courses.FirstOrDefault(c => c.Id == courseId);
        if (course is not null)
            course.EnrolledCount++;

        return Task.CompletedTask;
    }

    public Task IncrementWaitlistedCountAsync(string courseId)
    {
        var course = _courses.FirstOrDefault(c => c.Id == courseId);
        if (course is not null)
            course.WaitlistedCount++;

        return Task.CompletedTask;
    }
}
